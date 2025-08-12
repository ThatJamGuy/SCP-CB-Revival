using UnityEngine;
using System.Collections;

public class EVNT_PostBreach : MonoBehaviour
{
    public GameObject breachUlgrin;
    public GameObject breachFranklin;

    public void TriggerFranklingUlgrinEvent() {
        StartCoroutine(FranklinUlgrinEvent());
    }

    private IEnumerator FranklinUlgrinEvent() {
        yield return new WaitForSeconds(0.5f);
        breachUlgrin.GetComponent<Animator>().SetTrigger("Turn180");
        yield return new WaitForSeconds(0.5f);
        breachFranklin.GetComponent<Animator>().SetTrigger("WalkBack");
    }
}