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
	[UpdateAfter(typeof(AudioVisualizationSpawnerSystem))]
	public class AudioVisualizationSystem : JobComponentSystem
	{
		private ComponentGroup audioTranslatorGroup;
		private ComponentGroup audioRotatorGroup;
		private ComponentGroup audioScalerGroup;

		protected override void OnCreateManager()
		{
			audioTranslatorGroup = GetComponentGroup(
				ComponentType.ReadOnly<AudioAmplitude>(),
				ComponentType.ReadOnly<AudioVisualizationInit>(),
				ComponentType.ReadOnly<AudioTranslator>(), 
				ComponentType.ReadWrite<Translation>());

			audioRotatorGroup = GetComponentGroup(
				ComponentType.ReadOnly<AudioAmplitude>(),
				ComponentType.ReadOnly<AudioVisualizationInit>(),
				ComponentType.ReadOnly<AudioRotator>(),
				ComponentType.ReadWrite<Rotation>());

			audioScalerGroup = GetComponentGroup(
				ComponentType.ReadOnly<AudioAmplitude>(),
				ComponentType.ReadOnly<AudioVisualizationInit>(),
				ComponentType.ReadOnly<AudioScaler>(),
				ComponentType.ReadWrite<Translation>(),
				ComponentType.ReadWrite<NonUniformScale>());
		}

		[BurstCompile]
		struct AudioTranslatorJob : IJobProcessComponentData<Translation, AudioTranslator, AudioVisualizationInit>
		{
			[ReadOnly] public float Amount;

			public void Execute(ref Translation translation,
				[ReadOnly] ref AudioTranslator audioTranslator, [ReadOnly] ref AudioVisualizationInit init)
			{
				translation.Value = init.BasePosition + audioTranslator.TranslationModifier * Amount;
			}
		}

		[BurstCompile]
		struct AudioRotatorJob : IJobProcessComponentData<Rotation, AudioRotator, AudioVisualizationInit>
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
		struct AudioScalerJob : IJobProcessComponentData<NonUniformScale, Translation, AudioAmplitude, AudioScaler, AudioVisualizationInit>
		{
			public void Execute(ref NonUniformScale scale, ref Translation translation, 
				[ReadOnly] ref AudioAmplitude amplitude, [ReadOnly] ref AudioScaler audioScaler, [ReadOnly] ref AudioVisualizationInit init)
			{
				//scale
				scale.Value = init.BaseScale + audioScaler.ScaleModifiers * amplitude.Value;

				//reposition
				translation.Value = init.BasePosition + scale.Value * .5f;
			}
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			if (!audioScalerGroup.IsEmptyIgnoreFilter)
				inputDeps = new AudioScalerJob().ScheduleGroup(audioScalerGroup, inputDeps);

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
