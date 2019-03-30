using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ECS.AudioVisualization.Components
{
	[Serializable]
	public struct AudioScaler : IComponentData
	{
		public float3 ScaleModifiers;
	}

	[RequireComponent(typeof(AudioAmplitudeProxy), typeof(AudioVisualizationInitProxy), typeof(AudioSampleIndexProxy))]
	[RequireComponent(typeof(ConvertToEntity))]
	public class AudioScalerProxy : ComponentDataProxy<AudioScaler> { }
}
