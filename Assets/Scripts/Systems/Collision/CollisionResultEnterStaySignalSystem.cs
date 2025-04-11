using Latios;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Latios.Transforms;
using static Unity.Entities.SystemAPI;

[BurstCompile, UpdateInGroup(typeof(PresentationSystemGroup)), UpdateBefore(typeof(CollisionResultEnterStayUpdateSystem))]
public partial struct CollisionResultEnterStaySignalSystem : ISystem
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
        new CollisionResultEnterSignalJob
        {
            resultBufferLookup = GetBufferLookup<CollisionResultBufferElement>(true),
        }.ScheduleParallel();
        new CollisionResultStaySignalJob
        {
            resultBufferLookup = GetBufferLookup<CollisionResultBufferElement>(true),
        }.ScheduleParallel();
    }

    [BurstCompile]
    partial struct CollisionResultEnterSignalJob : IJobEntity
    {
        [ReadOnly] public BufferLookup<CollisionResultBufferElement> resultBufferLookup;
        public void Execute(ref Signal signal, in CollisionResultEnterBufferPtr resultBufferPtr)
        {
            if (!resultBufferLookup.HasBuffer(resultBufferPtr.sourceEntity))
                return;
            var resultBuffer = resultBufferLookup[resultBufferPtr.sourceEntity];
            signal.signalPassed = resultBuffer.Length > 0;
        }
    }

    [BurstCompile]
    partial struct CollisionResultStaySignalJob : IJobEntity
    {
        [ReadOnly] public BufferLookup<CollisionResultBufferElement> resultBufferLookup;
        public void Execute(ref Signal signal, in CollisionResultStayBufferPtr resultBufferPtr)
        {
            if (!resultBufferLookup.HasBuffer(resultBufferPtr.sourceEntity))
                return;
            var resultBuffer = resultBufferLookup[resultBufferPtr.sourceEntity];
            signal.signalPassed = resultBuffer.Length > 0;
        }
    }
}
