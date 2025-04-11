using Latios;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class SignalListenerAuthoring : MonoBehaviour
{
}

public struct SignalListener : IComponentData
{
    public EntityWithBuffer<SignalBufferElement> eventEntity;
    public bool eventFired;
}

public class SignalListenerAuthoringBaker : Baker<SignalListenerAuthoring>
{
    public override void Bake(SignalListenerAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent<SignalListener>(entity);
    }
}
