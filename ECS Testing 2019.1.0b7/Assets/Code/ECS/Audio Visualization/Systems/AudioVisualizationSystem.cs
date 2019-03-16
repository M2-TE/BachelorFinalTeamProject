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

		protected override void OnCreateManager()
		{
			audioTranslatorGroup = GetComponentGroup(
				ComponentType.ReadOnly<AudioTranslator>(), 
				ComponentType.ReadWrite<Translation>());
		}

		//[BurstCompile]
		struct AudioTranslatorJob : IJobProcessComponentData<Translation, AudioTranslator>
		{
			[ReadOnly] public float Intensity;

			public void Execute(ref Translation translation,[ReadOnly] ref AudioTranslator audioTranslator)
			{
				translation.Value = audioTranslator.BaseTranslation + audioTranslator.TranslationModifier * Intensity;
			}
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			var audioTransJob = new AudioTranslatorJob
			{
				Intensity = Time.time
			}.ScheduleGroup(audioTranslatorGroup, inputDeps);

			return audioTransJob;
		}
	}
}
