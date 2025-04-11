using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class ForceAuthoring : MonoBehaviour
{
    public float force;
}
public struct Force : IComponentData
{
    public float force;
}
public class ForceAuthoringBaker : Baker<ForceAuthoring>
{
    public override void Bake(ForceAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new Force
        {
            force = authoring.force,
        });
    }
}
