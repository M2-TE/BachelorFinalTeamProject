using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ECS.AudioVisualization.Components
{
	[Serializable]
	public struct AudioTranslator : IComponentData
	{
		public float3 TranslationModifier;
	}

	public class AudioTranslatorProxy : ComponentDataProxy<AudioTranslator> { }
}
