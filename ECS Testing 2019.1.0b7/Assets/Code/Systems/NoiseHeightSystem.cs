using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class NoiseHeightSystem : JobComponentSystem
{
	ComponentGroup noiseHeightGroup;
	private List<NoiseHeight> uniqueTypes = new List<NoiseHeight>(10);

	protected override void OnCreateManager()
	{
		noiseHeightGroup = GetComponentGroup(ComponentType.ReadOnly<NoiseHeight>(), ComponentType.ReadWrite<Translation>());
	}

	[BurstCompile]
	[RequireComponentTag(typeof(NoiseHeight))]
	struct NoiseHeightJob : IJobProcessComponentData<Translation>
	{
		[ReadOnly] public float Time;
		//[ReadOnly] public NoiseHeight Settings;

		public void Execute(ref Translation translation)
		{
			var x = translation.Value.x;
			var z = translation.Value.z;
			var height = noise.cnoise(new float3(x, math.sin(Time), z));
			translation.Value = new float3(x, height, z);
		}
	}

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		EntityManager.GetAllUniqueSharedComponentData(uniqueTypes);
		for (int i = 0; i < uniqueTypes.Count; i++)
		{
			var settings = uniqueTypes[i];
			noiseHeightGroup.SetFilter(settings);

			var job = new NoiseHeightJob
			{
				Time = Time.time
				//Settings = settings
			};

			inputDeps = job.ScheduleGroup(noiseHeightGroup, inputDeps);
		}
		uniqueTypes.Clear();
		return inputDeps;
	}
}
