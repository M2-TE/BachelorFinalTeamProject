using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ECS.AudioVisualization.Components
{
	public struct AudioVisualizationInit : IComponentData
	{
		[NonSerialized] public float3 BasePosition;
		[NonSerialized] public quaternion BaseRotation;
		[NonSerialized] public float3 BaseScale;
	}

	class AudioVisualizationInitProxy : ComponentDataProxy<AudioVisualizationInit> { }
}
