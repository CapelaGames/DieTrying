using Latios;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Latios.Transforms;
using static Unity.Entities.SystemAPI;

[BurstCompile, UpdateAfter(typeof(SignalSystem))]
public partial struct OnCollisionEnterForceSignalListenerSystem : ISystem
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
        new OnCollisionEnterForceSignalListenerJob
        {
            bodyLookup = GetComponentLookup<RigidBody>(false),
            transformLookup = GetComponentLookup<WorldTransform>(true),
            resultBufferLookup = GetBufferLookup<CollisionResultBufferElement>(true),
            forceLookup = GetComponentLookup<Force>(true),
        }.ScheduleParallel();
    }

    [BurstCompile]
    partial struct OnCollisionEnterForceSignalListenerJob : IJobEntity
    {
        [NativeDisableParallelForRestriction]
        public ComponentLookup<RigidBody> bodyLookup;
        [ReadOnly] public ComponentLookup<WorldTransform> transformLookup;
        [ReadOnly] public BufferLookup<CollisionResultBufferElement> resultBufferLookup;
        [ReadOnly] public ComponentLookup<Force> forceLookup;
        public void Execute(ref SignalListener listener, in CollisionResultEnterBufferPtr resultBufferPtr, in ForcePtr forcePtr)
        {
            if (listener.eventFired)
            {
                if (ForcePointer.TryGetForce(forceLookup, forcePtr, out var force))
                {
                    if (CollisionResultBuffer.TryGetEnterBufferFromPtr(ref resultBufferLookup, resultBufferPtr, out var resultBuffer))
                    {
                        CollisionResultBuffer.ApplyForceToCollisionResultBuffer(ref bodyLookup, transformLookup, resultBuffer, force);
                    }
                }
                listener.eventFired = false;
            }
        }
    }
}
