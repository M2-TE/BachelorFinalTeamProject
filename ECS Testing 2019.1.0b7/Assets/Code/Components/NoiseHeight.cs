using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct NoiseHeight : ISharedComponentData
{
	public float Amplification;
	public float Scale;
	public float3 MovementDirection;
}