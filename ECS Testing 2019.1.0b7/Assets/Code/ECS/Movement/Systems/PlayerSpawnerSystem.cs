using ECS.Movement.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace ECS.Movement.Systems
{
	public class PlayerSpawnerSystem : JobComponentSystem
	{
		EndSimulationEntityCommandBufferSystem entityCommandBufferSystem;

		protected override void OnCreateManager()
		{
			entityCommandBufferSystem = World.GetOrCreateManager<EndSimulationEntityCommandBufferSystem>();
		}

		struct PlayerSpawnerJob : IJobProcessComponentDataWithEntity<PlayerSpawner, Translation>
		{
			[ReadOnly] public EntityCommandBuffer CommandBuffer;

			public void Execute(Entity entity, int index,[ReadOnly] ref PlayerSpawner playerSpawner, [ReadOnly] ref Translation translation)
			{
				var instance = CommandBuffer.Instantiate(playerSpawner.Prefab);
				CommandBuffer.SetComponent(entity, new Translation { Value = translation.Value });

				CommandBuffer.DestroyEntity(entity);
			}
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			var commandBuffer = entityCommandBufferSystem.CreateCommandBuffer();

			var job = new PlayerSpawnerJob
			{
				CommandBuffer = commandBuffer
			}.ScheduleSingle(this, inputDeps);

			entityCommandBufferSystem.AddJobHandleForProducer(job);

			return job;
		}
	}
}
