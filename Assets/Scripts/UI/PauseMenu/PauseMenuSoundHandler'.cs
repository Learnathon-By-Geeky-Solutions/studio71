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
    [SerializeField]private Slider inGameSoundSlider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogError("AudioManager instance not found in PauseMenuSoundHandler!");
        }

        if (sfxSlider != null)
        {
            float currentSfxVolume = AudioManager.Instance.SfxVolumeMultiplier;
            Debug.Log($"PauseMenuSoundHandler: Setting SFX slider initial value to: {currentSfxVolume}");
            sfxSlider.SetValueWithoutNotify(currentSfxVolume);
            sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);
        }
        else
        {
            Debug.LogError("SFX Slider is not assigned in pauseMenuSoundHandler!");
        }

        if (backgroundMusicSlider != null)
        {
            float currentMusicVolume = AudioManager.Instance.BackgroundMusicVolumeMultiplier;
            Debug.Log($"PauseMenuSoundHandler: Setting Music slider initial value to: {currentMusicVolume}");
            backgroundMusicSlider.SetValueWithoutNotify(currentMusicVolume);
            backgroundMusicSlider.onValueChanged.AddListener(OnBackgroundMusicSliderChanged);
        }
        else
        {
            Debug.LogError("Background Music Slider is not assigned in pauseMenuSoundHandler!");
        }
        
        if (inGameSoundSlider != null)
        {
            float currentInGameSoundVolume = AudioManager.Instance.InGameSoundVolumeMultiplier;
            Debug.Log($"PauseMenuSoundHandler: Setting In-Game Sound slider initial value to: {currentInGameSoundVolume}");
            inGameSoundSlider.SetValueWithoutNotify(currentInGameSoundVolume);
            inGameSoundSlider.onValueChanged.AddListener(OnInGameSoundSliderChanged);
        }
        else
        {
            Debug.LogError("In-Game Sound Slider is not assigned in pauseMenuSoundHandler!");
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
    
    public static void OnInGameSoundSliderChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetInGameSoundVolume(value);
        }
    }

    public void backToPauseMenu()
    {   AudioManager.PlaySound(SoundKeys.ButtonPress);
         pauseMenuButtons.SetActive(true);
        
         soundMenuUI.SetActive(false);
        
    }

}
}