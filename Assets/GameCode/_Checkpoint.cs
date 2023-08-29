using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _Checkpoint : MonoBehaviour
{
    [SerializeField]
    _TagHeuerClock lapTimer;

    void Start()
    {
        if (lapTimer == null)
        {
            lapTimer = FindObjectOfType<_TagHeuerClock>();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player") // Make sure the checkpoint colliders have the tag "Checkpoint"
        {
            
            if (lapTimer != null)
            {
                Debug.Log("Checkpoint reached");
                lapTimer.ReachCheckpoint(transform);
            }
        }
    }
}
