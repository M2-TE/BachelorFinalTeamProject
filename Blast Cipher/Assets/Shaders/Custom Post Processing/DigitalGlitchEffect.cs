using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(DigitalGlitchRenderer), PostProcessEvent.BeforeStack, "Custom/DigitalGlitchEffect")]
public sealed class DigitalGlitchEffect : PostProcessEffectSettings
{
	[Range(0f, 1f)]
	public FloatParameter intensity = new FloatParameter { value = 0f };
}
