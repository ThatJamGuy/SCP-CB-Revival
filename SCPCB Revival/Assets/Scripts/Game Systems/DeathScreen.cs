using UnityEngine;

public class DeathScreen : MonoBehaviour
{
    [Header("Menus")]
    [SerializeField] private GameObject deathMenuScreen;

    [Header("Audio")]
    [SerializeField] private AudioClip menuInteract;
    [SerializeField] private AudioClip menuFailInteract;

    [SerializeField] private AudioSource interactSource;

    public bool isOpen;

    private void Start()
    {
        interactSource.ignoreListenerPause = true;
    }

    public void ToggleDeathMenu()
    {
        isOpen = !isOpen;
        GameManager.Instance.TogglePlayerInput(true);
        deathMenuScreen.SetActive(isOpen);

        //GameManager.Instance.PauseGame();
    }

    public void PlayInteractSFX(bool failed)
    {
        if (!failed)
            interactSource.PlayOneShot(menuInteract);
        else
            interactSource.PlayOneShot(menuFailInteract);
    }
}