using Latios;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Latios.Transforms;
using static Unity.Entities.SystemAPI;
using System;

[BurstCompile]
public partial struct SignalSystem : ISystem
{
    LatiosWorldUnmanaged latiosWorld;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        latiosWorld = state.GetLatiosWorldUnmanaged();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        new SignalEventFireJob
        {
            signalLookup = GetComponentLookup<Signal>(true),
            listenerLookup = GetComponentLookup<SignalListener>(false),
        }.ScheduleParallel();
        new SignalListenerDebugJob
        {
        }.ScheduleParallel();
    }

    [BurstCompile]
    partial struct SignalEventFireJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<Signal> signalLookup;
        [NativeDisableParallelForRestriction]
        public ComponentLookup<SignalListener> listenerLookup;
        public void Execute(
            Entity entity,
            in DynamicBuffer<SignalBufferElement> signalBuffer,
            ref DynamicBuffer<SignalListenerBufferElement> listenerBuffer
            )
        {
            for (int i = 0; i < signalBuffer.Length; i++)
            {
                var signal = signalLookup[signalBuffer[i].signalEntity];
                if (!signal.signalPassed)
                    return;
            }
            for (int i = 0; i < listenerBuffer.Length; i++)
            {
                UnityEngine.Debug.LogWarning("sig event listen buff");
                if (Signals.TryGetSignalListener(listenerBuffer[i].listenerEntity, listenerLookup, out var listener))
                {
                    listener.eventEntity = entity;
                    listener.eventFired = true;
                    listenerLookup[listenerBuffer[i].listenerEntity] = listener;
                }
                else
                {
                    //UnityEngine.Debug.LogError("SignalEventFireJob: An entity in the listener buffer has no SignalListener component!");
                }
            }
        }
    }

    [BurstCompile]
    partial struct SignalListenerDebugJob : IJobEntity
    {
        public void Execute(ref SignalListener listener, in SignalListenerDebug listenerDebug)
        {
            if (listener.eventFired)
            {
                UnityEngine.Debug.LogError(listenerDebug.debugText);
                listener.eventFired = false;
            }
        }
    }
}

public static class Signals
{
    public static bool TryGetSignalBuffer(
        in Entity eventEntity,
        in BufferLookup<SignalBufferElement> signalBufferLookup,
        out DynamicBuffer<SignalBufferElement> signalBuffer)
    {
        signalBuffer = new DynamicBuffer<SignalBufferElement>();
        if (signalBufferLookup.HasBuffer(eventEntity))
        {
            signalBuffer = signalBufferLookup[eventEntity];
            return true;
        }
        return false;
    }
    /// <summary>
    /// Gets TargetBuffer from the SignalBuffer on an Event Entity. This is when one of the Signals is a TargetingZoneSignal
    /// </summary>
    /// <param name="signalBuffer"></param>
    /// <param name="targetingZoneSignalLookup"></param>
    /// <param name="targetBufferLookup"></param>
    /// <param name="targetBuffer"></param>
    /// <returns></returns>
    /*public static bool TryGetTargetBufferInSignals(
        in DynamicBuffer<SignalBufferElement> signalBuffer,
        in ComponentLookup<TargetingZoneSignal> targetingZoneSignalLookup,
        in BufferLookup<TargetBufferElement> targetBufferLookup,
        out DynamicBuffer<TargetBufferElement> targetBuffer)
    {
        targetBuffer = new DynamicBuffer<TargetBufferElement>();
        for (int i = 0; i < signalBuffer.Length; i++)
        {
            var signalEntity = signalBuffer[i].signalEntity;
            if (targetingZoneSignalLookup.HasComponent(signalEntity))
            {
                var targetingZoneSignal = targetingZoneSignalLookup[signalEntity];
                var targetingZoneEntity = targetingZoneSignal.targetingZoneEntity;
                if (targetBufferLookup.HasBuffer(targetingZoneEntity))
                {
                    targetBuffer = targetBufferLookup[targetingZoneEntity];
                    return true;
                }
            }
        }
        return false;
    }*/

    public static bool TryGetSignalListener(in Entity listenerEntity, in ComponentLookup<SignalListener> signalListenerLookup, out SignalListener listener)
    {
        listener = new SignalListener();
        if (signalListenerLookup.HasComponent(listenerEntity))
        {
            listener = signalListenerLookup[listenerEntity];
            return true;
        }
        return false;
    }
}

[Serializable]
public enum InequalityType : byte
{
    GreaterThanOrEqualTo, LessThanOrEqualTo, EqualTo, StrictlyGreaterThan, StrictlyLessThan,
}
