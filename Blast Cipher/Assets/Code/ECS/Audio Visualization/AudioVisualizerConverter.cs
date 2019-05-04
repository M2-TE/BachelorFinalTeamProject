using ECS.AudioVisualization.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ECS.AudioVisualization
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(ConvertToEntity))]
	public class AudioVisualizerConverter : MonoBehaviour, IConvertGameObjectToEntity
	{
		public int SampleGroupIndex;

		[Space]
		public bool Scale;
		public float3 ScaleModifiers;

		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			// set transform
			dstManager.SetComponentData(entity, new Translation { Value = transform.position });
			dstManager.SetComponentData(entity, new Rotation { Value = transform.rotation });

			if (dstManager.HasComponent<NonUniformScale>(entity))
				dstManager.SetComponentData(entity, new NonUniformScale { Value = transform.lossyScale });
			else dstManager.AddComponentData(entity, new NonUniformScale { Value = transform.lossyScale });

			// set vital components for visualization system
			dstManager.AddComponentData(entity, new AudioSampleIndex { Value = SampleGroupIndex });
			dstManager.AddComponentData(entity, new AudioAmplitude());
			dstManager.AddComponentData(entity, new AudioVisualizationInit
			{
				BasePosition = transform.position,
				BaseRotation = transform.rotation,
				BaseScale = transform.lossyScale
			});

			if (Scale)
			{
				dstManager.AddComponentData(entity, new AudioScaler { ScaleModifiers = ScaleModifiers });
			}
		}
	}
}
