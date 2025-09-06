using UnityEngine;
using System.Collections;

public class EVNT_PostBreach : MonoBehaviour
{
    public GameObject breachUlgrin;
    public GameObject breachFranklin;
    public GameObject scp173;
    public Animator flickerFog;
    public AudioSource flickerSound;

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
}