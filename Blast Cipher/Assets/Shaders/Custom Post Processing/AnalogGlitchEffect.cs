using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(AnalogGlitchRenderer), PostProcessEvent.AfterStack, "Custom/AnalogGlitchEffect")]
public sealed class AnalogGlitchEffect : PostProcessEffectSettings
{
	[Range(0f, 1f)]
	public FloatParameter scanLineJitter = new FloatParameter { value = 0f };

	[Range(0f, 1f)]
	public FloatParameter verticalJump = new FloatParameter { value = 0f };

	[Range(0f, 1f)]
	public FloatParameter horizontalShake = new FloatParameter { value = 0f };

	[Range(0f, 1f)]
	public FloatParameter colorDrift = new FloatParameter { value = 0f };
}
