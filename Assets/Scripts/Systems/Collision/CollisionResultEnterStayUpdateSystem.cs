using Latios;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Latios.Transforms;
using static Unity.Entities.SystemAPI;
[BurstCompile, UpdateInGroup(typeof(PresentationSystemGroup))]
public partial struct CollisionResultEnterStayUpdateSystem : ISystem
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
        new CollisionResultEnterStayUpdateJob
        {
            resultBufferLookup = GetBufferLookup<CollisionResultBufferElement>(false),
            entityStorageInfoLookup = GetEntityStorageInfoLookup(),
        }.ScheduleParallel();
    }

    [BurstCompile]
    partial struct CollisionResultEnterStayUpdateJob : IJobEntity
    {
        [NativeDisableParallelForRestriction]
        public BufferLookup<CollisionResultBufferElement> resultBufferLookup;
        public EntityStorageInfoLookup entityStorageInfoLookup;
        public void Execute(Entity entity, in CollisionResultExitBufferPtr exitBufferPtr)
        {
            if (!resultBufferLookup.HasBuffer(entity))
                return;
            if (!resultBufferLookup.HasBuffer(exitBufferPtr.sourceEntity))
                return;
            var resultBuffer = resultBufferLookup[entity];
            for (int i = resultBuffer.Length - 1; i >= 0; i--)
            {
                var result = resultBuffer[i];

                //CollisionResultDebug.EntityAndFrames(result);

                if (!entityStorageInfoLookup.Exists(result.hitEntity))
                {
                    var exitBuffer = resultBufferLookup[exitBufferPtr.sourceEntity];
                    exitBuffer.Add(result);
                    resultBuffer.RemoveAt(i);
                    continue;
                }
                if (result.previousFramesCollidedFor != result.framesCollidedFor)
                {
                    result.previousFramesCollidedFor = result.framesCollidedFor;
                    resultBuffer[i] = result;
                }
                else
                {
                    //add to exit buffer
                    var exitBuffer = resultBufferLookup[exitBufferPtr.sourceEntity];
                    exitBuffer.Add(result);
                    resultBuffer.RemoveAt(i);
                    continue;
                }
            }
        }
    }
}
