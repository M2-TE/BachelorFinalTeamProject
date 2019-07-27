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

		protected override void OnCreate()
		{
			bufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
		}

		struct AudioVisualizationSpawnerJob : IJobForEachWithEntity<AudioVisualizationSpawner, Translation, Rotation>
		{
			[ReadOnly] public EntityCommandBuffer Buffer;

			public void Execute(Entity entity, int index, [ReadOnly] ref AudioVisualizationSpawner spawner, [ReadOnly] ref Translation translation, [ReadOnly] ref Rotation rotation)
			{
				switch (spawner.SpawnMode)
				{
					default:
					case 0: // standard
						for (var x = 0; x < spawner.Size.x; x++)
						{
							for (var y = 0; y < spawner.Size.y; y++)
							{
								for (var z = 0; z < spawner.Size.z; z++)
								{
									var instance = Buffer.Instantiate(spawner.PrefabEntity);

									Buffer.AddComponent(instance, new AudioSampleIndex { Value = x });
									Buffer.AddComponent(instance, new AudioAmplitude());

									var position = translation.Value + new float3(x, y, z);
									var scale = spawner.PrefabSize;

									Buffer.SetComponent(instance, new Translation { Value = position });
									Buffer.SetComponent(instance, new Rotation { Value = rotation.Value });
									Buffer.AddComponent(instance, new NonUniformScale { Value = scale });

									Buffer.AddComponent(instance, new AudioVisualizationInit
									{
										BasePosition = position,
										BaseRotation = rotation.Value,
										BaseScale = scale,
										LockedScaling = spawner.LockedScaling
									});
								}
							}
						}
						break;

					case 1: // circular centered
						float2 centerPos = new float2(spawner.Size.x * .5f, spawner.Size.z * .5f);
						float2 currentPos;
						float xLimit = spawner.Size.x * spawner.Spacing.x + 1;
						float yLimit = spawner.Size.y * spawner.Spacing.y + 1;
						float zLimit = spawner.Size.z * spawner.Spacing.z + 1;
						for (var x = 0f; x < xLimit; x += spawner.Spacing.x)
						{
							for (var y = 0f; y < yLimit; y += spawner.Spacing.y)
							{
								for (var z = 0f; z < zLimit; z += spawner.Spacing.z)
								{
									currentPos = new float2(x, z);
									float dst = math.distance(currentPos, centerPos);
									if (dst > spawner.Size.x * .5f) continue;

									var instance = Buffer.Instantiate(spawner.PrefabEntity);

									Buffer.AddComponent(instance, new AudioSampleIndex { Value = (int)dst });
									Buffer.AddComponent(instance, new AudioAmplitude());

									var position = translation.Value + new float3(x - centerPos.x, y, z - centerPos.y);
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
										LockedScaling = spawner.LockedScaling
									});
								}
							}
						}
						break;

					case 2: // cube tower
						float2 size = spawner.TowerSize;

						// South
						float step = size.x / spawner.Size.x;
						int sampleIndex = 0;
						for (float x = -size.x * .5f; x < size.x * .5f; x += step)
						{
							var instance = Buffer.Instantiate(spawner.PrefabEntity);

							Buffer.AddComponent(instance, new AudioSampleIndex { Value = sampleIndex++ });
							Buffer.AddComponent(instance, new AudioAmplitude());

							var position = translation.Value + new float3(x, spawner.Spacing.y, -size.y * .5f);
							var scale = spawner.PrefabSize;

							Buffer.SetComponent(instance, new Translation { Value = position });
							Buffer.SetComponent(instance, new Rotation { Value = rotation.Value });
							Buffer.AddComponent(instance, new NonUniformScale { Value = scale });

							Buffer.AddComponent(instance, new AudioVisualizationInit
							{
								BasePosition = position,
								BaseRotation = rotation.Value,
								BaseScale = scale,
								LockedScaling = spawner.LockedScaling
							});
						}

						// North
						step = size.x / spawner.Size.x;
						sampleIndex = 0;
						for (float x = size.x * .5f; x > -size.x * .5f; x -= step)
						{
							var instance = Buffer.Instantiate(spawner.PrefabEntity);

							Buffer.AddComponent(instance, new AudioSampleIndex { Value = sampleIndex++ });
							Buffer.AddComponent(instance, new AudioAmplitude());

							var position = translation.Value + new float3(x, spawner.Spacing.y, size.y * .5f);
							var scale = spawner.PrefabSize;

							Buffer.SetComponent(instance, new Translation { Value = position });
							Buffer.SetComponent(instance, new Rotation { Value = rotation.Value });
							Buffer.AddComponent(instance, new NonUniformScale { Value = scale });

							Buffer.AddComponent(instance, new AudioVisualizationInit
							{
								BasePosition = position,
								BaseRotation = rotation.Value,
								BaseScale = scale,
								LockedScaling = spawner.LockedScaling
							});
						}

						// East
						step = size.y / spawner.Size.y;
						sampleIndex = 0;
						for (float y = -size.x * .5f; y < size.x * .5f; y += step)
						{
							var instance = Buffer.Instantiate(spawner.PrefabEntity);

							Buffer.AddComponent(instance, new AudioSampleIndex { Value = sampleIndex++ });
							Buffer.AddComponent(instance, new AudioAmplitude());

							var position = translation.Value + new float3(size.x * .5f, spawner.Spacing.y, y);
							var scale = spawner.PrefabSize;

							Buffer.SetComponent(instance, new Translation { Value = position });
							Buffer.SetComponent(instance, new Rotation { Value = rotation.Value });
							Buffer.AddComponent(instance, new NonUniformScale { Value = scale });

							Buffer.AddComponent(instance, new AudioVisualizationInit
							{
								BasePosition = position,
								BaseRotation = rotation.Value,
								BaseScale = scale,
								LockedScaling = spawner.LockedScaling
							});
						}

						// West
						step = size.y / spawner.Size.y;
						sampleIndex = 0;
						for (float y = size.y * .5f; y > -size.y * .5f; y -= step)
						{
							var instance = Buffer.Instantiate(spawner.PrefabEntity);

							Buffer.AddComponent(instance, new AudioSampleIndex { Value = sampleIndex++ });
							Buffer.AddComponent(instance, new AudioAmplitude());

							var position = translation.Value + new float3(-size.x * .5f, spawner.Spacing.y, y);
							var scale = spawner.PrefabSize;

							Buffer.SetComponent(instance, new Translation { Value = position });
							Buffer.SetComponent(instance, new Rotation { Value = rotation.Value });
							Buffer.AddComponent(instance, new NonUniformScale { Value = scale });

							Buffer.AddComponent(instance, new AudioVisualizationInit
							{
								BasePosition = position,
								BaseRotation = rotation.Value,
								BaseScale = scale,
								LockedScaling = spawner.LockedScaling
							});
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
