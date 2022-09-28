using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class Menu : MonoBehaviour
{
    public Animator transition;

    [Header("SettingsUI")]
    public Slider horizontalMouse;
    public Slider verticalMouse;
    public Slider sfx;

    public Text horizontalMouseDisplay;
    public Text verticalMouseDisplay;
    public Text sfxDisplay;
    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        horizontalMouse.value = PlayerPrefs.GetFloat("HorizontalMouse", 50);
        verticalMouse.value = PlayerPrefs.GetFloat("VerticalMouse", 50);
        sfx.value = PlayerPrefs.GetFloat("SFX", 50);
        UpdateSliderValueDisplay();
    }
    public void Load(int index)
    {
        StartCoroutine(ChangeScene(index));
    }
    IEnumerator ChangeScene(int sceneIndex)
    {
        transition.Play("FadeOut");
        yield return new WaitForSeconds(transition.GetCurrentAnimatorClipInfo(0).Length);
        SceneManager.LoadScene(sceneIndex);
    }
    public void UpdateSliderValueDisplay()
    {
        horizontalMouseDisplay.text = horizontalMouse.value.ToString("0.0");
        verticalMouseDisplay.text = verticalMouse.value.ToString("0.0");
        sfxDisplay.text = sfx.value.ToString("0.0");
    }
    public void SaveSettings()
    {
        PlayerPrefs.SetFloat("HorizontalMouse", horizontalMouse.value);
        PlayerPrefs.SetFloat("VerticalMouse", verticalMouse.value);
        PlayerPrefs.SetFloat("SFX", sfx.value);
        SwitchMenus(false);
    }
    public void SwitchMenus(bool settings)
    {
        if (settings)
            transition.Play("Settings");
        else
            transition.Play("Main");
    }
    public void Quit()
    {
        Application.Quit();
    }
}
