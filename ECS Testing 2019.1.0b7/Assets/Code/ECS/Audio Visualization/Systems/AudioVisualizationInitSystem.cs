using ECS.AudioVisualization.Components;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

namespace ECS.AudioVisualization.Systems
{
	[UpdateBefore(typeof(AudioVisualizationSystem))]
	public class AudioVisualizationInitSystem : JobComponentSystem
	{
		EndSimulationEntityCommandBufferSystem entityCommandBufferSystem;
		private ComponentGroup audioTranslatorGroup;
		private List<AudioVisualizationInit> uniqueAudioTranslators = new List<AudioVisualizationInit>();

		protected override void OnCreateManager()
		{
			entityCommandBufferSystem = World.GetOrCreateManager<EndSimulationEntityCommandBufferSystem>();

			audioTranslatorGroup = GetComponentGroup(
				ComponentType.ReadOnly<AudioVisualizationInit>(),
				ComponentType.ReadOnly<Translation>(),
				ComponentType.ReadWrite<AudioTranslator>());
		}

		[RequireComponentTag(typeof(AudioVisualizationInit))]
		struct AudioTranslatorInitJob : IJobProcessComponentDataWithEntity<Translation, AudioTranslator>
		{
			[ReadOnly] public EntityCommandBuffer CommandBuffer;

			public void Execute(Entity entity, int index, [ReadOnly] ref Translation translation, ref AudioTranslator audioTranslator)
			{
				audioTranslator.BaseTranslation = translation.Value;
				Debug.Log(audioTranslator.BaseTranslation);
				CommandBuffer.RemoveComponent<AudioVisualizationInit>(entity);
			}
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			var commandBuffer = entityCommandBufferSystem.CreateCommandBuffer();

			EntityManager.GetAllUniqueSharedComponentData(uniqueAudioTranslators);
			for (int i = 0; i < uniqueAudioTranslators.Count; i++)
			{
				var audioTranslator = uniqueAudioTranslators[i];
				audioTranslatorGroup.SetFilter(audioTranslator);

				inputDeps = new AudioTranslatorInitJob
				{
					CommandBuffer = commandBuffer
				}.ScheduleGroupSingle(audioTranslatorGroup, inputDeps);

				entityCommandBufferSystem.AddJobHandleForProducer(inputDeps);
			}
			uniqueAudioTranslators.Clear();

			return inputDeps;
		}
	}
}
