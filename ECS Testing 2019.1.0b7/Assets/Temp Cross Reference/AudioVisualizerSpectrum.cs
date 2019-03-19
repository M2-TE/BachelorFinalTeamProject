using UnityEngine;
using UnityEngine.UI;

[System.Obsolete]
public class AudioVisualizerSpectrum : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private int nPillars = 50;
    [SerializeField] private float pillarSpacing = 10f;
    [SerializeField] private float repeatTime = .001f;
    [SerializeField] private int sampleSize = 1024;
    [SerializeField] private float intensity = 500f;
    [SerializeField] private float dropoffFactor = 5f;
    [SerializeField] private int zoomFactor = 10;

    private RectTransform ownRectTrans;
    private RectTransform[] pillarRects;
    private float[] samples;

    private void Start()
    {
        InstantiateVariables();
        CreatePillars();

        InvokeRepeating("UpdateAudioVisualizers", 0f, repeatTime);

        ownRectTrans.GetComponent<Image>().color = new Color(0, 0, 0, 0);
    }

    private void InstantiateVariables()
    {
        ownRectTrans = GetComponent<RectTransform>();
        samples = new float[sampleSize];
        pillarRects = new RectTransform[nPillars];
    }

    private void CreatePillars()
    {
        for (int i = 0; i < nPillars; i++)
        {
            GameObject pillar = new GameObject();

            RectTransform rectTrans = pillar.AddComponent<RectTransform>();
            pillarRects[i] = rectTrans;
            rectTrans.SetParent(ownRectTrans);
            rectTrans.pivot = new Vector2(0, 0f);

            rectTrans.sizeDelta = new Vector2
                (ownRectTrans.sizeDelta.x / nPillars
                - pillarSpacing, 1f);

            rectTrans.anchoredPosition = new Vector2
                (-ownRectTrans.sizeDelta.x / 2 + (rectTrans.sizeDelta.x + pillarSpacing) * i,
                -ownRectTrans.sizeDelta.y / 2);

            pillar.AddComponent<Image>();
        }
    }

    private void UpdateAudioVisualizers()
    {
        AudioListener.GetSpectrumData(samples, 0, FFTWindow.BlackmanHarris);

        float amplitude;
        int startIndex = 0;
        int maxIndex = sampleSize / zoomFactor / nPillars;

        for (int i = 0; i < nPillars; i++)
        {
            amplitude = 0f;
            for (int sampleIndex = startIndex; sampleIndex < maxIndex; sampleIndex++)
                amplitude += samples[sampleIndex];

            //amplitude /= (maxIndex - startIndex);

            if (dropoffFactor == 0f || pillarRects[i].sizeDelta.y < 10 + amplitude * intensity)
                pillarRects[i].sizeDelta = new Vector2(pillarRects[i].sizeDelta.x, 10 + amplitude * intensity);
            else
                pillarRects[i].sizeDelta = new Vector2(pillarRects[i].sizeDelta.x, Mathf.MoveTowards(pillarRects[i].sizeDelta.y, 10, dropoffFactor));
            
            startIndex = maxIndex;
            maxIndex += sampleSize / zoomFactor / nPillars;
        }
    }
}