using Latios;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class CollisionResultStayBufferPtrAuthoring : MonoBehaviour
{
    public GameObject ptrObject;
}

public struct CollisionResultStayBufferPtr : IComponentData
{
    public EntityWithBuffer<CollisionResultBufferElement> sourceEntity;
}

public class CollisionResultStayBufferPtrAuthoringBaker : Baker<CollisionResultStayBufferPtrAuthoring>
{
    public override void Bake(CollisionResultStayBufferPtrAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new CollisionResultStayBufferPtr
        {
            sourceEntity = GetEntity(authoring.ptrObject, TransformUsageFlags.Dynamic),
        });
    }
}
