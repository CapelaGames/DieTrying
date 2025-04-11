using Latios;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class CollisionResultExitBufferPtrAuthoring : MonoBehaviour
{
    public GameObject ptrObject;
}

public struct CollisionResultExitBufferPtr : IComponentData
{
    public EntityWithBuffer<CollisionResultBufferElement> sourceEntity;
}

public class CollisionResultExitBufferPtrAuthoringBaker : Baker<CollisionResultExitBufferPtrAuthoring>
{
    public override void Bake(CollisionResultExitBufferPtrAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new CollisionResultExitBufferPtr
        {
            sourceEntity = GetEntity(authoring.ptrObject, TransformUsageFlags.Dynamic),
        });
    }
}
