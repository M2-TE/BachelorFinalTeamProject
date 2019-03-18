using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;



namespace ECS.AudioVisualization.Components
{
	[Serializable]
	public struct AudioSpike : ISharedComponentData
	{
		public int SampleIndex;
		[NonSerialized] public float SpikeValue;
	}

	[RequireComponent(typeof(AudioVisualizationInitProxy))]
	public class AudioSpikeProxy : SharedComponentDataProxy<AudioSpike> { }
}
