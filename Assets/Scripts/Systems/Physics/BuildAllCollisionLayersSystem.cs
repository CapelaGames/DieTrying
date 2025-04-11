//This is only for david's game?
using Latios;
using Latios.Psyshock;
using Latios.Transforms;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
public partial struct BuildAllCollisionLayersSystems : ISystem, ISystemNewScene
{
    BuildCollisionLayerTypeHandles typeHandles;
    LatiosWorldUnmanaged latiosWorld;

    EntityQuery scoreTriggerQuery;
    EntityQuery projectileQuery;
    EntityQuery areaOfEffectQuery;
    EntityQuery ballQuery;
    EntityQuery towerViewQuery;
    EntityQuery staticEnvironmentQuery;
    EntityQuery pinQuery;

    EntityQuery buttonQuery;
    EntityQuery draggableQuery;
    EntityQuery droppableQuery;

    EntityQuery towerQuery;
    EntityQuery beamQuery;
    EntityQuery beamConnectorQuery;

    EntityQuery damageQuery;
    EntityQuery effectorQuery;
    EntityQuery areaTriggerQuery;
    EntityQuery huntersMarkQuery;

    EntityQuery huntingLodgeQuery;

    EntityQuery targetingZoneQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        latiosWorld = state.GetLatiosWorldUnmanaged();
        //scoreTriggerQuery = state.Fluent().With<ScoreTrigger>(true).PatchQueryForBuildingCollisionLayer().Build();
        //projectileQuery = state.Fluent().With<ProjectileTag>(true).PatchQueryForBuildingCollisionLayer().Build();
        //areaOfEffectQuery = state.Fluent().With<AreaOfEffectTag>(true).PatchQueryForBuildingCollisionLayer().Build();
        //ballQuery = state.Fluent().With<Ball>(true).PatchQueryForBuildingCollisionLayer().Build();
        //towerViewQuery = state.Fluent().With<TowerCurrentTarget>(true).PatchQueryForBuildingCollisionLayer().Build();
        staticEnvironmentQuery = state.Fluent().With<StaticEnvironmentTag>(true).PatchQueryForBuildingCollisionLayer().Build();
        //pinQuery = state.Fluent().With<PinTag>(true).PatchQueryForBuildingCollisionLayer().Build();
        //buttonQuery = state.Fluent().With<ButtonTag>(true).PatchQueryForBuildingCollisionLayer().Build();
        //draggableQuery = state.Fluent().With<DraggableTag>(true).PatchQueryForBuildingCollisionLayer().Build();
        //droppableQuery = state.Fluent().With<DroppableTag>(true).PatchQueryForBuildingCollisionLayer().Build();
        //towerQuery = state.Fluent().With<TowerTag, Parent>(true).PatchQueryForBuildingCollisionLayer().Build();
        //beamQuery = state.Fluent().With<BeamTag>().PatchQueryForBuildingCollisionLayer().Build();
        //beamConnectorQuery = state.Fluent().With<BeamConnectorTag>().PatchQueryForBuildingCollisionLayer().Build();
        //
        //ensure towers with damage information on them dont get included for example BeamConnector
        //damageQuery = state.Fluent().With<Damage>(true).Without<TowerTag>().PatchQueryForBuildingCollisionLayer().Build();
        //effectorQuery = state.Fluent().With<Effector>(true).PatchQueryForBuildingCollisionLayer().Build();
        //areaTriggerQuery = state.Fluent().With<AreaTriggerTag>(true).PatchQueryForBuildingCollisionLayer().Build();

        typeHandles = new BuildCollisionLayerTypeHandles(ref state);
    }

    [BurstCompile]
    public void OnNewScene(ref SystemState state)
    {
        // latiosWorld.sceneBlackboardEntity.AddOrSetCollectionComponentAndDisposeOld(new ScoreTriggerCollisionLayer());
        // latiosWorld.sceneBlackboardEntity.AddOrSetCollectionComponentAndDisposeOld(new ProjectileCollisionLayer());
        // latiosWorld.sceneBlackboardEntity.AddOrSetCollectionComponentAndDisposeOld(new AreaOfEffectCollisionLayer());
        // latiosWorld.sceneBlackboardEntity.AddOrSetCollectionComponentAndDisposeOld(new BallCollisionLayer());
        // latiosWorld.sceneBlackboardEntity.AddOrSetCollectionComponentAndDisposeOld(new TowerViewCollisionLayer());
        latiosWorld.sceneBlackboardEntity.AddOrSetCollectionComponentAndDisposeOld(new StaticEnvironmentCollisionLayer());
        // latiosWorld.sceneBlackboardEntity.AddOrSetCollectionComponentAndDisposeOld(new PinCollisionLayer());
        // latiosWorld.sceneBlackboardEntity.AddOrSetCollectionComponentAndDisposeOld(new ButtonCollisionLayer());
        // latiosWorld.sceneBlackboardEntity.AddOrSetCollectionComponentAndDisposeOld(new DraggableCollisionLayer());
        // latiosWorld.sceneBlackboardEntity.AddOrSetCollectionComponentAndDisposeOld(new DroppableCollisionLayer());
        // latiosWorld.sceneBlackboardEntity.AddOrSetCollectionComponentAndDisposeOld(new TowerCollisionLayer());
        // latiosWorld.sceneBlackboardEntity.AddOrSetCollectionComponentAndDisposeOld(new BeamCollisionLayer());
        // latiosWorld.sceneBlackboardEntity.AddOrSetCollectionComponentAndDisposeOld(new BeamConnectorCollisionLayer());
        // latiosWorld.sceneBlackboardEntity.AddOrSetCollectionComponentAndDisposeOld(new DamageCollisionLayer());
        // latiosWorld.sceneBlackboardEntity.AddOrSetCollectionComponentAndDisposeOld(new EffectorCollisionLayer());
        // latiosWorld.sceneBlackboardEntity.AddOrSetCollectionComponentAndDisposeOld(new AreaTriggerCollisionLayer());
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        typeHandles.Update(ref state);
        CollisionLayerSettings settings = BuildCollisionLayerConfig.defaultSettings;

        state.Dependency = Physics.BuildCollisionLayer(staticEnvironmentQuery, in typeHandles).WithSettings(settings)
            .ScheduleParallel(out CollisionLayer staticEnvironmentLayer, Allocator.Persistent, state.Dependency);
        latiosWorld.sceneBlackboardEntity.SetCollectionComponentAndDisposeOld(new StaticEnvironmentCollisionLayer
        {
            layer = staticEnvironmentLayer
        });

    }
}
