using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ECS.AudioVisualization.Components
{
	[Serializable]
	public struct AudioScaler : ISharedComponentData
	{
		public float3 BaseScale;
		public float3 ScaleModifiers;
	}

	[RequireComponent(typeof(AudioVisualizationInitProxy))]
	public class AudioScalerProxy : SharedComponentDataProxy<AudioScaler> { }
}
