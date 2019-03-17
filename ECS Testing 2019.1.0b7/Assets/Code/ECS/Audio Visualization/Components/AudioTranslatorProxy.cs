using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ECS.AudioVisualization.Components
{
	[Serializable]
	public struct AudioTranslator : IComponentData
	{
		[NonSerialized] public float3 BaseTranslation;
		public float3 TranslationModifier;
	}

	[RequireComponent(typeof(AudioVisualizationInitProxy))]
	public class AudioTranslatorProxy : ComponentDataProxy<AudioTranslator> { }
}
