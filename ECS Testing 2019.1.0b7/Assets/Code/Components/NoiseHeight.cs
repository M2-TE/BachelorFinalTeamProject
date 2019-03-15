using System;
using Unity.Entities;

[Serializable]
public struct NoiseHeight : ISharedComponentData
{
	public float Amplification;
	public float Resolution;
}