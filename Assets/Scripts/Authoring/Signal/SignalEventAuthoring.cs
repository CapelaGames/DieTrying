using Latios;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class SignalEventAuthoring : MonoBehaviour
{
    public GameObject[] signals;
    public GameObject[] listeners;
}

public struct SignalBufferElement : IBufferElementData
{
    public EntityWith<Signal> signalEntity;
}

public struct SignalListenerBufferElement : IBufferElementData
{
    public EntityWith<SignalListener> listenerEntity;
}

public class SignalEventAuthoringBaker : Baker<SignalEventAuthoring>
{
    public override void Bake(SignalEventAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);
        AddBuffer<SignalBufferElement>(entity);
        foreach (var item in authoring.signals)
        {
            AppendToBuffer(entity, new SignalBufferElement
            {
                signalEntity = GetEntity(item, TransformUsageFlags.Dynamic),
            });
        }
        AddBuffer<SignalListenerBufferElement>(entity);
        foreach (var item in authoring.listeners)
        {
            AppendToBuffer(entity, new SignalListenerBufferElement
            {
                listenerEntity = GetEntity(item, TransformUsageFlags.Dynamic),
            });
        }
    }
}
