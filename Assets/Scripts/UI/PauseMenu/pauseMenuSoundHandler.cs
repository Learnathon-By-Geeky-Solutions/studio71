using UnityEngine;
using UnityEngine.UI;
using SingletonManagers;
namespace UI.PauseMenu{
public class pauseMenuSoundHandler : MonoBehaviour
{     
    [Header("UI References")]
    public GameObject  soundMenuUI;
    public GameObject pauseMenuButtons;
    public Slider sfxSlider;
    public Slider backgroundMusicSlider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
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

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void OnSfxSliderChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSfxVolume(value);
        }
    }

    public void OnBackgroundMusicSliderChanged(float value)
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