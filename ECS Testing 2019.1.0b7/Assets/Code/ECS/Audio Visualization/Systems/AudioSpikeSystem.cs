using ECS.AudioVisualization.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

namespace ECS.AudioVisualization.Systems
{
	[UpdateAfter(typeof(AudioVisualizationInitSystem))]
	public class AudioSpikeSystem : JobComponentSystem
	{
		private ComponentGroup group;
		private AudioSource audioSource;
		private readonly float[] samples = new float[64];

		protected override void OnCreateManager()
		{
			audioSource = Object.FindObjectOfType<AudioSource>();
			Debug.Log(audioSource.clip.name);
			//group = GetComponentGroup(ComponentType.ReadWrite<AudioSpike>()); // WORK WITH CHUNKS HERE
		}

		[RequireComponentTag(typeof(AudioSpike))]
		private struct AudioSpikeJob : IJobProcessComponentData<Translation>
		{
			[ReadOnly] public float SpikeIntensity;

			public void Execute(ref Translation trans)
			{
				throw new System.NotImplementedException();
			}
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			audioSource.GetSpectrumData(samples, 0, FFTWindow.BlackmanHarris);
			return new AudioSpikeJob { SpikeIntensity = samples[0] }.ScheduleGroup(group, inputDeps);
		}
	}
}
