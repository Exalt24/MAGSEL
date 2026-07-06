using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainBenchmark : MonoBehaviour
{
    public WaveFunctionCollapse wfc;    // Assign in Inspector
    public int runs = 10;
    public string emotion = "joy";

    private List<float> samples = new List<float>();
    private bool _completed;
    private float _lastMs;

    void Start()
    {
        if (wfc == null)
        {
            Debug.LogError("Assign WaveFunctionCollapse in Inspector.");
            return;
        }
        StartCoroutine(RunBenchmark());
    }

    IEnumerator RunBenchmark()
    {
        yield return null;

        // Subscribe to Unity’s log callback
        Application.logMessageReceived += LogHandler;

        for (int i = 0; i < runs; i++)
        {
            _completed = false;
            _lastMs = 0f;

            int seed = Random.Range(int.MinValue, int.MaxValue);
            wfc.InitializeTerrain(seed, emotion);


            while (!_completed)
                yield return null;

            samples.Add(_lastMs);
            Debug.Log($"Run {i+1}/{runs}: {_lastMs:F1} ms");
        }

        Application.logMessageReceived -= LogHandler;

        // compute average
        float sum = 0f;
        foreach (var t in samples) sum += t;
        float avg = sum / samples.Count;
        Debug.Log($"[Benchmark] {runs} runs average: {avg:F1} ms");
    }

    void LogHandler(string condition, string stackTrace, LogType type)
    {
        if (type == LogType.Log && condition.StartsWith("[Benchmarking] Generation complete"))
        {
            var parts = condition.Split(' ');
            if (parts.Length >= 5 && float.TryParse(parts[4], out float ms))
            {
                _lastMs = ms;
                _completed = true;
            }
        }
    }
}
