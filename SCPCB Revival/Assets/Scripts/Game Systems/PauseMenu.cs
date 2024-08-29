using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [Header("Menus")]
    [SerializeField] private GameObject pauseMenuScreen;
    [SerializeField] private GameObject quitMenuScreen;

    [Header("Audio")]
    [SerializeField] private AudioClip menuInteract;
    [SerializeField] private AudioClip menuFailInteract;

    [SerializeField] private AudioSource interactSource;

    private bool isOpen;

    public void TogglePauseMenu()
    {
        isOpen = !isOpen;
        GameManager.Instance.TogglePlayerInput(true);
        pauseMenuScreen.SetActive(isOpen);

        GameManager.Instance.PauseGame();

        quitMenuScreen.SetActive(false);
    }

    public void PlayInteractSFX(bool failed)
    {
        if (!failed)
            interactSource.PlayOneShot(menuInteract);
        else
            interactSource.PlayOneShot(menuFailInteract);
    }
}