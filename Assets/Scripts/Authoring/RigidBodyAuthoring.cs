using Latios.Psyshock;
using Latios.Transforms.Authoring;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

// Post-Jam Notes:
// These are the parameters we used to describe the rigid body, largely copied from Free Parking.
// The combining mechanism used for friction and restitution are hardcoded into the system.
// The change from Free Parking was the addition of sound for when the rigid body collides with things.

public class RigidBodyAuthoring : MonoBehaviour
{
    public float mass = 1f;
    [Range(0, 1)] public float coefficientOfFriction = 0.5f;
    [Range(0, 1)] public float coefficientOfRestitution = 0.5f;
    [Range(0, 1)] public float linearDamping = 0.05f;
    [Range(0, 1)] public float angularDamping = 0.05f;
    public bool ignoreGravity;
    public bool ignoreSimulation; //meaning we only want to affect the sim
    public float gravityStrength;

    public bool ignoreLinearMotion;
    public bool ignoreAngularX;
    public bool ignoreAngularY;
    public bool ignoreAngularZ;

    //    public bool isObstacle;
    //    public bool isBall;

    public float3 initialLinear;
    public float3 initialAngular;
}

public class RigidBodyAuthoringBaker : Baker<RigidBodyAuthoring>
{
    public override void Bake(RigidBodyAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new RigidBody
        {
            mass = new UnitySim.Mass
            {
                inverseMass = math.rcp(authoring.mass)
            },
            coefficientOfFriction = authoring.coefficientOfFriction,
            coefficientOfRestitution = authoring.coefficientOfRestitution,
            linearDamping = authoring.linearDamping,
            angularDamping = authoring.angularDamping,
            ignoreGravity = authoring.ignoreGravity,
            ignoreSimulation = authoring.ignoreSimulation,
            ignoreLinearMotion = authoring.ignoreLinearMotion,
            ignoreAngularX = authoring.ignoreAngularX,
            ignoreAngularY = authoring.ignoreAngularY,
            ignoreAngularZ = authoring.ignoreAngularZ,
            gravityStrength = authoring.gravityStrength,
            massValue = authoring.mass,
            velocity = new UnitySim.Velocity { linear = authoring.initialLinear, angular = authoring.initialAngular },
        });
        AddComponent<PreviousTransformRequest>(entity);
    }

    [BakingType]
    struct PreviousTransformRequest : IRequestPreviousTransform { }
}
