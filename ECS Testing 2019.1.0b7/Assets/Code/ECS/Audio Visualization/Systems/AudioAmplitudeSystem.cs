using ECS.AudioVisualization.Components;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace ECS.AudioVisualization.Systems
{
	[UpdateBefore(typeof(AudioVisualizationSystem))]
	public class AudioAmplitudeSystem : JobComponentSystem
	{
		private NativeArray<float> amplitudes;

		private int sampleCount;
		private int visualizerGroupCount;
		private float[] samples;
		private int2[] sampleGroups; // groups samples with x being the begin index and y being the end index (index in samples array)
		private AudioSource audioSource;

		private ComponentGroup group;

		protected override void OnCreateManager()
		{
			audioSource = Object.FindObjectOfType<AudioSource>();
			sampleCount = 8192;
			visualizerGroupCount = 200;
			samples = new float[sampleCount];
			CalculateSampleGroups();

			group = GetComponentGroup(
				ComponentType.ReadOnly<AudioSampleIndex>(),
				ComponentType.ReadWrite<AudioAmplitude>());
		}

		private void CalculateSampleGroups()
		{
			// allocate native array (and previously dispose it if one is already allocated)
			if (amplitudes.IsCreated) amplitudes.Dispose();
			amplitudes = new NativeArray<float>(visualizerGroupCount, Allocator.Persistent);

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

		[BurstCompile]
		struct AudioAmplitudeJob : IJobProcessComponentData<AudioSampleIndex, AudioAmplitude>
		{
			[ReadOnly] public NativeArray<float> Amplitudes;

			public void Execute([ReadOnly] ref AudioSampleIndex c0, ref AudioAmplitude c1)
			{
				c1.Value = Amplitudes[c0.SampleIndex];
			}
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			audioSource.GetSpectrumData(samples, 0, FFTWindow.Blackman);
			for (int amplitudeIndex = 0; amplitudeIndex < amplitudes.Length; amplitudeIndex++)
			{
				int2 sampleGroup = sampleGroups[amplitudeIndex];

				float additiveAmplitude = 0f;
				for (int i = sampleGroup.x; i < sampleGroup.y + 1; i++)
					additiveAmplitude += samples[i];

				additiveAmplitude /= sampleGroup.y - sampleGroup.x + 1;

				amplitudes[amplitudeIndex] = additiveAmplitude;
			}

			inputDeps = new AudioAmplitudeJob
			{
				Amplitudes = amplitudes
			}.ScheduleGroup(group, inputDeps);

			return inputDeps;
		}

		protected override void OnDestroyManager()
		{
			amplitudes.Dispose();
		}
	}
}