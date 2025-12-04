using System.Collections;
using UnityEngine;

public class TeslaController : MonoBehaviour {
    [SerializeField] private GameObject teslaTrigger;
    [SerializeField] private GameObject teslaShockEffect;

    private void Start() {
        teslaTrigger.SetActive(true);
        teslaShockEffect.SetActive(false);
    }

    public void TriggerTesla() {
        StartCoroutine(TeslaShockCoroutine());
        StartCoroutine(TriggerTeslaCoroutine());
    }

    private IEnumerator TeslaShockCoroutine() {
        AudioManager.instance.PlaySound(FMODEvents.instance.teslaShock, transform.position);
        yield return new WaitForSeconds(0.5f);
        teslaShockEffect.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        teslaShockEffect.SetActive(false);
    }

    private IEnumerator TriggerTeslaCoroutine() {
        teslaTrigger.SetActive(false);
        yield return new WaitForSeconds(2f);
        teslaTrigger.SetActive(true);
    }
}