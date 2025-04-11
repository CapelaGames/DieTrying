using Latios;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Latios.Transforms;
using static Unity.Entities.SystemAPI;
using Latios.Psyshock;

[BurstCompile, UpdateInGroup(typeof(PresentationSystemGroup), OrderFirst = true)]
public partial struct CollisionResultSystem : ISystem
{
    LatiosWorldUnmanaged latiosWorld;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        latiosWorld = state.GetLatiosWorldUnmanaged();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        //collision layer for collision buffer
        //collision buffer vs everything else
        //plus self avoidance through a list of entities
        var staticEnvironmentCollisionLayer = latiosWorld.sceneBlackboardEntity.GetCollectionComponent<StaticEnvironmentCollisionLayer>().layer;
        //var ballCollisionLayer = latiosWorld.sceneBlackboardEntity.GetCollectionComponent<BallCollisionLayer>().layer;
        // var pinCollisionLayer = latiosWorld.sceneBlackboardEntity.GetCollectionComponent<PinCollisionLayer>().layer;
        //var collisionResultPinVsBallProcessor = new CollisionResultPinVsBallProcessor
        //{
        //    bodyLookup = GetComponentLookup<RigidBody>(),
        //    collisionResultPtrLookup = GetComponentLookup<CollisionResultPtr>(),
        //    collisionResultBufferLookup = GetBufferLookup<CollisionResultBufferElement>(false),
        //};
        //state.Dependency = Physics.FindPairs(pinCollisionLayer, ballCollisionLayer, collisionResultPinVsBallProcessor).ScheduleParallel(state.Dependency);
        var collisionProcessor = new CollisionProcessor
        {
            bodyLookup = GetComponentLookup<RigidBody>(),
            enterBufferPtrLookup = GetComponentLookup<CollisionResultEnterBufferPtr>(),
            stayBufferPtrLookup = GetComponentLookup<CollisionResultStayBufferPtr>(),
            resultBufferLookup = GetBufferLookup<CollisionResultBufferElement>(false),
        };
        //state.Dependency = PhysicsDebug.DrawFindPairs(pinCollisionLayer, ballCollisionLayer).ScheduleParallel(state.Dependency);
        // state.Dependency = Physics.FindPairs(pinCollisionLayer, ballCollisionLayer, collisionProcessor).ScheduleParallel(state.Dependency);
    }

    struct CollisionProcessor : IFindPairsProcessor
    {
        public PhysicsComponentLookup<RigidBody> bodyLookup;
        public PhysicsComponentLookup<CollisionResultEnterBufferPtr> enterBufferPtrLookup;
        public PhysicsComponentLookup<CollisionResultStayBufferPtr> stayBufferPtrLookup;
        [NativeDisableParallelForRestriction]
        public BufferLookup<CollisionResultBufferElement> resultBufferLookup;
        public void Execute(in FindPairsResult result)
        {
            if (Collision.ResultDistanceBetween(result, bodyLookup, out var colliderDistanceResult))
            {
                //we do this in both directions
                CollisionResultBuffer.UpdateResultBuffers(result.entityA, result.entityB, colliderDistanceResult, enterBufferPtrLookup, stayBufferPtrLookup, ref resultBufferLookup);
                CollisionResultBuffer.UpdateResultBuffers(result.entityB, result.entityA, colliderDistanceResult.ToFlipped(), enterBufferPtrLookup, stayBufferPtrLookup, ref resultBufferLookup);
            }
        }
    }

    struct CollisionResultPinVsBallProcessor : IFindPairsProcessor
    {
        public PhysicsComponentLookup<RigidBody> bodyLookup;
        public PhysicsComponentLookup<CollisionResultPtr> collisionResultPtrLookup;
        [NativeDisableParallelForRestriction]
        public BufferLookup<CollisionResultBufferElement> collisionResultBufferLookup;
        public void Execute(in FindPairsResult result)
        {

            //if A or B has collision result buffers
            if (Collision.ResultDistanceBetween(result, bodyLookup, out var colliderDistanceResult))
            {
                UpdateResult(result.entityA, result.entityB, colliderDistanceResult);
                UpdateResult(result.entityB, result.entityA, colliderDistanceResult.ToFlipped());
            }
        }
        void UpdateResult(SafeEntity entity, SafeEntity hitEntity, ColliderDistanceResult colliderDistanceResult)
        {
            if (Collision.TryGetCollisionResultPtr(entity, collisionResultPtrLookup, out var collisionResultPtr))
            {
                if (CollisionResultPointer.TryGetCollisionResultBuffer(collisionResultBufferLookup, collisionResultPtr, out var collisionResultBuffer))
                {
                    Collision.UpdateResultBuffer(hitEntity, colliderDistanceResult, ref collisionResultBuffer);
                }
            }
        }
    }
}

