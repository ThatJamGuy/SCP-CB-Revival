using scpcbr;
using System.Collections;
using UnityEngine;

public class EVNT_PostBreach : MonoBehaviour {
    public GameObject breachUlgrin;
    public GameObject breachFranklin;
    public GameObject scp173;
    public Animator flickerFog;
    public AudioSource flickerSound;

    private GlobalCameraShake cameraShake;

    #region Default Methods
    private void Start() {
        InitializePostBreach();
    }
    #endregion

    private void InitializePostBreach() {
        // Set the ambience to zone 1
        // (post-breach LCZ cuz that's where we are now. Improve later to support saving/loading in other zones)
        AmbienceController.Instance.currentZone = 1;
        AmbienceController.Instance.PlayNextCommotion();

        cameraShake = GlobalCameraShake.Instance;

        // Change the music to Post-Breach theme (Later set to zone theme after leaving starting room)
        MusicPlayer.Instance.ChangeMusic("The Breach");
    }

    public void TriggerIntroShake(float duration) {
        cameraShake.ShakeCamera(.3f, 0, duration);
    }

    public void TriggerFranklingUlgrinEvent() {
        StartCoroutine(FranklinUlgrinEvent());
    }

    private IEnumerator FranklinUlgrinEvent() {
        yield return new WaitForSeconds(0.5f);
        breachUlgrin.GetComponent<Animator>().SetTrigger("Turn180");
        yield return new WaitForSeconds(2f);
        breachFranklin.GetComponent<Animator>().SetTrigger("WalkBack");
        yield return new WaitForSeconds(4f);
        flickerFog.SetTrigger("Flicker");
        scp173.transform.position += new Vector3(0, 0, -10);
        flickerSound.Play();
    }

    // --- Utility ---
    IEnumerator Wait(float t) { yield return new WaitForSeconds(t); }
}