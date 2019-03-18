using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ECS.AudioVisualization.Components
{
	[Serializable]
	public struct AudioScaler : IComponentData
	{
		public float3 BaseScale;
		public float3 ScaleModifiers;
		public float ScaleIntensity;
	}

	[RequireComponent(typeof(AudioVisualizationInitProxy), typeof(AudioTranslatorProxy))]
	public class AudioScalerProxy : ComponentDataProxy<AudioScaler> { }
}
