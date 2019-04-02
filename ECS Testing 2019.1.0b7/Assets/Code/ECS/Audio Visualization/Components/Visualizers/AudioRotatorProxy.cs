using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ECS.AudioVisualization.Components
{
	[Serializable]
	public struct AudioRotator : IComponentData
	{
		public float3 RotationModifiers;
	}

	public class AudioRotatorProxy : ComponentDataProxy<AudioRotator> { }
}
