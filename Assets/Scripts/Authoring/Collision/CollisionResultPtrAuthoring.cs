using Latios;
using Latios.Psyshock;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class CollisionResultPtrAuthoring : MonoBehaviour
{
    [Tooltip("Object which holds data for many systems")]
    public GameObject ptrObject;
}

public struct CollisionResultPtr : IComponentData
{
    public EntityWithBuffer<CollisionResultBufferElement> sourceEntity;
}

public class CollisionResultPtrAuthoringBaker : Baker<CollisionResultPtrAuthoring>
{
    public override void Bake(CollisionResultPtrAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new CollisionResultPtr
        {
            sourceEntity = GetEntity(authoring.ptrObject, TransformUsageFlags.Dynamic),
        });
    }
}

public static class CollisionResultPointer
{
    public static bool TryGetCollisionResultBuffer(in BufferLookup<CollisionResultBufferElement> collisionResultBufferLookup, in CollisionResultPtr collisionResultPtr, out DynamicBuffer<CollisionResultBufferElement> collisionResultBuffer)
    {
        collisionResultBuffer = new DynamicBuffer<CollisionResultBufferElement>();
        if (!collisionResultBufferLookup.HasBuffer(collisionResultPtr.sourceEntity))
        {
            UnityEngine.Debug.LogError("CollisionResultPointer.TryGetCollisionResultBuffer: pointer.sourceEntity does not have a DynamicBuffer<CollisionResultBufferElement> !");
            return false;
        }
        collisionResultBuffer = collisionResultBufferLookup[collisionResultPtr.sourceEntity];
        return true;
    }
    public static bool TryGetCollisionResultBuffer(EntityStorageInfoLookup entityStorageInfoLookup, in BufferLookup<CollisionResultBufferElement> collisionResultBufferLookup, in CollisionResultPtr collisionResultPtr, out DynamicBuffer<CollisionResultBufferElement> collisionResultBuffer)
    {
        collisionResultBuffer = new DynamicBuffer<CollisionResultBufferElement>();
        if (!entityStorageInfoLookup.Exists(collisionResultPtr.sourceEntity))
        {
            UnityEngine.Debug.LogError("CollisionResultPointer.TryGetCollisionResultBuffer: pointer.sourceEntity does not exist!");
            return false;
        }
        if (!collisionResultBufferLookup.HasBuffer(collisionResultPtr.sourceEntity))
        {
            UnityEngine.Debug.LogError("CollisionResultPointer.TryGetCollisionResultBuffer: pointer.sourceEntity does not have a DynamicBuffer<CollisionResultBufferElement> !");
            return false;
        }
        collisionResultBuffer = collisionResultBufferLookup[collisionResultPtr.sourceEntity];
        return true;
    }
}