public static class CollisionResultBuffer
{
    public static bool TryGetEnterBufferFromPtr(SafeEntity entity, PhysicsComponentLookup<CollisionResultEnterBufferPtr> bufferPtrLookup, ref BufferLookup<CollisionResultBufferElement> resultBufferLookup, out DynamicBuffer<CollisionResultBufferElement> resultBuffer)
    {
        resultBuffer = new DynamicBuffer<CollisionResultBufferElement>();
        if (!bufferPtrLookup.HasComponent(entity))
        {
            //UnityEngine.Debug.LogError("CollisionResultBuffer.TryGetEnterBufferFromPtr: Entity has no CollisionResultEnterBufferPtr!");
            return false;
        }
        var sourceEntity = bufferPtrLookup[entity].sourceEntity;
        if (!resultBufferLookup.HasBuffer(sourceEntity))
        {
            //UnityEngine.Debug.LogError("CollisionResultBuffer.TryGetEnterBufferFromPtr: SourceEntity has no DynamicBuffer<CollisionResultBufferElement>!");
            return false;
        }
        resultBuffer = resultBufferLookup[sourceEntity];
        return true;
    }

    public static bool TryGetEnterBufferFromPtr(ref BufferLookup<CollisionResultBufferElement> resultBufferLookup, CollisionResultEnterBufferPtr resultBufferPtr, out DynamicBuffer<CollisionResultBufferElement> resultBuffer)
    {
        resultBuffer = new DynamicBuffer<CollisionResultBufferElement>();
        if (!resultBufferLookup.HasBuffer(resultBufferPtr.sourceEntity))
        {
            return false;
        }
        resultBuffer = resultBufferLookup[resultBufferPtr.sourceEntity];
        return true;
    }

    public static bool TryGetStayBufferFromPtr(SafeEntity entity, PhysicsComponentLookup<CollisionResultStayBufferPtr> bufferPtrLookup, ref BufferLookup<CollisionResultBufferElement> resultBufferLookup, out DynamicBuffer<CollisionResultBufferElement> resultBuffer)
    {
        resultBuffer = new DynamicBuffer<CollisionResultBufferElement>();
        if (!bufferPtrLookup.HasComponent(entity))
        {
            //UnityEngine.Debug.LogError("CollisionResultBuffer.TryGetStayBufferFromPtr: Entity has no CollisionResultStayBufferPtr!");
            return false;
        }
        var sourceEntity = bufferPtrLookup[entity].sourceEntity;
        if (!resultBufferLookup.HasBuffer(sourceEntity))
        {
            //UnityEngine.Debug.LogError("CollisionResultBuffer.TryGetStayBufferFromPtr: SourceEntity has no DynamicBuffer<CollisionResultBufferElement>!");
            return false;
        }
        resultBuffer = resultBufferLookup[sourceEntity];
        return true;
    }

    public static bool TryGetResultInBuffer(Entity hitEntity, DynamicBuffer<CollisionResultBufferElement> resultBuffer, out int index)
    {
        index = -1;
        for (int i = 0; i < resultBuffer.Length; i++)
        {
            var collisionResult = resultBuffer[i];
            if (hitEntity.Equals(collisionResult.hitEntity))
            {
                index = i;
                return true;
            }
        }
        return false;
    }

