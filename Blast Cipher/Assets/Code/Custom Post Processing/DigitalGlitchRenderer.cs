using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public sealed class DigitalGlitchRenderer : PostProcessEffectRenderer<DigitalGlitchEffect>
{
	bool initiated;
	Texture2D _noiseTexture;
	RenderTexture _trashFrame1;
	RenderTexture _trashFrame2;

	public override void Render(PostProcessRenderContext context)
	{
		if (!initiated)
		{
			SetUpResources();
		}

		if (Random.value > Mathf.Lerp(0.9f, 0.5f, settings.intensity))
		{
			UpdateNoiseTexture();
		}

		// Update trash frames on a constant interval.
		var fcount = Time.frameCount;
		if (fcount % 13 == 0) context.command.BlitFullscreenTriangle(context.source, _trashFrame1);
		if (fcount % 73 == 0) context.command.BlitFullscreenTriangle(context.source, _trashFrame2);

		var sheet = context.propertySheets.Get(Shader.Find("Hidden/Custom/Glitch/Digital"));
		sheet.properties.SetFloat("_Intensity", settings.intensity);
		sheet.properties.SetTexture("_NoiseTex", _noiseTexture);

		var trashFrame = Random.value > 0.5f ? _trashFrame1 : _trashFrame2;
		sheet.properties.SetTexture("_TrashTex", trashFrame);

		context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
	}

	static Color RandomColor()
	{
		return new Color(Random.value, Random.value, Random.value, Random.value);
	}

	void SetUpResources()
	{
		_noiseTexture = new Texture2D(64, 32, TextureFormat.ARGB32, false)
		{
			hideFlags = HideFlags.DontSave,
			wrapMode = TextureWrapMode.Clamp,
			filterMode = FilterMode.Point
		};

		_trashFrame1 = new RenderTexture(Screen.width, Screen.height, 0);
		_trashFrame2 = new RenderTexture(Screen.width, Screen.height, 0);
		_trashFrame1.hideFlags = HideFlags.DontSave;
		_trashFrame2.hideFlags = HideFlags.DontSave;

		UpdateNoiseTexture();

		initiated = true;
	}

	void UpdateNoiseTexture()
	{
		var color = RandomColor();

		for (var y = 0; y < _noiseTexture.height; y++)
		{
			for (var x = 0; x < _noiseTexture.width; x++)
			{
				if (Random.value > 0.89f) color = RandomColor();
				_noiseTexture.SetPixel(x, y, color);
			}
		}

		_noiseTexture.Apply();
	}
}
