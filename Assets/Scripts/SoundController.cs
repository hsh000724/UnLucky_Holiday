using UnityEngine;
using UnityEngine.UI;

public class SoundController : MonoBehaviour
{
    public AudioSource bgmSource;      // BGM용 AudioSource
    public Slider bgmSlider;           // 배경음 슬라이더
    public Toggle bgmMuteToggle;          // 음소거 토글

    public AudioSource sfxSource;      // BGM용 AudioSource
    public Slider sfxSlider;           // 배경음 슬라이더
    public Toggle sfxMuteToggle;          // 음소거 토글

    private void Start()
    {
        if (bgmSlider != null)
        {
            bgmSlider.value = bgmSource.volume;
            bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        }

        if (bgmMuteToggle != null)
        {
            bgmMuteToggle.isOn = bgmSource.mute;
            bgmMuteToggle.onValueChanged.AddListener(SetBGMMute);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = sfxSource.volume;
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }

        if (sfxMuteToggle != null)
        {
            sfxMuteToggle.isOn = sfxSource.mute;
            sfxMuteToggle.onValueChanged.AddListener(SetSFXMute);
        }
    }

    public void SetBGMVolume(float volume)
    {
        bgmSource.volume = volume;

        // 볼륨이 0이면 음소거 상태로 간주
        if (volume <= 0.01f)
            bgmMuteToggle.isOn = true;
        else
            bgmMuteToggle.isOn = false;
    }

    public void SetBGMMute(bool isMuted)
    {
        bgmSource.mute = isMuted;

        // 음소거가 아닐 경우 슬라이더 값을 다시 볼륨에 맞게 설정
        if (!isMuted && bgmSlider != null)
            bgmSource.volume = bgmSlider.value;
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = volume;

        // 볼륨이 0이면 음소거 상태로 간주
        if (volume <= 0.01f)
            sfxMuteToggle.isOn = true;
        else
            sfxMuteToggle.isOn = false;
    }
    public void SetSFXMute(bool isMuted)
    {
        sfxSource.mute = isMuted;

        // 음소거가 아닐 경우 슬라이더 값을 다시 볼륨에 맞게 설정
        if (!isMuted && sfxSlider != null)
            sfxSource.volume = sfxSlider.value;
    }
}