    public static void UpdateResultBuffers(SafeEntity entity, SafeEntity hitEntity, ColliderDistanceResult colliderDistanceResult, PhysicsComponentLookup<CollisionResultEnterBufferPtr> enterBufferPtrLookup,
        PhysicsComponentLookup<CollisionResultStayBufferPtr> stayBufferPtrLookup, ref BufferLookup<CollisionResultBufferElement> resultBufferLookup)
    {
        if (!TryGetEnterBufferFromPtr(entity, enterBufferPtrLookup, ref resultBufferLookup, out var enterResultBuffer))
        {
            return;
        }
        if (!TryGetStayBufferFromPtr(entity, stayBufferPtrLookup, ref resultBufferLookup, out var stayResultBuffer))
        {
            return;
        }
        if (TryGetResultInBuffer(hitEntity, stayResultBuffer, out int stayIndex))
        {
            //then we in stay buffer
            var stayResult = stayResultBuffer[stayIndex];
            stayResult.framesCollidedFor++;
            stayResult.colliderDistanceResult = colliderDistanceResult;
            stayResultBuffer[stayIndex] = stayResult;
        }
        else if (TryGetResultInBuffer(hitEntity, enterResultBuffer, out int enterIndex))
        {
            //then we in enter buffer
            var enterResult = enterResultBuffer[enterIndex];
            enterResult.framesCollidedFor++;
            enterResult.colliderDistanceResult = colliderDistanceResult;
            //remove from enterResultBuffer
            enterResultBuffer.RemoveAt(enterIndex);
            //and we have to move into the stay buffer
            stayResultBuffer.Add(enterResult);
        }
        else
        {
            //then we are in neither buffer and we are adding to the enter buffer
            enterResultBuffer.Add(new CollisionResultBufferElement
            {
                entity = entity,
                hitEntity = hitEntity,
                colliderDistanceResult = colliderDistanceResult,
                previousFramesCollidedFor = -1,
                framesCollidedFor = 0,
            });
        }
    }

    public static void ApplyForceToCollisionResultBuffer(ref ComponentLookup<RigidBody> bodyLookup, in ComponentLookup<WorldTransform> transformLookup, in DynamicBuffer<CollisionResultBufferElement> resultBuffer, in Force force)
    {
        for (int i = 0; i < resultBuffer.Length; i++)
        {
            var result = resultBuffer[i];
            if (!bodyLookup.HasComponent(result.hitEntity))
            {
                continue;
            }
            var hitRigidBody = bodyLookup[result.hitEntity];
            var A = transformLookup[result.entity].worldTransform.position;
            var B = transformLookup[result.hitEntity].worldTransform.position;
            var forceDir = math.normalize(new float3(B.xy - A.xy, 0)); //assuming no Z dimension
            hitRigidBody.velocity.linear += forceDir * force.force;
            bodyLookup[result.hitEntity] = hitRigidBody;
        }
    }
    /*
        public static void ApplyDamageToCollisionResultBuffer(ref ComponentLookup<DamageTaken> damageTakenLookup, in DynamicBuffer<CollisionResultBufferElement> resultBuffer, in Damage damage)
        {
            for (int i = 0; i < resultBuffer.Length; i++)
            {
                var result = resultBuffer[i];
                if (!damageTakenLookup.HasComponent(result.hitEntity))
                {
                    continue;
                }
                var damageTaken = damageTakenLookup[result.hitEntity];
                damageTaken.Value += damage.damage;
                damageTakenLookup[result.hitEntity] = damageTaken;
            }
        }
        */
}

public static class Collision
{
    public static float GetMaxDistance(in FindPairsResult result, in PhysicsComponentLookup<RigidBody> bodyLookup)
    {
        return GetMaxDistance(result.entityA, result.entityB, in bodyLookup);
    }

    public static float GetMaxDistance(in SafeEntity entityA, in SafeEntity entityB, in PhysicsComponentLookup<RigidBody> bodyLookup)
    {
        var rigidBodyA = new RigidBody();
        var rigidBodyB = new RigidBody();
        switch (bodyLookup.HasComponent(entityA), bodyLookup.HasComponent(entityB))
        {
            case (true, true):
                rigidBodyA = bodyLookup[entityA];
                rigidBodyB = bodyLookup[entityB];
                return UnitySim.MotionExpansion.GetMaxDistance(rigidBodyA.motionExpansion, rigidBodyB.motionExpansion);
            case (true, false):
                rigidBodyA = bodyLookup[entityA];
                return UnitySim.MotionExpansion.GetMaxDistance(rigidBodyA.motionExpansion);
            case (false, true):
                rigidBodyB = bodyLookup[entityB];
                return UnitySim.MotionExpansion.GetMaxDistance(rigidBodyB.motionExpansion);
            case (false, false):
                return 0f;
        }
    }

