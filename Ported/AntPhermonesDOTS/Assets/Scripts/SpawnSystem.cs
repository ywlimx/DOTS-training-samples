﻿using System.Xml;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public class SpawnSystem : JobComponentSystem
{
    BeginInitializationEntityCommandBufferSystem m_EntityCommandBufferSystem;

    protected override void OnCreate()
    {
        m_EntityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var commandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent();
        var jobHandle = Entities
            .WithName("SpawnerSystem")
            .ForEach((Entity entity, int entityInQueryIndex, ref Spawner spawner) =>
            {
                var count = 3;
                for (var x = 0; x < count; ++x)
                {
                    for (var z = 0; z < count; ++z)
                    {
                        var instance = commandBuffer.Instantiate(entityInQueryIndex, spawner.Prefab);
                        var position = new float3(x * 5.0f,0,z * 5.0f);
                        commandBuffer.SetComponent(entityInQueryIndex, instance, new Translation {Value = position}); 
                    }
                }
                commandBuffer.DestroyEntity(entityInQueryIndex, entity);

            }).Schedule(inputDeps);
        m_EntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);
        return jobHandle;
    }
}