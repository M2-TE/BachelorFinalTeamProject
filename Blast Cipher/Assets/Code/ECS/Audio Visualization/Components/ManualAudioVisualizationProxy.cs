using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace ECS.AudioVisualization.Components
{
	[DisallowMultipleComponent, RequireComponent(typeof(ConvertToEntity))]
	public class ManualAudioVisualizationProxy : MonoBehaviour, IConvertGameObjectToEntity
	{
		[SerializeField] private int sampleIndex;
		[SerializeField] private bool lockedScaling;

		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			var amplitude = new AudioAmplitude();
			var sampler = new AudioSampleIndex
			{
				Value = sampleIndex
			};
			var nonUniformScale = new NonUniformScale
			{
				Value = transform.lossyScale
			};
			var init = new AudioVisualizationInit
			{
				BasePosition = transform.position,
				BaseRotation = transform.rotation,
				BaseScale = transform.lossyScale,
				LockedScaling = lockedScaling ? 1 : 0
			};

			dstManager.AddComponentData(entity, amplitude);
			dstManager.AddComponentData(entity, sampler);
			dstManager.SetComponentData(entity, nonUniformScale);
			dstManager.AddComponentData(entity, init);
		}
	}
}
