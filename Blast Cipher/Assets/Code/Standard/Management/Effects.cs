using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class Effects
{
	private Effects() { }
	private static Effects instance;

	private PostProcessVolume ppVol;

	private Vignette vignette;
	private float baseVignetteIntensity;

	private AnalogGlitchEffect analogGlitch;
	private float baseAnalogScanLineJitter;
	private float baseAnalogVerticalJump;
	private float baseAnalogHorizontalShake;
	private float baseAnalogColorDrift;

	private DigitalGlitchEffect digitalGlitch;
	private float baseDitigalGlitchIntensity;

	public static void Init(PostProcessVolume ppVol)
	{
		instance = new Effects
		{
			ppVol = ppVol
		};

		// vignette init
		ppVol.profile.TryGetSettings(out instance.vignette);
		instance.baseVignetteIntensity = instance.vignette.intensity;

		// analog glitch init
		ppVol.profile.TryGetSettings(out instance.analogGlitch);
		instance.baseAnalogScanLineJitter = instance.analogGlitch.scanLineJitter;
		instance.baseAnalogVerticalJump = instance.analogGlitch.verticalJump;
		instance.baseAnalogHorizontalShake = instance.analogGlitch.horizontalShake;
		instance.baseAnalogColorDrift = instance.analogGlitch.colorDrift;

		// digital glitch init
		ppVol.profile.TryGetSettings(out instance.digitalGlitch);
		instance.baseDitigalGlitchIntensity = instance.digitalGlitch.intensity;
	}
	
	private static IEnumerator Transition(FloatParameter param, float targetIntensity, float duration)
	{
		float baseIntensity = param.value;
		float timer = 0f;
		do
		{
			param.value = Mathf.LerpUnclamped(baseIntensity, targetIntensity, timer / duration);
			timer += Time.unscaledDeltaTime;
			yield return null;
		}
		while (timer < duration);

		param.value = targetIntensity;
		//Debug.Log(param + " " +  targetIntensity);
	}
	
	private static IEnumerator Spike(FloatParameter param, float spikeIntensity, float inDuration, float outDuration)
	{
		yield return null;
	}

	public static void ResetVignette(float duration)
		=> StartVignetteTransition(instance.baseVignetteIntensity, duration);

	public static void StartVignetteTransition(float targetIntensity, float duration)
		=> instance.ppVol.StartCoroutine(Transition(instance.vignette.intensity, targetIntensity, duration));


	public static void ResetDigitalGlitch(float duration)
		=> StartDigitalGlitchTransition(instance.baseDitigalGlitchIntensity, duration);

	public static void StartDigitalGlitchTransition(float targetIntensity, float duration)
		=> instance.ppVol.StartCoroutine(Transition(instance.digitalGlitch.intensity, targetIntensity, duration));


	public static void ResetAnalogGlitch(float duration)
	{
		StartAnalogGlitchTransition
			(instance.baseAnalogScanLineJitter, 
			instance.baseAnalogVerticalJump, 
			instance.baseAnalogHorizontalShake, 
			instance.baseAnalogColorDrift,
			duration);
	}

	public static void StartAnalogGlitchTransition(float scanLineJitter, float verticalJump, float horizontalShake, float colorDrift, float duration)
	{
		instance.ppVol.StartCoroutine(Transition(instance.analogGlitch.scanLineJitter, scanLineJitter, duration));
		instance.ppVol.StartCoroutine(Transition(instance.analogGlitch.verticalJump, verticalJump, duration));
		instance.ppVol.StartCoroutine(Transition(instance.analogGlitch.horizontalShake, horizontalShake, duration));
		instance.ppVol.StartCoroutine(Transition(instance.analogGlitch.colorDrift, colorDrift, duration));
	}
}
