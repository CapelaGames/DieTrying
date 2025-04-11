using Latios;
using Latios.Psyshock;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

// Post-Jam Notes:
// These are the physics components. We used a single PairStream that multiple systems wrote to in sequence.
// The AnimatedScale is here and not in environment because we didn't initially plan the freezing mechanic.

public struct RigidBody : IComponentData
{
    public UnitySim.Velocity velocity;
    public UnitySim.MotionExpansion motionExpansion;
    public RigidTransform inertialPoseWorldTransform;
    public UnitySim.Mass mass;
    public UnitySim.MotionStabilizer motionStabilizer;
    public float angularExpansion;
    public int bucketIndex;
    public int numOtherSignificantBodiesInContact;
    public float coefficientOfFriction;
    public float coefficientOfRestitution;

    public float massValue;
    public float linearDamping;
    public float angularDamping;

    public bool ignoreGravity;
    public bool ignoreSimulation; //meaning we only want to affect the sim
    public float gravityStrength;

    public bool ignoreLinearMotion;
    public bool ignoreAngularX;
    public bool ignoreAngularY;
    public bool ignoreAngularZ;

    public EntityWith<Prefab> bumpSound;
}

struct AnimatedScale : IComponentData
{
    public float minScale;
    public float targetScale;
    public float scaleUpStartTime;
    public float scaleUpEndTime;
    public float currentTime;
}

partial struct RigidBodyCollisionLayer : ICollectionComponent
{
    public CollisionLayer layer;

    public JobHandle TryDispose(JobHandle inputDeps) => layer.IsCreated ? layer.Dispose(inputDeps) : inputDeps;
}

partial struct PhysicsPairStream : ICollectionComponent
{
    public PairStream pairStream;

    public JobHandle TryDispose(JobHandle inputDeps) => pairStream.Dispose(inputDeps);
}

struct ContactStreamData
{
    public UnitySim.ContactJacobianBodyParameters bodyParameters;
    public StreamSpan<UnitySim.ContactJacobianContactParameters> contactParameters;
    public StreamSpan<float> contactImpulses;
    public float3 hitPoint;
    public bool hit;
}
