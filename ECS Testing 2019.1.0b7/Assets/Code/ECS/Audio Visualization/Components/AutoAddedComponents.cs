using Unity.Entities;
using Unity.Mathematics;

namespace ECS.AudioVisualization.Components
{
	public struct AudioSampleIndex : IComponentData
	{
		public int Value;
	}
	public struct AudioAmplitude : IComponentData
	{
		public float Value;
	}

	public struct AudioVisualizationInit : IComponentData
	{
		public float3 BasePosition;
		public quaternion BaseRotation;
		public float3 BaseScale;
		public int LockedScaling;
	}
}