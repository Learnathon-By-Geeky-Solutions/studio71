using UnityEngine;
using UnityEngine.UI;
using SingletonManagers;
namespace UI{
public class PauseMenuSoundHandler : MonoBehaviour
{     
    [Header("UI References")]
    [SerializeField]private GameObject  soundMenuUI;
    [SerializeField]private GameObject pauseMenuButtons;
    [SerializeField]private Slider sfxSlider;
    [SerializeField]private Slider backgroundMusicSlider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);
            OnSfxSliderChanged(sfxSlider.value);
        }
        else
        {
            Debug.LogError("SFX Slider is not assigned in pauseMenuSoundHandler!");
        }

        if (backgroundMusicSlider != null)
        {
            backgroundMusicSlider.onValueChanged.AddListener(OnBackgroundMusicSliderChanged);
            OnBackgroundMusicSliderChanged(backgroundMusicSlider.value);
        }
        else
        {
            Debug.LogError("Background Music Slider is not assigned in pauseMenuSoundHandler!");
        }
    }

    
    
    public static void OnSfxSliderChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSfxVolume(value);
        }
    }

    public static void OnBackgroundMusicSliderChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetBackgroundMusicVolume(value);
        }
    }

    public void backToPauseMenu()
    {
         soundMenuUI.SetActive(false);
        pauseMenuButtons.SetActive(true);
    }

}
}