using Latios;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Latios.Transforms;
using static Unity.Entities.SystemAPI;

[BurstCompile, UpdateInGroup(typeof(PresentationSystemGroup), OrderLast = true)]
public partial struct CollisionResultExitSystem : ISystem
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
        new CollisionResultExitUpdateJob
        {
        }.ScheduleParallel();
    }

    [BurstCompile, WithAll(typeof(CollisionResultExitBufferTag))]
    partial struct CollisionResultExitUpdateJob : IJobEntity
    {
        public void Execute(ref DynamicBuffer<CollisionResultBufferElement> resultBuffer)
        {
            resultBuffer.Clear();
        }
    }
}
