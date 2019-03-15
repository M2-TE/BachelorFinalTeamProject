using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

public class SpawnerSystem : JobComponentSystem
{
	EndSimulationEntityCommandBufferSystem entityCommandBufferSystem;

	protected override void OnCreateManager()
	{
		entityCommandBufferSystem = World.GetOrCreateManager<EndSimulationEntityCommandBufferSystem>();
	}

	struct SpawnerJob : IJobProcessComponentDataWithEntity<Spawner, LocalToWorld>
	{
		public EntityCommandBuffer CommandBuffer;

		public void Execute(Entity entity, int index,[ReadOnly] ref Spawner spawner, [ReadOnly] ref LocalToWorld transMatrix)
		{
			var instance = CommandBuffer.Instantiate(spawner.Prefab);

			CommandBuffer.DestroyEntity(entity);
		}
	}

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		var job = new SpawnerJob
		{
			CommandBuffer = entityCommandBufferSystem.CreateCommandBuffer()
		}.ScheduleSingle(this, inputDeps);

		entityCommandBufferSystem.AddJobHandleForProducer(job);

		return job;
	}
}
