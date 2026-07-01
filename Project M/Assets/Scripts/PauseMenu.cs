using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    private @PlayerInputActions input;
    public static bool IsPaused { get; private set; }

    [SerializeField] private GameObject pauseMenu;

    void Awake()
    {
        input = new @PlayerInputActions();
    }

    void OnEnable() => input.Enable();
    void OnDisable() => input.Disable();

    void Update()
    {
        if (input.Player.Pause.WasPressedThisFrame())
        {
            TogglePause();
        }
    }

    void TogglePause()
    {
        IsPaused = !IsPaused;

        pauseMenu.SetActive(IsPaused);

        Time.timeScale = IsPaused ? 0f : 1f;

        Cursor.lockState = IsPaused
            ? CursorLockMode.Confined
            : CursorLockMode.Locked;

        Cursor.visible = IsPaused;
    }

    public void OnResume()
    {
        TogglePause();
    }

    public void OnQuit()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

}