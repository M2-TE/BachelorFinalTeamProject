using ECS.AudioVisualization.Components;
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
	[UpdateAfter(typeof(AudioAmplitudeSystem))]
	public class AudioVisualizationSystem : JobComponentSystem
	{
		private EntityQuery audioTranslatorGroup;
		private EntityQuery audioRotatorGroup;
		private EntityQuery audioScalerGroup;

		protected override void OnCreateManager()
		{
			audioTranslatorGroup = GetEntityQuery(
				ComponentType.ReadOnly<AudioAmplitude>(),
				ComponentType.ReadOnly<AudioVisualizationInit>(),
				ComponentType.ReadOnly<AudioTranslator>(), 
				ComponentType.ReadWrite<Translation>());

			audioRotatorGroup = GetEntityQuery(
				ComponentType.ReadOnly<AudioAmplitude>(),
				ComponentType.ReadOnly<AudioVisualizationInit>(),
				ComponentType.ReadOnly<AudioRotator>(),
				ComponentType.ReadWrite<Rotation>());
			

			audioScalerGroup = GetEntityQuery(
				ComponentType.ReadOnly<AudioAmplitude>(),
				ComponentType.ReadOnly<AudioVisualizationInit>(),
				ComponentType.ReadOnly<AudioScaler>(),
				ComponentType.ReadWrite<Translation>(),
				ComponentType.ReadWrite<NonUniformScale>());
		}

		[BurstCompile]
		struct AudioTranslatorJob : IJobForEach<Translation, AudioTranslator, AudioVisualizationInit>
		{
			[ReadOnly] public float Amount;

			public void Execute(ref Translation translation,
				[ReadOnly] ref AudioTranslator audioTranslator, [ReadOnly] ref AudioVisualizationInit init)
			{
				translation.Value = init.BasePosition + audioTranslator.TranslationModifier * Amount;
			}
		}

		[BurstCompile]
		struct AudioRotatorJob : IJobForEach<Rotation, AudioRotator, AudioVisualizationInit>
		{
			[ReadOnly] public float Amount;

			public void Execute(ref Rotation rotation, 
				[ReadOnly] ref AudioRotator audioRotator, [ReadOnly] ref AudioVisualizationInit init)
			{
				//var eulerBase = init.BaseRotation;
				rotation.Value = quaternion.Euler(audioRotator.RotationModifiers * Amount);
			}
		}

		[BurstCompile]
		struct AudioScalerJob : IJobForEach<NonUniformScale, Translation, AudioAmplitude, AudioScaler, AudioVisualizationInit>
		{
			public void Execute(ref NonUniformScale scale, ref Translation translation, 
				[ReadOnly] ref AudioAmplitude amplitude, [ReadOnly] ref AudioScaler audioScaler, [ReadOnly] ref AudioVisualizationInit init)
			{
				//scale
				scale.Value = init.BaseScale + audioScaler.ScaleModifiers * amplitude.Value;

				//reposition
				if(init.LockedScaling == 1)
					translation.Value = init.BasePosition + scale.Value * .5f;
			}
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			if (!audioScalerGroup.IsEmptyIgnoreFilter)
				inputDeps = new AudioScalerJob().Schedule(audioScalerGroup, inputDeps);

			//if (!audioTranslatorGroup.IsEmptyIgnoreFilter)
			//{
			//	inputDeps = new AudioTranslatorJob
			//	{
			//		Amount = amplitude
			//	}.ScheduleGroup(audioTranslatorGroup, inputDeps);
			//}

			//if (!audioRotatorGroup.IsEmptyIgnoreFilter)
			//{
			//	inputDeps = new AudioRotatorJob
			//	{
			//		Amount = amplitude
			//	}.ScheduleGroup(audioRotatorGroup, inputDeps);
			//}

			return inputDeps;
		}
	}
}
