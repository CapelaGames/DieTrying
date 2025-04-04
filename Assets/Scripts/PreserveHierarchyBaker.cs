using Unity.Entities;
using UnityEngine;

class PreserveHierarchyBaker : Baker<UnityEngine.Transform>
{
    public override void Bake(UnityEngine.Transform authoring)
    {
        GetEntity(TransformUsageFlags.Dynamic);

    }
}
