using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(AudioLowPassFilter))]
public class SoundscapeAudioSource : MonoBehaviour {
	
	public bool DrawDebugLine = false;
	public LayerMask OcclusionLayer = 1;
		
	[Header("Occlusion Control")]
	public float FadeDuration = 0.1f;
	[Range(0f, 1f)]
	public float DampVolume = 0.1f;
	[Range(0, 5000)]
	public float CutoffFreq = 500f;

	Transform audioListener;
	AudioSource audioSource;
	AudioLowPassFilter lowPassFilter;
	float defaultVolume;
	float defaultCutoffFreq;
	float distanceThreshold;
	AudioState _audioState;

	enum AudioState	{
		None,
		DefaultAudio,
		DampAudio,
	}
	
	void Awake() {

		audioListener = GameObject.FindObjectOfType<AudioListener>().transform;
		audioSource = GetComponent<AudioSource>();
		lowPassFilter = GetComponent<AudioLowPassFilter>();

		defaultVolume = audioSource.volume;
		defaultCutoffFreq = lowPassFilter.cutoffFrequency;	
		distanceThreshold = audioSource.maxDistance * audioSource.maxDistance;

		_audioState = AudioState.None;
	}
	
	void FixedUpdate() {
		//stop linecasts when the player is outside of audible range
		if ((audioListener.position - transform.position).sqrMagnitude < distanceThreshold)	{

			RaycastHit hit;
			bool occluded = Physics.Linecast(transform.position, audioListener.position, out hit, OcclusionLayer);

			if (occluded && _audioState != AudioState.DampAudio) {
				StopAllCoroutines();
				StartCoroutine(DampAudio());
			}
			else if (!occluded && _audioState != AudioState.DefaultAudio) {
				StopAllCoroutines();
				StartCoroutine(DefaultAudio());
			}

			if (DrawDebugLine)	{
				if (occluded) Debug.DrawLine(transform.position, hit.point, Color.red);
				else Debug.DrawLine(transform.position, audioListener.position, Color.red);
			}
		}
	}

	IEnumerator DefaultAudio()	{
		//Debug.Log ("DefaultAudio : start");
		_audioState = AudioState.DefaultAudio;
		yield return null;
			
		float t = 0f;
		while (audioSource.volume < defaultVolume) {
			audioSource.volume = Mathf.Lerp(audioSource.volume, defaultVolume, t);
			lowPassFilter.cutoffFrequency = Mathf.Lerp(lowPassFilter.cutoffFrequency, defaultCutoffFreq, t);
			t += Time.deltaTime / FadeDuration;
			yield return new WaitForEndOfFrame();
		}
		//Debug.Log ("DefaultAudio : complete");
	}

	IEnumerator DampAudio()	{
		//Debug.Log ("DampAudio : start");
		_audioState = AudioState.DampAudio;
		yield return null;

		float t = 0f;
		while (audioSource.volume > DampVolume) {
			audioSource.volume = Mathf.Lerp(audioSource.volume, DampVolume, t);
			lowPassFilter.cutoffFrequency = Mathf.Lerp(lowPassFilter.cutoffFrequency, CutoffFreq, t);
			t += Time.deltaTime / FadeDuration;
			yield return new WaitForEndOfFrame();
		}
		//Debug.Log ("DampAudio : complete");
	}
}