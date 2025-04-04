using Latios.Authoring;
using Latios.Psyshock;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Collider = Latios.Psyshock.Collider;

public class BallAuthoring : MonoBehaviour
{
    public float Speed;
    public float3 Direction;
}

public class BallAuthoringBaker : Baker<BallAuthoring>
{
    public override void Bake(BallAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);

        AddComponent(entity, new RigidBodyData
        {
            Speed = authoring.Speed,
            Direction = authoring.Direction,
        });
    }
}
