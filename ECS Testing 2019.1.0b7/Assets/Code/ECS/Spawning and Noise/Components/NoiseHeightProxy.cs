using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ECS.Spawning.Components
{
	[Serializable]
	public struct NoiseHeight : ISharedComponentData
	{
		public float Amplification;
		public float Scale;
		public float3 MovementDirection;
	}

	public class NoiseHeightProxy : SharedComponentDataProxy<NoiseHeight> { }
}