    public static float GetMaxDistance(in Entity entityA, in Entity entityB, in ComponentLookup<RigidBody> bodyLookup)
    {
        var rigidBodyA = new RigidBody();
        var rigidBodyB = new RigidBody();
        switch (bodyLookup.HasComponent(entityA), bodyLookup.HasComponent(entityB))
        {
            case (true, true):
                rigidBodyA = bodyLookup[entityA];
                rigidBodyB = bodyLookup[entityB];
                return UnitySim.MotionExpansion.GetMaxDistance(rigidBodyA.motionExpansion, rigidBodyB.motionExpansion);
            case (true, false):
                rigidBodyA = bodyLookup[entityA];
                return UnitySim.MotionExpansion.GetMaxDistance(rigidBodyA.motionExpansion);
            case (false, true):
                rigidBodyB = bodyLookup[entityB];
                return UnitySim.MotionExpansion.GetMaxDistance(rigidBodyB.motionExpansion);
            case (false, false):
                return 0f;
        }
    }

    public static bool ResultDistanceBetween(in FindPairsResult result, in PhysicsComponentLookup<RigidBody> bodyLookup, out ColliderDistanceResult colliderDistanceResult)
    {
        if (Physics.DistanceBetween(result.colliderA, result.transformA, result.colliderB, result.transformB, GetMaxDistance(result, bodyLookup), out colliderDistanceResult))
        {
            return true;
        }
        return false;
    }

    public static bool TryGetResultInBuffer(SafeEntity entity, Entity hitEntity, ref PhysicsBufferLookup<CollisionResultBufferElement> collisionResultBufferLookup, out int index)
    {
        index = -1;
        DynamicBuffer<CollisionResultBufferElement> resultBuffer = collisionResultBufferLookup[entity];
        for (int i = 0; i < resultBuffer.Length; i++)
        {
            var collisionResult = resultBuffer[i];
            if (hitEntity.Equals(collisionResult.hitEntity))
            {
                index = i;
                return true;
            }
        }

        return false;
    }

    public static bool TryGetResultInBuffer(Entity hitEntity, in DynamicBuffer<CollisionResultBufferElement> collisionResultBuffer, out int index)
    {
        index = -1;
        for (int i = 0; i < collisionResultBuffer.Length; i++)
        {
            var collisionResult = collisionResultBuffer[i];
            if (hitEntity.Equals(collisionResult.hitEntity))
            {
                index = i;
                return true;
            }
        }
        return false;
    }

    public static void UpdateResultBuffer(SafeEntity entity, SafeEntity hitEntity, ColliderDistanceResult colliderDistanceResult, ref PhysicsBufferLookup<CollisionResultBufferElement> collisionResultBufferLookup)
    {
        if (!collisionResultBufferLookup.HasBuffer(entity))
            return;
        if (TryGetResultInBuffer(entity, hitEntity, ref collisionResultBufferLookup, out int index))
        {
            var resultBuffer = collisionResultBufferLookup[entity];
            var result = resultBuffer[index];
            result.framesCollidedFor++;
            resultBuffer[index] = result;
        }
        else
        {
            var resultBuffer = collisionResultBufferLookup[entity];
            resultBuffer.Add(new CollisionResultBufferElement
            {
                hitEntity = hitEntity,
                colliderDistanceResult = colliderDistanceResult,
                previousFramesCollidedFor = -1,
                framesCollidedFor = 0,
            });
        }
    }

    public static void UpdateResultBuffer(Entity hitEntity, ColliderDistanceResult colliderDistanceResult, ref DynamicBuffer<CollisionResultBufferElement> collisionResultBuffer)
    {
        if (TryGetResultInBuffer(hitEntity, collisionResultBuffer, out int index))
        {
            var collisionResult = collisionResultBuffer[index];
            collisionResult.framesCollidedFor++;
            collisionResultBuffer[index] = collisionResult;
        }
        else
        {
            collisionResultBuffer.Add(new CollisionResultBufferElement
            {
                hitEntity = hitEntity,
                colliderDistanceResult = colliderDistanceResult,
                previousFramesCollidedFor = -1,
                framesCollidedFor = 0,
            });
        }
    }

    public static bool TryGetCollisionResultPtr(SafeEntity entity, in PhysicsComponentLookup<CollisionResultPtr> collisionResultPtrLookup, out CollisionResultPtr collisionResultPtr)
    {
        collisionResultPtr = default;
        if (!collisionResultPtrLookup.HasComponent(entity))
        {
            return false;
        }
        collisionResultPtr = collisionResultPtrLookup[entity];
        return true;
    }
}
