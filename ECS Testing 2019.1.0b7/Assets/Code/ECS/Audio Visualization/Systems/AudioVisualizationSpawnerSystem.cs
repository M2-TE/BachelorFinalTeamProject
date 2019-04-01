using ECS.AudioVisualization.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ECS.AudioVisualization.Systems
{
	public class AudioVisualizationSpawnerSystem : JobComponentSystem
	{
		EndSimulationEntityCommandBufferSystem bufferSystem;

		protected override void OnCreateManager()
		{
			bufferSystem = World.GetOrCreateManager<EndSimulationEntityCommandBufferSystem>();
		}

		struct AudioVisualizationSpawnerJob : IJobProcessComponentDataWithEntity<AudioVisualizationSpawner, Translation, Rotation>
		{
			[ReadOnly] public EntityCommandBuffer Buffer;

			public void Execute(Entity entity, int index, [ReadOnly] ref AudioVisualizationSpawner spawner, [ReadOnly] ref Translation translation, [ReadOnly] ref Rotation rotation)
			{
				switch (spawner.SpawnMode)
				{
					default:
					//case 0: // standard
					//	for (var x = 0; x < spawner.Size.x; x++)
					//	{
					//		for (var y = 0; y < spawner.Size.y; y++)
					//		{
					//			for (var z = 0; z < spawner.Size.z; z++)
					//			{
					//				var instance = Buffer.Instantiate(spawner.PrefabEntity);

					//				Buffer.AddComponent(instance, new AudioSampleIndex { SampleIndex = x });
					//				Buffer.AddComponent(instance, new AudioAmplitude());

					//				var position = translation.Value + new float3(x, y, z);
					//				var rotation = quaternion.identity;
					//				var scale = spawner.PrefabSize;

					//				Buffer.SetComponent(instance, new Translation { Value = position });
					//				Buffer.SetComponent(instance, new Rotation { Value = rotation });
					//				Buffer.AddComponent(instance, new NonUniformScale { Value = scale });

					//				Buffer.AddComponent(instance, new AudioVisualizationInit
					//				{
					//					BasePosition = position,
					//					BaseRotation = rotation,
					//					BaseScale = scale
					//				});
					//			}
					//		}
					//	}
					//	break;

					case 1: // circular centered
						float2 centerPos = new float2(spawner.Size.x * .5f, spawner.Size.z * .5f);
						float2 currentPos;
						for (var x = 0; x < spawner.Size.x; x++)
						{
							for (var y = 0; y < spawner.Size.y; y++)
							{
								for (var z = 0; z < spawner.Size.z; z++)
								{
									currentPos = new float2(x, z);
									float dst = math.distance(currentPos, centerPos);
									if (dst > spawner.Size.x * .5f) continue;

									var instance = Buffer.Instantiate(spawner.PrefabEntity);

									Buffer.AddComponent(instance, new AudioSampleIndex { SampleIndex = (int)dst });
									Buffer.AddComponent(instance, new AudioAmplitude());

									var position = translation.Value + new float3(x, y, z);
									//var rotation = quaternion.identity;
									var scale = spawner.PrefabSize;

									Buffer.SetComponent(instance, new Translation { Value = position });
									Buffer.SetComponent(instance, new Rotation { Value = rotation.Value });
									Buffer.AddComponent(instance, new NonUniformScale { Value = scale });

									Buffer.AddComponent(instance, new AudioVisualizationInit
									{
										BasePosition = position,
										BaseRotation = rotation.Value,
										BaseScale = scale,
										LockedScaling = 1
									});
								}
							}
						}
						break;
				}

				Buffer.DestroyEntity(entity);
			}
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			var handle = new AudioVisualizationSpawnerJob
			{
				Buffer = bufferSystem.CreateCommandBuffer()
			}.ScheduleSingle(this, inputDeps);

			bufferSystem.AddJobHandleForProducer(handle);

			return handle;
		}
	}
}
