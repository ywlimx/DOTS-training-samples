﻿using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Fire
{
    [GenerateAuthoringComponent]
    public struct FireGridSpawner : IComponentData
    {
        public Entity Prefab;
        public int CountX;
        public int CountZ;
        public float3 Origin;
    }

    public struct Initialized : IComponentData { };

    public class FireGridSpawnerSystem : SystemBase
    {
        private EntityQuery UninitializedSpawners;

        struct SpawnerInfo
        {
            public Entity Prefab;
            public int CountX;
            public int CountZ;
            public int TotalCount;
            public float3 Center;
        }

        protected override void OnCreate()
        {
            RequireForUpdate(UninitializedSpawners);
        }

        protected override void OnUpdate()
        {
            // Check if we have initialized
            EntityQuery queryGroup = GetEntityQuery(typeof(Initialized));
            if (queryGroup.CalculateEntityCount() > 0)
            {
                return;
            }

            var spawners = new NativeArray<SpawnerInfo>(UninitializedSpawners.CalculateEntityCount(), Allocator.TempJob);

            Entities.WithStoreEntityQueryInField(ref UninitializedSpawners).WithNone<Initialized>()
                .ForEach((int entityInQueryIndex, ref FireGridSpawner spawner) =>
                {
                    spawners[entityInQueryIndex] = new SpawnerInfo
                    {
                        Prefab = spawner.Prefab,
                        CountX = spawner.CountX,
                        CountZ = spawner.CountZ,
                        TotalCount = spawner.CountX * spawner.CountZ,
                        Center = spawner.Origin
                    };
                }).Run();

            foreach (var spawner in spawners)
            {
                EntityManager.Instantiate(spawner.Prefab, spawner.TotalCount, Allocator.Temp);
            }

            EntityManager.AddComponent<Initialized>(UninitializedSpawners);

            var fireBufferEntity = GetSingletonEntity<FireBuffer>();

            SpawnerInfo spanwerInfoInstance = spawners[0];
            EntityManager.AddComponentData(fireBufferEntity, new FireBufferMetaData
            {
                CountX = spanwerInfoInstance.CountX,
                CountZ = spanwerInfoInstance.CountZ,
                TotalSize = spanwerInfoInstance.TotalCount
            });

            UnityEngine.Debug.Log($"FireGridSpawner initializing grid [{spanwerInfoInstance.CountX}, {spanwerInfoInstance.CountZ}] Total: {spanwerInfoInstance.TotalCount}");

            // Grab Fire buffer
            var gridBufferLookup = GetBufferFromEntity<FireBufferElement>();
            var gridBuffer = gridBufferLookup[fireBufferEntity];
            gridBuffer.ResizeUninitialized(spanwerInfoInstance.TotalCount);
            var gridArray = gridBuffer.AsNativeArray();

            Random m_Random = new Random(0x1234567);
            Entities
                .WithDeallocateOnJobCompletion(spawners)
                .ForEach((Entity fireEntity, int entityInQueryIndex, ref Translation translation, in BoundsComponent bounds) =>
                {
                    for (int i = 0; i < spawners.Length; ++i)
                    {
                        var spawner = spawners[i];
                        if (entityInQueryIndex < spawner.TotalCount)
                        {
                            int x = entityInQueryIndex % spawner.CountX;
                            int z = entityInQueryIndex / spawner.CountX;
                            var posX = bounds.SizeXZ * (x - (spawner.CountX - 1) / 2);
                            var posZ = bounds.SizeXZ * (z - (spawner.CountZ - 1) / 2);
                            // Add random offset on y to debug that the grid spacing is correct
                            var posY = m_Random.NextFloat(-0.01f, 0.01f);
                            translation.Value = spawner.Center + new float3(posX, posY, posZ);

                            gridArray[entityInQueryIndex] = new FireBufferElement { FireEntity = fireEntity };
                            break;
                        }
                        entityInQueryIndex -= spawner.TotalCount;
                    }
                }).ScheduleParallel();
        }
    }
}

/*
 * Entity for all fires (Grid storage)
 * On that entity we can add a dynamic buffer component (native list, attached to an entity)
 * new struct IBufferElementData (entity fire)
 *
 * As we initialize this list - make sure all the entity references are correct
 * - Can access the singleton reference
 * - Or we can pass the native array into jobs to work with
*/
