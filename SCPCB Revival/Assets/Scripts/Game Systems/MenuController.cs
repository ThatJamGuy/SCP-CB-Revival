using UnityEngine;

public class MenuController : MonoBehaviour
{
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip menuInteract;
    [SerializeField] private AudioClip menuFailInteract;

    [SerializeField] private AudioSource interactSource;

    private void Start()
    {
        MusicPlayer.Instance.StartMusic(menuMusic);
    }

    public void PlayInteractSFX(bool failed)
    {
        if (!failed)
            interactSource.PlayOneShot(menuInteract);
        else
            interactSource.PlayOneShot(menuFailInteract);
    }

    public void OpenLink(string linkURL)
    {
        Application.OpenURL(linkURL);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}