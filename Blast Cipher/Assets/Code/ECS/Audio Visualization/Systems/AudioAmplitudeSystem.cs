using ECS.AudioVisualization.Components;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace ECS.AudioVisualization.Systems
{
	[UpdateAfter(typeof(AudioVisualizationSpawnerSystem))]
	public class AudioAmplitudeSystem : JobComponentSystem
	{
		private NativeArray<float> sampleArr;
		private NativeArray<float> amplitudeArr;
		private NativeArray<float> totalAmplitude;
		private NativeArray<int2> sampleGroupArr;

		private int sampleCount;
		private int visualizerGroupCount;
		private float[] samples;
		private AudioSource audioSource;

		private EntityQuery audioEntities;

		protected override void OnCreate()
		{
			audioSource = Object.FindObjectOfType<AudioSource>();
			sampleCount = 8192;
			visualizerGroupCount = 10;
			CalculateSampleGroups();

			audioEntities = GetEntityQuery(
				ComponentType.ReadOnly<AudioSampleIndex>(),
				ComponentType.ReadWrite<AudioAmplitude>());
		}

		private void CalculateSampleGroups()
		{
			// allocate native array (and previously dispose it if one is already allocated)
			if (amplitudeArr.IsCreated) amplitudeArr.Dispose();
			if (totalAmplitude.IsCreated) totalAmplitude.Dispose();
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
			totalAmplitude = new NativeArray<float>(JobsUtility.MaxJobThreadCount, Allocator.Persistent);
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
			public NativeArray<float> TotalAmplitude;

			public void Execute(int amplitudeIndex)
			{
				int2 sampleGroup = SampleGroupArr[amplitudeIndex];

				float additiveAmplitude = 0f;
				for (int i = sampleGroup.x; i < sampleGroup.y + 1; i++)
					additiveAmplitude += SampleArr[i];

				additiveAmplitude /= sampleGroup.y - sampleGroup.x + 1;

				AmplitudeArr[amplitudeIndex] = additiveAmplitude;
				TotalAmplitude[0] += additiveAmplitude;
			}
		}

		[BurstCompile]
		struct AudioAmplitudeJob : IJobForEach<AudioSampleIndex, AudioAmplitude>
		{
			[ReadOnly] public NativeArray<float> Amplitudes;
			[ReadOnly] public NativeArray<float> TotalAmplitude;

			public void Execute([ReadOnly] ref AudioSampleIndex sampleIndex, ref AudioAmplitude amplitude)
			{
				amplitude.Value = sampleIndex.Value > -1 ? Amplitudes[sampleIndex.Value] : TotalAmplitude[0] / Amplitudes.Length;
			}
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			audioSource.GetSpectrumData(samples, 0, FFTWindow.Blackman);
			sampleArr.CopyFrom(samples);
			totalAmplitude[0] = 0f;

			var samplerJob = new AudioAmplitudeCalcJob
			{
				SampleGroupArr = sampleGroupArr,
				SampleArr = sampleArr,
				AmplitudeArr = amplitudeArr,
				TotalAmplitude = totalAmplitude
			}.Schedule(sampleGroupArr.Length, 1024, inputDeps);

			var ampJob = new AudioAmplitudeJob
			{
				Amplitudes = amplitudeArr,
				TotalAmplitude = totalAmplitude
			}.Schedule(audioEntities, samplerJob);

			return ampJob;
		}

		protected override void OnDestroy()
		{
			amplitudeArr.Dispose();
			totalAmplitude.Dispose();
			sampleArr.Dispose();
			sampleGroupArr.Dispose();
		}
	}
}