using Latios;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class ForcePtrAuthoring : MonoBehaviour
{
    public GameObject ptrObject;
}
public struct ForcePtr : IComponentData
{
    public EntityWith<Force> sourceEntity;
}
public class ForcePtrAuthoringBaker : Baker<ForcePtrAuthoring>
{
    public override void Bake(ForcePtrAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new ForcePtr
        {
            sourceEntity = GetEntity(authoring.ptrObject, TransformUsageFlags.Dynamic),
        });
    }
}
public static class ForcePointer
{
    public static bool TryGetForce(in EntityStorageInfoLookup entityStorageInfoLookup, in ComponentLookup<Force> lookup, in ForcePtr pointer, out Force force)
    {
        force = new Force();
        if (!entityStorageInfoLookup.Exists(pointer.sourceEntity))
        {
            UnityEngine.Debug.LogError("ForcePointer.TryGetForce: pointer.sourceEntity does not exist!");
            return false;
        }
        if (!lookup.HasComponent(pointer.sourceEntity))
        {
            UnityEngine.Debug.LogError("ForcePointer.TryGetForce: pointer.sourceEntity does not have a force component!");
            return false;
        }
        force = lookup[pointer.sourceEntity];
        return true;
    }
    public static bool TryGetForce(in ComponentLookup<Force> lookup, in ForcePtr pointer, out Force force)
    {
        force = new Force();
        if (!lookup.HasComponent(pointer.sourceEntity))
        {
            UnityEngine.Debug.LogError("ForcePointer.TryGetForce: pointer.sourceEntity does not have a force component!");
            return false;
        }
        force = lookup[pointer.sourceEntity];
        return true;
    }
}
