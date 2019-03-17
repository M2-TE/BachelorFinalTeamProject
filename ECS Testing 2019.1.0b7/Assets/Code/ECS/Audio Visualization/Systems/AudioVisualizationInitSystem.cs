using ECS.AudioVisualization.Components;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ECS.AudioVisualization.Systems
{
	[UpdateBefore(typeof(AudioVisualizationSystem))]
	public class AudioVisualizationInitSystem : JobComponentSystem
	{
		EndSimulationEntityCommandBufferSystem entityCommandBufferSystem;

		private ComponentGroup audioTranslatorGroup;
		private ComponentGroup audioRotatorGroup;

		protected override void OnCreateManager()
		{
			entityCommandBufferSystem = World.GetOrCreateManager<EndSimulationEntityCommandBufferSystem>();

			audioTranslatorGroup = GetComponentGroup(
				ComponentType.ReadOnly<AudioVisualizationInit>(),
				ComponentType.ReadOnly<Translation>(),
				ComponentType.ReadWrite<AudioTranslator>());

			audioRotatorGroup = GetComponentGroup(
				ComponentType.ReadOnly<AudioVisualizationInit>(),
				ComponentType.ReadOnly<Rotation>(),
				ComponentType.ReadWrite<AudioRotator>());
		}

		[RequireComponentTag(typeof(AudioVisualizationInit))]
		struct AudioTranslatorInitJob : IJobProcessComponentDataWithEntity<Translation, AudioTranslator>
		{
			[ReadOnly] public EntityCommandBuffer CommandBuffer;

			public void Execute(Entity entity, int index, [ReadOnly] ref Translation translation, ref AudioTranslator audioTranslator)
			{
				audioTranslator.BaseTranslation = translation.Value;
				CommandBuffer.RemoveComponent<AudioVisualizationInit>(entity);
			}
		}

		[RequireComponentTag(typeof(AudioVisualizationInit))]
		struct AudioRotatorInitJob : IJobProcessComponentDataWithEntity<Rotation, AudioRotator>
		{
			[ReadOnly] public EntityCommandBuffer CommandBuffer;

			public void Execute(Entity entity, int index, [ReadOnly] ref Rotation rotation, ref AudioRotator audioRotator)
			{
				audioRotator.BaseRotation = rotation.Value;
				CommandBuffer.RemoveComponent<AudioVisualizationInit>(entity);
			}
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			var commandBuffer = entityCommandBufferSystem.CreateCommandBuffer();

			// Translator Init Job
			inputDeps = new AudioTranslatorInitJob
			{
				CommandBuffer = commandBuffer
			}.ScheduleGroupSingle(audioTranslatorGroup, inputDeps);
			entityCommandBufferSystem.AddJobHandleForProducer(inputDeps);

			// Rotator Init Job
			inputDeps = new AudioRotatorInitJob
			{
				CommandBuffer = commandBuffer
			}.ScheduleGroupSingle(audioRotatorGroup, inputDeps);
			entityCommandBufferSystem.AddJobHandleForProducer(inputDeps);

			return inputDeps;
		}
	}
}
