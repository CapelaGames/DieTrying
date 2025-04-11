using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class CollisionResultExitBufferTagAuthoring : MonoBehaviour
{

}

public struct CollisionResultExitBufferTag : IComponentData { }

public class CollisionResultExitBufferTagAuthoringBaker : Baker<CollisionResultExitBufferTagAuthoring>
{
    public override void Bake(CollisionResultExitBufferTagAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent<CollisionResultExitBufferTag>(entity);
    }
}
