using UnityEngine;

/// <summary>
/// This script prevents the audio from pausing when the game is paused
/// </summary>
public class IgnoreListenerPause : MonoBehaviour
{
    private AudioSource audioSource;

    private void Start() => (audioSource = GetComponent<AudioSource>()).ignoreListenerPause = true;
}