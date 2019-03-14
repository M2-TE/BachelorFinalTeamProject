using System;
using Unity.Entities;

[Serializable]
public struct Test : IComponentData
{
	public float Value;
}

public class TestComponent : ComponentDataProxy<Test> { }
