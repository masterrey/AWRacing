using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class _TagHeuerClock : MonoBehaviour
{
    public Text lapTimeText;
    public Text bestLapTimeText;
    public Transform[] checkpoints; // Array of checkpoint Transforms

    private float lapTime;
    private float bestLapTime;
    private int lapCount;
    private int nextCheckpointIndex; // Index of the next checkpoint the car has to reach
    private bool isTiming;

    // Start is called before the first frame update
    void Start()
    {
        lapTime = 0f;
        bestLapTime = float.MaxValue; // Initialize to a large value
        lapCount = 0;
        nextCheckpointIndex = 0;
        isTiming = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isTiming)
        {
            lapTime += Time.deltaTime/Time.timeScale;
            UpdateLapTimeUI(lapTime, lapTimeText);
        }
    }

    void UpdateLapTimeUI(float time, Text textComponent)
    {
        int minutes = (int)(time / 60);
        int seconds = (int)(time % 60);
        float milliseconds = (time * 1000) % 1000;

        textComponent.text = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
    }
    public void ReachCheckpoint(Transform checkpointTransform)
    {
        if (checkpoints[nextCheckpointIndex] == checkpointTransform)
        {
            nextCheckpointIndex = (nextCheckpointIndex + 1) % checkpoints.Length;

            // If we reach the first checkpoint again, complete a lap
            if (nextCheckpointIndex == 0)
            {
                CompleteLap();
            }
        }
    }

    void CompleteLap()
    {
        isTiming = true;
        lapCount++;

        if (lapTime < bestLapTime)
        {
            bestLapTime = lapTime;
            UpdateLapTimeUI(bestLapTime, bestLapTimeText);
        }

        lapTime = 0f;
    }

    public void StopTiming()
    {
        isTiming = false;
    }

    public void StartTiming()
    {
        isTiming = true;
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (!isTiming)
            {
                StartTiming();
            }
           
        }
    }
}
