using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeControl : MonoBehaviour
{
    [Header("Audio Mixer")]
    public AudioMixer audioMixer;

    // --- 마스터 볼륨 UI 요소 ---
    [Header("Master UI Elements")]
    public Slider masterSlider;
    public Toggle masterMuteToggle;

    [Header("UI Elements")]
    public Slider bgmSlider;
    public Toggle bgmMuteToggle;

    public Slider sfxSlider;
    public Toggle sfxMuteToggle;

    private float masterBeforeMute = 0.8f; // 마스터 볼륨 음소거 전 값
    private float bgmBeforeMute = 0.8f;
    private float sfxBeforeMute = 0.8f;

    private void Start()
    {
        // 1. 볼륨 값 불러오기 및 초기 적용
        InitializeVolumes();

        // 💡 2. UI 이벤트 리스너 연결 (수정된 핵심 부분)
        // 코드를 통해 슬라이더/토글의 값이 변경될 때 해당 함수가 호출되도록 보장합니다.
        ConnectUIListeners();
    }

    private void InitializeVolumes()
    {
        // 마스터 볼륨 초기화
        float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 0.8f);
        masterSlider.value = masterVolume;
        SetMasterVolume(masterVolume);
        masterMuteToggle.isOn = masterVolume <= 0.0001f;

        // BGM 볼륨 초기화
        float bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 0.8f);
        bgmSlider.value = bgmVolume;
        SetBGMVolume(bgmVolume);
        bgmMuteToggle.isOn = bgmVolume <= 0.0001f;

        // SFX 볼륨 초기화
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
        sfxSlider.value = sfxVolume;
        SetSFXVolume(sfxVolume);
        sfxMuteToggle.isOn = sfxVolume <= 0.0001f;
    }

    private void ConnectUIListeners()
    {
        // 마스터
        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        masterMuteToggle.onValueChanged.AddListener(ToggleMasterMute);

        // BGM
        bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        bgmMuteToggle.onValueChanged.AddListener(ToggleBGMMute);

        // SFX
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        sfxMuteToggle.onValueChanged.AddListener(ToggleSFXMute);
    }

    // --- 마스터 볼륨 설정 메서드 ---
    public void SetMasterVolume(float value)
    {
        // 0에 가까운 값은 음소거 처리 (-80dB)
        if (value <= 0.0001f)
        {
            audioMixer.SetFloat("MasterVolume", -80f);
            // 슬라이더 조작 시 음소거 토글 상태 업데이트
            masterMuteToggle.isOn = true;
        }
        else
        {
            // 로그 스케일로 변환하여 볼륨 적용 (0.0001~1.0 -> -80dB~0dB)
            audioMixer.SetFloat("MasterVolume", Mathf.Log10(value) * 20f);
            masterBeforeMute = value; // 음소거 해제를 위해 현재 값 저장
            // 슬라이더 조작 시 음소거 토글 상태 업데이트
            masterMuteToggle.isOn = false;
        }

        PlayerPrefs.SetFloat("MasterVolume", value);
    }

    // --- 마스터 음소거 토글 메서드 ---
    public void ToggleMasterMute(bool isMuted)
    {
        if (isMuted)
        {
            // 음소거 시 슬라이더 값을 최소값(거의 0)으로 설정하여 SetMasterVolume 호출 유도
            masterSlider.value = 0.0001f;
        }
        else
        {
            // 음소거 해제 시 이전 값으로 복구하여 SetMasterVolume 호출 유도
            masterSlider.value = masterBeforeMute;
        }
    }

    // --- BGM/SFX 볼륨 설정 메서드는 변경 없음 ---
    public void SetBGMVolume(float value)
    {
        if (value <= 0.0001f)
        {
            audioMixer.SetFloat("BGMVolume", -80f);
            bgmMuteToggle.isOn = true;
        }
        else
        {
            audioMixer.SetFloat("BGMVolume", Mathf.Log10(value) * 20f);
            bgmBeforeMute = value;
            bgmMuteToggle.isOn = false;
        }

        PlayerPrefs.SetFloat("BGMVolume", value);
    }

    public void SetSFXVolume(float value)
    {
        if (value <= 0.0001f)
        {
            audioMixer.SetFloat("SFXVolume", -80f);
            sfxMuteToggle.isOn = true;
        }
        else
        {
            audioMixer.SetFloat("SFXVolume", Mathf.Log10(value) * 20f);
            sfxBeforeMute = value;
            sfxMuteToggle.isOn = false;
        }

        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    public void ToggleBGMMute(bool isMuted)
    {
        if (isMuted)
        {
            bgmSlider.value = 0.0001f;
        }
        else
        {
            bgmSlider.value = bgmBeforeMute;
        }
    }

    public void ToggleSFXMute(bool isMuted)
    {
        if (isMuted)
        {
            sfxSlider.value = 0.0001f;
        }
        else
        {
            sfxSlider.value = sfxBeforeMute;
        }
    }
}