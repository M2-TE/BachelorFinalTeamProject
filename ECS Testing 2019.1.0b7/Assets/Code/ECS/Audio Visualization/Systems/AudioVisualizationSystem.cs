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
	[UpdateAfter(typeof(AudioSpikeSystem))]
	public class AudioVisualizationSystem : JobComponentSystem
	{
		private ComponentGroup audioSpikeGroup;

		private ComponentGroup audioTranslatorGroup;
		private ComponentGroup audioRotatorGroup;
		private ComponentGroup audioScalerGroup;

		protected override void OnCreateManager()
		{
			audioSpikeGroup = GetComponentGroup(
				ComponentType.ReadOnly<AudioSpike>()); // DO THIS WHEN AUDIO SPIKE IS SHARED


			audioTranslatorGroup = GetComponentGroup(
				ComponentType.ReadOnly<AudioTranslator>(), 
				ComponentType.ReadWrite<Translation>());

			audioRotatorGroup = GetComponentGroup(
				ComponentType.ReadOnly<AudioRotator>(),
				ComponentType.ReadWrite<Rotation>());

			audioScalerGroup = GetComponentGroup(
				ComponentType.ReadOnly<AudioScaler>(),
				ComponentType.ReadOnly<AudioTranslator>(),
				ComponentType.ReadWrite<Translation>(),
				ComponentType.ReadWrite<NonUniformScale>());
		}

		[BurstCompile] [ExcludeComponent(typeof(AudioScaler))]
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

		[BurstCompile]
		struct AudioScalerJob : IJobProcessComponentData<NonUniformScale, Translation, AudioScaler, AudioTranslator>
		{
			[ReadOnly] public float Amount;

			public void Execute(ref NonUniformScale scale, ref Translation translation, [ReadOnly] ref AudioScaler audioScaler, [ReadOnly] ref AudioTranslator audioTranslator)
			{
				//scale
				scale.Value = audioScaler.BaseScale + audioScaler.ScaleModifiers * Amount;

				//reposition
				translation.Value = audioTranslator.BaseTranslation + scale.Value * .5f;
			}

			//public void Execute(ref NonUniformScale scale, ref Translation translation, 
			//	[ReadOnly] ref AudioScaler audioScaler, [ReadOnly] ref AudioTranslator audioTranslator, [ReadOnly] ref AudioSpike audioSpike)
			//{
			//	Amount = audioSpike.SpikeValue;

			//	//scale
			//	scale.Value = audioScaler.BaseScale + audioScaler.ScaleModifiers * Amount;

			//	//reposition
			//	translation.Value = audioTranslator.BaseTranslation + scale.Value * .5f;
			//}
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

			if (!audioScalerGroup.IsEmptyIgnoreFilter)
			{
				inputDeps = new AudioScalerJob
				{
					Amount = Time.time
				}.ScheduleGroup(audioScalerGroup, inputDeps);
			}

			return inputDeps;
		}
	}
}
