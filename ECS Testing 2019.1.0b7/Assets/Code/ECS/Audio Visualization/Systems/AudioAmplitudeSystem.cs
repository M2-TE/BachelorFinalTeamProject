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
	[UpdateAfter(typeof(AudioVisualizationSpawnerSystem))]
	public class AudioAmplitudeSystem : JobComponentSystem
	{
		private NativeArray<float> sampleArr;
		private NativeArray<float> amplitudeArr;
		private NativeArray<int2> sampleGroupArr;

		private int sampleCount;
		private int visualizerGroupCount;
		private float[] samples;
		private AudioSource audioSource;

		private ComponentGroup audioEntities;

		protected override void OnCreateManager()
		{
			audioSource = Object.FindObjectOfType<AudioSource>();
			sampleCount = 8192;
			//sampleCount = 4096;
			visualizerGroupCount = 201;
			CalculateSampleGroups();

			audioEntities = GetComponentGroup(
				ComponentType.ReadOnly<AudioSampleIndex>(),
				ComponentType.ReadWrite<AudioAmplitude>());
		}

		private void CalculateSampleGroups()
		{
			// allocate native array (and previously dispose it if one is already allocated)
			if (amplitudeArr.IsCreated) amplitudeArr.Dispose();
			if (sampleArr.IsCreated) sampleArr.Dispose();
			if (sampleGroupArr.IsCreated) sampleGroupArr.Dispose();


			samples = new float[sampleCount];

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

			amplitudeArr = new NativeArray<float>(visualizerGroupCount, Allocator.Persistent);
			sampleArr = new NativeArray<float>(sampleCount, Allocator.Persistent);
			sampleGroupArr = new NativeArray<int2>(sampleGroupList.Count, Allocator.Persistent);
			sampleGroupArr.CopyFrom(sampleGroupList.ToArray());
		}

		[BurstCompile]
		struct AudioAmplitudeCalcJob : IJobParallelFor
		{
			[ReadOnly] public NativeArray<int2> SampleGroupArr;
			[ReadOnly] public NativeArray<float> SampleArr;
			public NativeArray<float> AmplitudeArr;

			public void Execute(int amplitudeIndex)
			{
				int2 sampleGroup = SampleGroupArr[amplitudeIndex];

				float additiveAmplitude = 0f;
				for (int i = sampleGroup.x; i < sampleGroup.y + 1; i++)
					additiveAmplitude += SampleArr[i];

				additiveAmplitude /= sampleGroup.y - sampleGroup.x + 1;

				AmplitudeArr[amplitudeIndex] = additiveAmplitude;
			}
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
			sampleArr.CopyFrom(samples);
			var samplerJob = new AudioAmplitudeCalcJob
			{
				SampleGroupArr = sampleGroupArr,
				SampleArr = sampleArr,
				AmplitudeArr = amplitudeArr
			}.Schedule(amplitudeArr.Length, 32, inputDeps);

			var ampJob = new AudioAmplitudeJob
			{
				Amplitudes = amplitudeArr
			}.ScheduleGroup(audioEntities, samplerJob);

			return ampJob;
		}

		protected override void OnDestroyManager()
		{
			amplitudeArr.Dispose();
			sampleArr.Dispose();
			sampleGroupArr.Dispose();
		}
	}
}