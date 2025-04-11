using Latios;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Latios.Transforms;
using static Unity.Entities.SystemAPI;

[BurstCompile, UpdateInGroup(typeof(PresentationSystemGroup)), UpdateAfter(typeof(CollisionResultEnterStayUpdateSystem))]
public partial struct CollisionResultExitSignalSystem : ISystem
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
        new CollisionResultExitSignalJob
        {
            resultBufferLookup = GetBufferLookup<CollisionResultBufferElement>(true),
        }.ScheduleParallel();
    }

    [BurstCompile]
    partial struct CollisionResultExitSignalJob : IJobEntity
    {
        [ReadOnly] public BufferLookup<CollisionResultBufferElement> resultBufferLookup;
        public void Execute(ref Signal signal, in CollisionResultExitBufferPtr resultBufferPtr)
        {
            if (!resultBufferLookup.HasBuffer(resultBufferPtr.sourceEntity))
                return;
            var resultBuffer = resultBufferLookup[resultBufferPtr.sourceEntity];
            signal.signalPassed = resultBuffer.Length > 0;
        }
    }
}
