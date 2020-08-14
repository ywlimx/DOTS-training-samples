﻿using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

public class LakeRefillSystem : SystemBase
{
    
    protected override void OnUpdate()
    {
        Entities
        .WithName("bucket_refill_lake")
        .WithAll<WaterRefill>()
        .ForEach((ref WaterAmount lakeWaterAmount) =>
        {
            // hardcode setting
            var refillRate = 1.0f;

            if(lakeWaterAmount.Value < lakeWaterAmount.MaxAmount)
            {
                lakeWaterAmount.Value += refillRate;
            }
        }).ScheduleParallel();
    }
}


