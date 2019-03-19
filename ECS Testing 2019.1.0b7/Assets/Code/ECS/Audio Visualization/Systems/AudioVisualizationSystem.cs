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
		private int sampleCount;
		private int visualizerGroupCount;
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
			sampleCount = 8192;
			visualizerGroupCount = 200;
			samples = new float[sampleCount];

			CalculateSampleGroups();

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
			float fResult = Mathf.Pow(sampleCount, 1f / (visualizerGroupCount - 1));
			List<int2> sampleGroupList = new List<int2> { new int2(0, 0) };
			for (var i = 1; i < visualizerGroupCount; i++)
			{
				var groupStartIndex = sampleGroupList[i - 1].y + 1;
				var groupEndIndex = (int)Mathf.Pow(fResult, i);
				if (groupEndIndex < groupStartIndex)
					groupEndIndex = groupStartIndex;

				if (i == visualizerGroupCount - 1)
					groupEndIndex = sampleCount - 1;

				sampleGroupList.Add(new int2(groupStartIndex, groupEndIndex));
				//Debug.Log(sampleGroupList[i]);
			}
			sampleGroups = sampleGroupList.ToArray();
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
			audioSource.GetSpectrumData(samples, 0, FFTWindow.Blackman);

			float[] amplitudes = new float[visualizerGroupCount];
			for(int amplitudeIndex = 0; amplitudeIndex < amplitudes.Length; amplitudeIndex++)
			{
				int2 sampleGroup = sampleGroups[amplitudeIndex];

				float additiveAmplitude = 0f;
				for(int i = sampleGroup.x; i < sampleGroup.y + 1; i++)
					additiveAmplitude += samples[i];

				additiveAmplitude /= sampleGroup.y - sampleGroup.x + 1;

				amplitudes[amplitudeIndex] = additiveAmplitude;
			}

			EntityManager.GetAllUniqueSharedComponentData(allVisualizers);
			for (var i = 1; i < allVisualizers.Count; i++)
			{
				var amplitude = amplitudes[allVisualizers[i].SampleIndex];

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
