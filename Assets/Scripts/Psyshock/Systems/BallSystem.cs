using Latios;
using Latios.Psyshock;
using Latios.Transforms;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

using static Unity.Entities.SystemAPI;

[BurstCompile]
[RequireMatchingQueriesForUpdate]
public partial struct BallSystem : ISystem
{
    LatiosWorldUnmanaged latiosWorld;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        latiosWorld = state.GetLatiosWorldUnmanaged();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        new Job { deltaTime = Time.DeltaTime }.ScheduleParallel();
    }

    [BurstCompile]
    [WithAll(typeof(BallData))]
    partial struct Job : IJobEntity
    {
        public float deltaTime;

        public void Execute(TransformAspect transform, ref BallData ball)
        {
            transform.worldPosition += ball.Direction * ball.Speed * deltaTime;
        }
    }
}
