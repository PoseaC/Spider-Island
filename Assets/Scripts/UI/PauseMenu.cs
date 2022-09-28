using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class PauseMenu : MonoBehaviour
{
    public Animator transition;
    public GameObject pauseMenu;
    public GameObject settingsMenu;

    [Header("SettingsUI")]
    public Slider horizontalMouse;
    public Slider verticalMouse;
    public Slider sfx;

    public Text horizontalMouseDisplay;
    public Text verticalMouseDisplay;
    public Text sfxDisplay;

    public static bool isPaused = true;
    float timeScale;
    private void Start()
    {
        AudioSource[] sources = FindObjectsOfType<AudioSource>();
        foreach (AudioSource source in sources)
        {
            source.volume = PlayerPrefs.GetFloat("SFX") / 100;
        }

        transition.Play("FadeOut");
        timeScale = Time.timeScale;
        horizontalMouse.value = PlayerPrefs.GetFloat("HorizontalMouse", 50);
        verticalMouse.value = PlayerPrefs.GetFloat("VerticalMouse", 50);
        sfx.value = PlayerPrefs.GetFloat("SFX", 50);
        UpdateSliderValueDisplay();
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Pause(isPaused);
        }
    }
    public void UpdateSliderValueDisplay()
    {
        horizontalMouseDisplay.text = horizontalMouse.value.ToString("0.0");
        verticalMouseDisplay.text = verticalMouse.value.ToString("0.0");
        sfxDisplay.text = sfx.value.ToString("0.0");
    }
    public void SaveSettings(bool closeSettingsMenu)
    {
        PlayerPrefs.SetFloat("HorizontalMouse", horizontalMouse.value);
        PlayerPrefs.SetFloat("VerticalMouse", verticalMouse.value);
        PlayerPrefs.SetFloat("SFX", sfx.value);

        FindObjectOfType<CameraMovement>().horizontalSensitivity = horizontalMouse.value * 10;
        FindObjectOfType<CameraMovement>().verticalSensitivity = verticalMouse.value * 10;
        FindObjectOfType<GameManager>().SetSFXVolume();

        if(closeSettingsMenu)
            OpenSettings(false);
    }
    public void Pause(bool pause)
    {
        if (pause)
        {
            Cursor.lockState = CursorLockMode.None;
            Time.timeScale = 0;
            pauseMenu.SetActive(true);
            if (Inventory.activeGun != null)
                Inventory.activeGun.GetComponent<WeaponTilt>().enabled = false;
            isPaused = false;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Time.timeScale = timeScale;
            settingsMenu.SetActive(false);
            pauseMenu.SetActive(false);
            SaveSettings(false); 
            if (Inventory.activeGun != null)
                Inventory.activeGun.GetComponent<WeaponTilt>().enabled = true;
            isPaused = true;
            Cursor.visible = false;
        }
    }
    public void OpenSettings(bool settings)
    {
        pauseMenu.SetActive(!settings);
        settingsMenu.SetActive(settings);
    }
    public void Load(int index)
    {
        Pause(false);
        StartCoroutine(ChangeScene(index));
    }
    IEnumerator ChangeScene(int sceneIndex)
    {
        transition.Play("FadeIn");
        yield return new WaitForSeconds(transition.GetCurrentAnimatorClipInfo(0).Length);
        SceneManager.LoadScene(sceneIndex);
    }
}
