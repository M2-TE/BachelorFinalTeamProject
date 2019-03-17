using ECS.AudioVisualization.Components;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ECS.AudioVisualization.Systems
{
	public class AudioVisualizationSystem : JobComponentSystem
	{
		private ComponentGroup audioTranslatorGroup;
		private ComponentGroup audioRotatorGroup;

		protected override void OnCreateManager()
		{
			audioTranslatorGroup = GetComponentGroup(
				ComponentType.ReadOnly<AudioTranslator>(), 
				ComponentType.ReadWrite<Translation>());

			audioRotatorGroup = GetComponentGroup(
				ComponentType.ReadOnly<AudioRotator>(),
				ComponentType.ReadWrite<Rotation>());
		}

		[BurstCompile]
		struct AudioTranslatorJob : IJobProcessComponentData<Translation, AudioTranslator>
		{
			[ReadOnly] public float Amount;

			public void Execute(ref Translation translation,[ReadOnly] ref AudioTranslator audioTranslator)
			{
				translation.Value = audioTranslator.BaseTranslation + audioTranslator.TranslationModifier * Amount;
			}
		}

		[BurstCompile]
		struct AudioRotatorJob : IJobProcessComponentData<Rotation, AudioRotator>
		{
			[ReadOnly] public float Amount;

			public void Execute(ref Rotation rotation, [ReadOnly] ref AudioRotator audioRotator)
			{
				//var eulerBase = audioRotator.BaseRotation;
				rotation.Value = quaternion.Euler(audioRotator.RotationModifiers * Amount);
			}
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			if (!audioTranslatorGroup.IsEmptyIgnoreFilter)
			{
				inputDeps = new AudioTranslatorJob
				{
					Amount = Time.time
				}.ScheduleGroup(audioTranslatorGroup, inputDeps);
			}

			if (!audioRotatorGroup.IsEmptyIgnoreFilter)
			{
				inputDeps = new AudioRotatorJob
				{
					Amount = Time.time
				}.ScheduleGroup(audioRotatorGroup, inputDeps);
			}

			return inputDeps;
		}
	}
}
