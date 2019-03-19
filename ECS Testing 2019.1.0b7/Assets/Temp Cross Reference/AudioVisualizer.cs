using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioVisualizer : MonoBehaviour
{
    #region Variables
    public enum VisualizationType { Pillars, Dots, Lines, Graph };

    [SerializeField] private VisualizationType visualizationType = VisualizationType.Pillars;
    [SerializeField] private Canvas parentCanvas;
    [SerializeField] private Camera canvasCam;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private int nVisualizers = 100;
    [SerializeField] private int nAudioSamples = 8192;
    [SerializeField] private float intensity = 1000;
    [SerializeField] private float visualizerMinHeight = 1f;

    [Space(10)] [SerializeField] private LineSettings lineSettings;

    private List<UnityEngine.Object> objectsToDestroy;
    private RectTransform ownRectTransform;
    private RectTransform[] visualizers;
    private LineRenderer[] lineRenderers;
    private Vector2Int[] sampleGroups;
    private float[] samples;
    #endregion

    #region Unity Calls
    void Start ()
    {
        InstantiateVariables();
        CalculateSampleGroups();

        UpdateCanvasSettings();
        switch (visualizationType)
        {
            case VisualizationType.Pillars:
                BuildPillars();
                break;

            case VisualizationType.Dots:

                break;

            case VisualizationType.Lines:
                BuildLines();
                break;

            case VisualizationType.Graph:

                break;

            default:

                break;
        }
    }

    private void InstantiateVariables()
    {
        GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);

        ownRectTransform = GetComponent<RectTransform>();
        visualizers = new RectTransform[nVisualizers];
        lineRenderers = new LineRenderer[nVisualizers];
        samples = new float[nAudioSamples];
        sampleGroups = new Vector2Int[nVisualizers];
        sampleGroups[0] = new Vector2Int(0, 0);

        objectsToDestroy = new List<UnityEngine.Object>();
    }

    private void CalculateSampleGroups()
    {
        float fResult = Mathf.Pow(nAudioSamples, 1f / nVisualizers);
        for (int i = 1; i < sampleGroups.Length; i++)
        {
            sampleGroups[i] = new Vector2Int
                ((int)Mathf.Pow(fResult, i) + i - 1,
                (int)Mathf.Pow(fResult, i + 1) + i - 1);
            if (sampleGroups[i].x > nAudioSamples || sampleGroups[i].y > nAudioSamples)
                nVisualizers--;
        }
        sampleGroups[nVisualizers - 1] = new Vector2Int(sampleGroups[nVisualizers - 1].x, nAudioSamples - 1);
    }

    void Update()
    {
        audioSource.GetSpectrumData(samples, 0, FFTWindow.BlackmanHarris);

        switch (visualizationType)
        {
            case VisualizationType.Pillars:
                ApplySpectrumToPillars();
                break;

            case VisualizationType.Dots:

                break;

            case VisualizationType.Lines:
                ApplySpectrumToLines();
                break;

            case VisualizationType.Graph:

                break;

            default:

                break;
        }
    }

    private void OnValidate()
    {
        UpdateCanvasSettings();
    }

    private void OnApplicationQuit()
    {
        foreach (UnityEngine.Object unityObject in objectsToDestroy)
            Destroy(unityObject);
    }
    #endregion

    #region Builders
    private void BuildPillars()
    {
        GameObject tempGO;
        RectTransform tempRT;
        
        for(int i = 0; i < nVisualizers; i++)
        {
            (tempRT = 
                (tempGO = new GameObject("Pillar")).AddComponent<RectTransform>())
                .SetParent(ownRectTransform);

            tempGO.AddComponent<Image>();

            tempRT.pivot = Vector2.zero;
            tempRT.anchorMax = Vector2.zero;
            tempRT.anchorMin = Vector2.zero;

            tempRT.sizeDelta = new Vector2(ownRectTransform.sizeDelta.x / nVisualizers, visualizerMinHeight);
            tempRT.anchoredPosition = new Vector2(tempRT.sizeDelta.x * i, 0f);

            visualizers[i] = tempRT;
        }
    }

    private void BuildLines()
    {
        GameObject tempGO;
        RectTransform tempRT;
        LineRenderer tempLR;
        Material tempMat = new Material(Shader.Find("UI/Unlit/Transparent"))
            { color = lineSettings.LineMainColor };
        objectsToDestroy.Add(tempMat);

        for (int i = 0; i < nVisualizers; i++)
        {
            tempGO = new GameObject("Line");

            #region Line Renderer Setup
            tempLR = tempGO.AddComponent<LineRenderer>();

            tempLR.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            tempLR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            tempLR.alignment = LineAlignment.TransformZ;
            tempLR.allowOcclusionWhenDynamic = false;
            tempLR.receiveShadows = false;
            tempLR.useWorldSpace = false;
            tempLR.material = tempMat;

            tempLR.colorGradient = lineSettings.Gradient;
            tempLR.widthCurve = lineSettings.LineShape;

            tempLR.SetPosition(1, new Vector3(0f, 100f, 0f));
            #endregion

            #region RectTransform Setup
            tempRT = tempGO.AddComponent<RectTransform>();

            tempRT.SetParent(ownRectTransform);
            tempRT.anchoredPosition3D = new Vector3((lineSettings.LineShape.Evaluate(0f) + lineSettings.LineSpacing) * i, 0f, 0f);
            tempRT.localScale = new Vector3(1f, 1f, 1f);

            tempRT.pivot = Vector2.zero;
            tempRT.anchorMax = Vector2.zero;
            tempRT.anchorMin = Vector2.zero;

            lineRenderers[i] = tempLR;
            #endregion
        }
    }
    #endregion

    #region Visualizers
    private void ApplySpectrumToPillars()
    {
        float amplitude;
        Vector2 deltaVector = new Vector2(visualizers[0].sizeDelta.x, visualizerMinHeight);
        for (int i = 0; i < nVisualizers; i++)
        {
            amplitude = 0;
            if (sampleGroups[i].x != sampleGroups[i].y)
            {
                for (int sampleIndex = sampleGroups[i].x; sampleIndex <= sampleGroups[i].y; sampleIndex++)
                    amplitude += Mathf.Abs(samples[sampleIndex]);

                amplitude = amplitude / (sampleGroups[i].y + 1 - sampleGroups[i].x);
            }
            else
                amplitude = Mathf.Abs(samples[sampleGroups[i].x]);

            deltaVector.y = visualizerMinHeight + amplitude * intensity;
            visualizers[i].sizeDelta = deltaVector;
        }
    }

    private void PlaceDots()
    {

    }

    private void ApplySpectrumToLines()
    {
        float amplitude;
        Vector3 lineEndPos = new Vector3(0f, 100f, 0f);
        
        for (int i = 0; i < nVisualizers; i++)
        {
            amplitude = 0;
            if (sampleGroups[i].x != sampleGroups[i].y)
            {
                for (int sampleIndex = sampleGroups[i].x; sampleIndex <= sampleGroups[i].y; sampleIndex++)
                    amplitude += Mathf.Abs(samples[sampleIndex]);

                amplitude = amplitude / (sampleGroups[i].y + 1 - sampleGroups[i].x);
            }
            else
                amplitude = Mathf.Abs(samples[sampleGroups[i].x]);

            lineEndPos.y = visualizerMinHeight + amplitude * intensity;
            lineRenderers[i].SetPosition(1, lineEndPos);

            // TODO change gradient to match mirroring
            if(lineSettings.Mirrored)
                lineRenderers[i].SetPosition(0, -lineEndPos);
        }
    }

    private void ApplySpectrumToPolygon()
    {

    }
    #endregion

    #region Public Calls
    public void UpdateCanvasSettings()
    {
        switch (visualizationType)
        {
            case VisualizationType.Pillars:
                parentCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                break;

            case VisualizationType.Dots:

                break;

            case VisualizationType.Lines:
                parentCanvas.renderMode = RenderMode.ScreenSpaceCamera;
                parentCanvas.worldCamera = canvasCam;
                break;

            case VisualizationType.Graph:

                break;

            default:

                break;
        }
    }
    #endregion

    #region Settings Structs
    [Serializable]
    private struct LineSettings
    {
        [Header("Colors")]
        public Color LineMainColor;
        public Gradient Gradient;

        [Header("Shape and Positioning")]
        public bool Mirrored;
        public float LineSpacing;
        public AnimationCurve LineShape;
        public AnimationCurve GroundShape;
    }

    [Serializable]
    private struct PolygonSettings
    {

    }
    #endregion
}