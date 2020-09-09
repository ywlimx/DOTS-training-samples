﻿using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class PlayerInitializationSystem : SystemBase
{
    EntityCommandBufferSystem ECBSystem;

    protected override void OnCreate()
    {
        base.OnCreate();
        ECBSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var ecb = ECBSystem.CreateCommandBuffer().AsParallelWriter();
        Entities.ForEach((int entityInQueryIndex, in Entity playerInitializationEntity, in PlayerInitialization playerInitialization) => {

            for (int i = 0; i < playerInitialization.PlayerCount; i++)
            {
                var playerEntity = ecb.Instantiate(entityInQueryIndex, playerInitialization.PlayerPrefab);
                if (i == 0)
                {
                    ecb.AddComponent<HumanPlayerTag>(entityInQueryIndex, playerEntity);

                    var playerArrowPreviewEntity = ecb.Instantiate(entityInQueryIndex, playerInitialization.HumanPlayerArrowPreview);
                    ecb.AddComponent<HumanPlayerTag>(entityInQueryIndex, playerArrowPreviewEntity);
                    ecb.AddComponent<PositionXZ>(entityInQueryIndex, playerArrowPreviewEntity);
                    ecb.AddComponent<Direction>(entityInQueryIndex, playerArrowPreviewEntity);
                }
            }
            ecb.AddComponent<Disabled>(entityInQueryIndex, playerInitializationEntity);    
        }).ScheduleParallel();
        ECBSystem.AddJobHandleForProducer(Dependency);
    }
}