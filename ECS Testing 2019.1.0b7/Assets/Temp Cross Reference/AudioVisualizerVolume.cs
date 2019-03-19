using UnityEngine;

[System.Obsolete]
public class AudioVisualizerVolume : MonoBehaviour
{
    private Vector3 initialObjectScale;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float repeatTime = .001f;
    [SerializeField] private int sampleSize = 1024;
    [SerializeField] private float intensity = 1f;
    [SerializeField] private float smoothing = 0f;
    private float[] samples;
    private float amplitude;

    private void Start()
    {
        InitializeVariables();

        InvokeRepeating("UpdateAudioParameters", 0, repeatTime);
        
    }

    private void InitializeVariables()
    {
        samples = new float[sampleSize];

        initialObjectScale = transform.localScale;
    }

    private void UpdateAudioParameters()
    {
        audioSource.clip.GetData(samples, audioSource.timeSamples);

        amplitude = 0;
        for (int i = 0; i < samples.Length; i++)
            amplitude += Mathf.Abs(samples[i]);
        amplitude /= sampleSize;

        if (smoothing == 0f || transform.localScale.x < initialObjectScale.x + amplitude)
            transform.localScale = initialObjectScale + initialObjectScale * amplitude * intensity;
    }

    private void Update()
    {
        // TODO improve garvage collection by reusing a v3
        if(smoothing != 0f)
            transform.localScale = new Vector3
                (transform.localScale.x - smoothing, 
                transform.localScale.y - smoothing, 
                transform.localScale.z - smoothing);
    }
}