/*
using Latios;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Latios.Transforms;
using static Unity.Entities.SystemAPI;
[BurstCompile, UpdateAfter(typeof(SignalSystem))]
public partial struct OnCollisionEnterStayDamageSignalListenerSystem : ISystem
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
        new CollisionEnterDamageSignalListenerJob
        {
            damageTakenLookup = GetComponentLookup<DamageTaken>(false),
            damageLookup = GetComponentLookup<Damage>(true),
            resultBufferLookup = GetBufferLookup<CollisionResultBufferElement>(true),
        }.ScheduleParallel();
    }

    [BurstCompile]
    partial struct CollisionEnterDamageSignalListenerJob : IJobEntity
    {
        [NativeDisableParallelForRestriction]
        public ComponentLookup<DamageTaken> damageTakenLookup;
        [ReadOnly] public ComponentLookup<Damage> damageLookup;
        [ReadOnly] public BufferLookup<CollisionResultBufferElement> resultBufferLookup;
        public void Execute(ref SignalListener listener, CollisionResultEnterBufferPtr resultBufferPtr, in DamagePtr damagePtr)
        {
            if (listener.eventFired)
            {
                if (DamagePointer.TryGetDamage(damageLookup, damagePtr, out var damage))
                {
                    if (CollisionResultBuffer.TryGetEnterBufferFromPtr(ref resultBufferLookup, resultBufferPtr, out var resultBuffer))
                    {
                        CollisionResultBuffer.ApplyDamageToCollisionResultBuffer(ref damageTakenLookup, resultBuffer, damage);
                    }
                }
                listener.eventFired = false;
            }
        }
    }

    [BurstCompile]
    partial struct CollisionStayDamageSignalListenerJob : IJobEntity
    {
        public void Execute(ref SignalListener listener, CollisionResultStayBufferPtr resultBufferPtr, in DamagePtr damagePtr)
        {
            if (listener.eventFired)
            {

                listener.eventFired = false;
            }
        }
    }
}
*/
