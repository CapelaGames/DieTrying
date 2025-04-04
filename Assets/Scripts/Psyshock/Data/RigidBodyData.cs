using Latios.Psyshock;
using Latios.Transforms;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct RigidBodyData : IComponentData
{
    public UnitySim.Velocity velocity;
    public UnitySim.Mass mass;
    public RigidTransform inertialPoseWorldTransform;

    public static RigidBodyData CreateInBaking(TransformQvvs worldTransform, Collider collider, float inverseMass, float3 staticStretch)
    {
        var localInertiaTensorDiagonal = UnitySim.LocalInertiaTensorFrom(in collider, staticStretch);
        var centerOfMass = UnitySim.LocalCenterOfMassFrom(in collider);
        UnitySim.ConvertToWorldMassInertia(worldTransform, localInertiaTensorDiagonal, centerOfMass, inverseMass, out var mass, out var inertialPoseWorldTransform);

        return new RigidBodyData
        {
            velocity = default,
            mass = mass,
            inertialPoseWorldTransform = inertialPoseWorldTransform,
        };
    }
}

partial struct SimJob : IJobEntity
{
    public float deltaTime;

    public void Execute(TransformAspect transform, ref RigidBodyData rigidBody)
    {
        // Apply gravity
        rigidBody.velocity.linear.y -= 9.81f * deltaTime;

        // Apply linear force of 5 N upwards
        UnitySim.ApplyFieldImpulse(ref rigidBody.velocity, in rigidBody.mass, new float3(0f, 5f, 0f) * deltaTime);

        // Apply a force of 5 N upwards offset by one meter from the entity's center
        UnitySim.ApplyImpulseAtWorldPoint(ref rigidBody.velocity,
                                            in rigidBody.mass,
                                            rigidBody.inertialPoseWorldTransform,
                                            transform.worldPosition + transform.rightDirection,
                                            new float3(0f, 5f, 0f) * deltaTime);

        // Save a copy of the old inertialPoseWorldTransform before modifying it.
        var oldInertialPoseWorldTransform = rigidBody.inertialPoseWorldTransform;

        // Update motion with 0.01f damping factors for linear and angular.
        UnitySim.Integrate(ref rigidBody.inertialPoseWorldTransform, ref rigidBody.velocity, 0.01f, 0.01f, deltaTime);

        // Apply back to entity's transform
        transform.worldTransform = UnitySim.ApplyInertialPoseWorldTransformDeltaToWorldTransform(transform.worldTransform,
                                                                                                oldInertialPoseWorldTransform,
                                                                                                rigidBody.inertialPoseWorldTransform);
    }
}

