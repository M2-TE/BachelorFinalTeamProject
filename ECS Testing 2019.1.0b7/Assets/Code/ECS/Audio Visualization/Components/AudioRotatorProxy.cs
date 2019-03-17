using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ECS.AudioVisualization.Components
{
	[Serializable]
	public struct AudioRotator : IComponentData
	{
		[NonSerialized] public quaternion BaseRotation;
		public float3 RotationModifiers;
	}

	[RequireComponent(typeof(AudioVisualizationInitProxy))]
	public class AudioRotatorProxy : ComponentDataProxy<AudioRotator> { }
}
