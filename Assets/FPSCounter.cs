using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    private TextMeshProUGUI _fpsText;
    private Queue<float> _frameTimes = new Queue<float>();
    private float _frameTimeSum = 0.0f;
    private const float _frameTimeWindow = 1.0f; // 1 second for averaging
    private float _updateInterval = 1.0f; // Update text every second
    private float _timer = 0.0f;

    private void Awake()
    {
        _fpsText = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        // Remove old frame times to keep the window at 1 second
        while (_frameTimes.Count > 0 && _frameTimeSum > _frameTimeWindow)
        {
            _frameTimeSum -= _frameTimes.Dequeue();
        }

        // Add the time of the current frame
        float currentFrameTime = Time.unscaledDeltaTime;
        _frameTimes.Enqueue(currentFrameTime);
        _frameTimeSum += currentFrameTime;

        // Update the display every _updateInterval seconds
        _timer += Time.unscaledDeltaTime;
        if (_timer >= _updateInterval)
        {
            _timer = 0f; // Reset timer

            // Calculate and display average FPS
            if (_frameTimeSum > 0 && _frameTimes.Count > 0)
            {
                float averageFps = _frameTimes.Count / _frameTimeSum;
                _fpsText.text = string.Format("{0:0.} fps", averageFps);
            }
        }
    }
}