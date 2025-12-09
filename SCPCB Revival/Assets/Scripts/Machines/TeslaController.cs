using System.Collections;
using UnityEngine;

public class TeslaController : MonoBehaviour {
    [SerializeField] private GameObject teslaTrigger;
    [SerializeField] private GameObject teslaShockEffect;
    [SerializeField] private BoxCollider teslaKillZoneCollider;

    private void Start() {
        teslaTrigger.SetActive(true);
        teslaShockEffect.SetActive(false);
    }

    public void TriggerTesla() {
        if (PlayerAccessor.instance.isDead) return;
        StartCoroutine(TeslaShockCoroutine());
        StartCoroutine(TriggerTeslaCoroutine());
    }

    public void KillPlayer() {
        GameManager.instance.ShowDeathScreen("Subject D-9341 killed by the Tesla Gate at [REDACTED].");
        PlayerAccessor.instance.isDead = true;
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