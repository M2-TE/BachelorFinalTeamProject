using System;
using Unity.Entities;

[Serializable]
public struct AudioSampleIndex : ISharedComponentData
{
	[NonSerialized] public int SampleIndex;
}

public class AudioSampleIndexProxy : SharedComponentDataProxy<AudioSampleIndex> { }