using NaughtyAttributes;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    [SerializeField] private Menu[] menus;

    [SerializeField] private AudioClip menuInteractSuccess;
    [SerializeField] private AudioClip menuInteractFail;

    [SerializeField] private AudioSource menuInteractSource;

    [System.Serializable]
    private class Menu
    {
        public GameObject menu;
        public bool isActive;
        public bool pausesGame;
        public bool togglesInput;
        public bool openViaKey;
        public KeyCode key;
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if(menuInteractSource != null) menuInteractSource.ignoreListenerPause = true;
    }

    private void Update()
    {
        for (int i = 0; i < menus.Length; i++)
        {
            var menu = menus[i];
            if (menu.openViaKey && Input.GetKeyDown(menu.key))
            {
                ToggleMenu(i);
            }
        }
    }

    public void ToggleMenu(int index)
    {
        if (index < 0 || index >= menus.Length) return;
        var menu = menus[index];
        menu.isActive = !menu.isActive;
        menu.menu.SetActive(menu.isActive);
        if(menu.pausesGame) GameManager.Instance.PauseGame();
        if(menu.togglesInput) GameManager.Instance.TogglePlayerInput(true);
    }

    public void PlayInteractSFX(bool failed)
    {
        if (!failed)
            menuInteractSource.PlayOneShot(menuInteractSuccess);
        else
            menuInteractSource.PlayOneShot(menuInteractFail);
    }
}