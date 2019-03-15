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
	private float currentNoiseHeight;

	protected override void OnCreateManager()
	{
		noiseHeightGroup = GetComponentGroup(ComponentType.ReadOnly<NoiseHeight>(), ComponentType.ReadWrite<Translation>());
	}

	[BurstCompile]
	[RequireComponentTag(typeof(NoiseHeight))]
	struct NoiseHeightJob : IJobProcessComponentData<Translation>
	{
		[ReadOnly] public float Amplification;
		[ReadOnly] public float Scale;
		[ReadOnly] public float3 NoiseOffset;

		public void Execute(ref Translation translation)
		{
			var x = translation.Value.x;
			var z = translation.Value.z;
			var height = noise.cnoise(new float3(x, translation.Value.y, z) * Scale + NoiseOffset);
			translation.Value = new float3(x, height * Amplification, z);
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
				NoiseOffset = settings.MovementDirection * Time.time,
				Amplification = settings.Amplification,
				Scale = settings.Scale
			};

			inputDeps = job.ScheduleGroup(noiseHeightGroup, inputDeps);
		}
		uniqueTypes.Clear();
		return inputDeps;
	}
}
