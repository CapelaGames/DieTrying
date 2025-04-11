using Latios;
using Latios.Psyshock;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class CollisionResultAuthoring : MonoBehaviour
{
}

public struct CollisionResultBufferElement : IBufferElementData
{
    public Entity entity;
    public Entity hitEntity;
    public ColliderDistanceResult colliderDistanceResult;
    public int previousFramesCollidedFor;
    public int framesCollidedFor;
}

public class CollisionResultAuthoringBaker : Baker<CollisionResultAuthoring>
{
    public override void Bake(CollisionResultAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);
        AddBuffer<CollisionResultBufferElement>(entity);
    }
}

public static class CollisionResultDebug
{
    public static void EntityAndFrames(CollisionResultBufferElement result)
    {
        UnityEngine.Debug.LogError($"{result.hitEntity.Index}:{result.hitEntity.Version} | prevFrame = {result.previousFramesCollidedFor} | frame = {result.framesCollidedFor}");
    }
}
