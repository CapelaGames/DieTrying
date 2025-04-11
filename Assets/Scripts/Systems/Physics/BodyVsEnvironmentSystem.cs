using Latios;
using Latios.Psyshock;
using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

// Post-Jam Notes:
// This is the system that runs FindPairs between the rigid bodies against the environment
// and adds the collisions to the PairStream. The code is mostly copied from Free Parking.

using static Unity.Entities.SystemAPI;

[BurstCompile]
public partial struct BodyVsEnvironmentSystem : ISystem, ISystemNewScene
{
    LatiosWorldUnmanaged latiosWorld;
    Rng m_rng;
    public void OnNewScene(ref SystemState state)
    {
        m_rng = new Rng("BodyVsEnvironmentSystem" + DateTime.Now.ToString("yyyy-MM-dd h:mm:ss tt"));
    }
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        latiosWorld = state.GetLatiosWorldUnmanaged();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var pairStream = latiosWorld.worldBlackboardEntity.GetCollectionComponent<PhysicsPairStream>(false).pairStream;
        var rigidBodyLayer = latiosWorld.sceneBlackboardEntity.GetCollectionComponent<RigidBodyCollisionLayer>(true).layer;
        var bodyLookup = GetComponentLookup<RigidBody>();

        var environmentLayer = latiosWorld.sceneBlackboardEntity.GetCollectionComponent<StaticEnvironmentCollisionLayer>(true).layer;
        //var pinLayer = latiosWorld.sceneBlackboardEntity.GetCollectionComponent<PinCollisionLayer>(true).layer;

        // var ballLayer = latiosWorld.sceneBlackboardEntity.GetCollectionComponent<BallCollisionLayer>(true).layer;
        //var ballLookup = GetComponentLookup<Ball>();

        var findBodyEnvironmentProcessor = new FindBodyVsEnvironmentProcessor
        {
            bodyLookup = bodyLookup,
            pairStream = pairStream.AsParallelWriter(),
            deltaTime = Time.DeltaTime,
            inverseDeltaTime = math.rcp(Time.DeltaTime),
            rng = m_rng,
            entityStorageInfoLookup = GetEntityStorageInfoLookup(),
        };
        state.Dependency = Physics.FindPairs(in rigidBodyLayer, in environmentLayer, in findBodyEnvironmentProcessor).ScheduleParallel(state.Dependency);
        m_rng.Shuffle();
        //state.Dependency = Physics.FindPairs(in rigidBodyLayer, in pinLayer, in findBodyEnvironmentProcessor).ScheduleParallel(state.Dependency);
        //state.Dependency = Physics.FindPairs(in ballLayer, in pinLayer, in findBodyPinProcessor).ScheduleParallel(state.Dependency);

    }

    [BurstCompile]
    struct FindBodyVsEnvironmentProcessor : IFindPairsProcessor
    {
        public PhysicsComponentLookup<RigidBody> bodyLookup;
        public PairStream.ParallelWriter pairStream;
        public float deltaTime;
        public float inverseDeltaTime;
        public Rng rng;
        DistanceBetweenAllCache distanceBetweenAllCache;
        public EntityStorageInfoLookup entityStorageInfoLookup;
        public void Execute(in FindPairsResult result)
        {
            var rigidBodyA = bodyLookup[result.entityA];
            var maxDistance = UnitySim.MotionExpansion.GetMaxDistance(in rigidBodyA.motionExpansion);
            Physics.DistanceBetweenAll(result.colliderA, result.transformA, result.colliderB, result.transformB, maxDistance, ref distanceBetweenAllCache);
            foreach (var distanceResult in distanceBetweenAllCache)
            {
                if (rigidBodyA.ignoreSimulation)
                {
                    return;
                }

                var contacts = UnitySim.ContactsBetween(result.colliderA, result.transformA, result.colliderB, result.transformB, in distanceResult);
                if (contacts.contactCount < 1 || contacts.contactCount > 32)
                {
                    UnityEngine.Debug.Log($"Bad contacts: typeA: {result.colliderA.type}, typeB: {result.colliderB.type}");
                    continue;
                }

                ref var streamData = ref pairStream.AddPairAndGetRef<ContactStreamData>(result.pairStreamKey, true, false, out var pair);
                streamData.contactParameters = pair.Allocate<UnitySim.ContactJacobianContactParameters>(contacts.contactCount, NativeArrayOptions.UninitializedMemory);
                streamData.contactImpulses = pair.Allocate<float>(contacts.contactCount, NativeArrayOptions.ClearMemory);
                streamData.hitPoint = distanceResult.hitpointB;
                streamData.hit = false;
                pair.userByte = 0;

                UnitySim.BuildJacobian(streamData.contactParameters.AsSpan(),
                                       out streamData.bodyParameters,
                                       rigidBodyA.inertialPoseWorldTransform,
                                       in rigidBodyA.velocity,
                                       in rigidBodyA.mass,
                                       RigidTransform.identity,
                                       default,
                                       default,
                                       contacts.contactNormal,
                                       contacts.AsSpan(),
                                       rigidBodyA.coefficientOfRestitution,
                                       rigidBodyA.coefficientOfFriction,
                                       UnitySim.kMaxDepenetrationVelocityDynamicStatic,
                                       9.81f,
                                       deltaTime,
                                       inverseDeltaTime);

                bodyLookup[result.entityA] = rigidBodyA;
            }
        }
    }
}
