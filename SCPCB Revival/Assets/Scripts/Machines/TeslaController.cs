using System.Collections;
using UnityEngine;

public class TeslaController : MonoBehaviour {
    [SerializeField] private GameObject teslaTrigger;
    [SerializeField] private GameObject teslaShockEffect;
    [SerializeField] private BoxCollider teslaKillZoneCollider;

    private Renderer teslaShockEffectRenderer;
    private float currentYOffset;
    private float timer = 0f;

    private void Awake() {
        teslaShockEffectRenderer = teslaShockEffect.GetComponent<Renderer>();
    }

    private void Start() {
        teslaTrigger.SetActive(true);
        teslaShockEffect.SetActive(false);
    }

    private void Update() {
        // Animates the texture offset of the tesla shock effect to make it look cooler kind if like the original game did it
        if (teslaShockEffect.activeSelf) {
            timer += Time.deltaTime;

            if (timer >= 0.01f) {
                timer = 0f;
                currentYOffset += 0.1f;
                if (currentYOffset > 1.0f) currentYOffset -= 1.0f;
                teslaShockEffectRenderer.material.mainTextureOffset = new Vector2(0, currentYOffset);
            }
        }
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