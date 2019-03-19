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
		private int sampleCount = 64;
		private int visualizerCount = 64;
		private float[] samples;
		private int2[] sampleGroups; // groups samples with x being the begin index and y being the end index (index in samples array)

		private AudioSource audioSource;

		private List<AudioSampleIndex> allVisualizers;
		private ComponentGroup allVisualizersGroup;

		private ComponentGroup audioTranslatorGroup;
		private ComponentGroup audioRotatorGroup;
		private ComponentGroup audioScalerGroup;

		protected override void OnCreateManager()
		{
			sampleGroups = new int2[sampleCount];
			sampleGroups[0] = new int2(0, 0); // first group is always the single first sample
			CalculateSampleGroups();

			samples = new float[sampleCount];
			audioSource = Object.FindObjectOfType<AudioSource>();

			allVisualizers = new List<AudioSampleIndex>();
			allVisualizersGroup = GetComponentGroup(
				ComponentType.ReadOnly<AudioSampleIndex>());


			audioTranslatorGroup = GetComponentGroup(
				ComponentType.ReadOnly<AudioSampleIndex>(),
				ComponentType.ReadOnly<AudioVisualizationInit>(),
				ComponentType.ReadOnly<AudioTranslator>(), 
				ComponentType.ReadWrite<Translation>());

			audioRotatorGroup = GetComponentGroup(
				ComponentType.ReadOnly<AudioSampleIndex>(),
				ComponentType.ReadOnly<AudioVisualizationInit>(),
				ComponentType.ReadOnly<AudioRotator>(),
				ComponentType.ReadWrite<Rotation>());

			audioScalerGroup = GetComponentGroup(
				ComponentType.ReadOnly<AudioSampleIndex>(),
				ComponentType.ReadOnly<AudioVisualizationInit>(),
				ComponentType.ReadOnly<AudioScaler>(),
				ComponentType.ReadWrite<Translation>(),
				ComponentType.ReadWrite<NonUniformScale>());
		}

		private void CalculateSampleGroups()
		{
			float fResult = Mathf.Pow(sampleCount, 1f / visualizerCount);
			for (int i = 1; i < sampleGroups.Length; i++)
			{
				sampleGroups[i] = new int2
					((int)Mathf.Pow(fResult, i) + i - 1,
					(int)Mathf.Pow(fResult, i + 1) + i - 1);
				if (sampleGroups[i].x > sampleCount || sampleGroups[i].y > sampleCount)
					sampleCount--;
			}
			sampleGroups[visualizerCount - 1] = new int2(sampleGroups[visualizerCount - 1].x, sampleCount - 1);
		}

		//[BurstCompile]
		struct AudioTranslatorJob : IJobProcessComponentData<Translation, AudioTranslator, AudioVisualizationInit>
		{
			[ReadOnly] public float Amount;

			public void Execute(ref Translation translation,
				[ReadOnly] ref AudioTranslator audioTranslator, [ReadOnly] ref AudioVisualizationInit init)
			{
				translation.Value = init.BasePosition + audioTranslator.TranslationModifier * Amount;
			}
		}

		//[BurstCompile]
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

		//[BurstCompile]
		struct AudioScalerJob : IJobProcessComponentData<NonUniformScale, Translation, AudioScaler, AudioVisualizationInit>
		{
			[ReadOnly] public float Amount;

			public void Execute(ref NonUniformScale scale, ref Translation translation, 
				[ReadOnly] ref AudioScaler audioScaler, [ReadOnly] ref AudioVisualizationInit init)
			{
				//scale
				scale.Value = init.BaseScale + audioScaler.ScaleModifiers * Amount;

				//reposition
				translation.Value = init.BasePosition + scale.Value * .5f;
			}
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			audioSource.GetSpectrumData(samples, 0, FFTWindow.BlackmanHarris);

			EntityManager.GetAllUniqueSharedComponentData(allVisualizers);
			for (var i = 1; i < allVisualizers.Count; i++)
			{
				var amplitude = samples[allVisualizers[i].SampleIndex];

				if (!audioScalerGroup.IsEmptyIgnoreFilter)
				{
					audioScalerGroup.SetFilter(allVisualizers[i]);
					inputDeps = new AudioScalerJob
					{
						Amount = amplitude
					}.ScheduleGroup(audioScalerGroup, inputDeps);
				}

				if (!audioTranslatorGroup.IsEmptyIgnoreFilter)
				{
					inputDeps = new AudioTranslatorJob
					{
						Amount = amplitude
					}.ScheduleGroup(audioTranslatorGroup, inputDeps);
				}

				if (!audioRotatorGroup.IsEmptyIgnoreFilter)
				{
					inputDeps = new AudioRotatorJob
					{
						Amount = amplitude
					}.ScheduleGroup(audioRotatorGroup, inputDeps);
				}
			}
			allVisualizers.Clear();

			return inputDeps;
		}
	}
}
