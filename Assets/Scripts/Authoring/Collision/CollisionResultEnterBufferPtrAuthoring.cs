using Latios;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class CollisionResultEnterBufferPtrAuthoring : MonoBehaviour
{
    public GameObject ptrObject;
}

public struct CollisionResultEnterBufferPtr : IComponentData
{
    public EntityWithBuffer<CollisionResultBufferElement> sourceEntity;
}

public class CollisionResultEnterBufferPtrAuthoringBaker : Baker<CollisionResultEnterBufferPtrAuthoring>
{
    public override void Bake(CollisionResultEnterBufferPtrAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new CollisionResultEnterBufferPtr
        {
            sourceEntity = GetEntity(authoring.ptrObject, TransformUsageFlags.Dynamic),
        });
    }
}
