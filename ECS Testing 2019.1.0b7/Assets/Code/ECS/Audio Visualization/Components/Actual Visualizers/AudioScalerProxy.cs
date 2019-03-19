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

	public class AudioScalerProxy : ComponentDataProxy<AudioScaler> { }
}
