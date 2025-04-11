using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class SignalAuthoring : MonoBehaviour
{
}

public struct Signal : IComponentData
{
    public bool signalPassed;
}

public class SignalAuthoringBaker : Baker<SignalAuthoring>
{
    public override void Bake(SignalAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent<Signal>(entity);
    }
}
