using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class AnalogGlitchRenderer : PostProcessEffectRenderer<AnalogGlitchEffect>
{

	float _verticalJumpTime;

	public override void Render(PostProcessRenderContext context)
	{
		var sheet = context.propertySheets.Get(Shader.Find("Hidden/Custom/Glitch/Analog"));


		_verticalJumpTime += Mathf.Lerp(Time.unscaledDeltaTime, Time.deltaTime, settings.timeScaleLerp) * settings.verticalJump * 11.3f;

		var sl_thresh = Mathf.Clamp01(1.0f - settings.scanLineJitter * 1.2f);
		var sl_disp = 0.002f + Mathf.Pow(settings.scanLineJitter, 3) * 0.05f;

		sheet.properties.SetVector("_ScanLineJitter", new Vector2(sl_disp, sl_thresh));
		sheet.properties.SetVector("_VerticalJump", new Vector2(settings.verticalJump, _verticalJumpTime));
		sheet.properties.SetFloat("_HorizontalShake", settings.horizontalShake * 0.2f);
		sheet.properties.SetVector("_ColorDrift", new Vector2(settings.colorDrift * 0.04f, /*Time.time * 606.11f*/1f));

		context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
	}
}
