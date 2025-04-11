using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class SignalListenerDebugAuthoring : MonoBehaviour
{
    public string debugText;
}

public struct SignalListenerDebug : IComponentData
{
    public FixedString128Bytes debugText;
}

public class SignalListenerDebugAuthoringBaker : Baker<SignalListenerDebugAuthoring>
{
    public override void Bake(SignalListenerDebugAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new SignalListenerDebug
        {
            debugText = authoring.debugText,
        });
    }
}
