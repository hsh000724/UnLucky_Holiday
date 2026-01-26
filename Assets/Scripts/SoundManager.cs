using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("SFX Player Clips")]
    public AudioClip ShieldActivateClip;
    public AudioClip MagneticFieldClip;
    public AudioClip HitClip;
    public AudioClip DeathClip;
    public AudioClip ShootClip;
    public AudioClip CriticalClip;
    public AudioClip ResurrectionClip;
    public AudioClip ExplosionClip;

    [Header("SFX Enemy Clips")]
    public AudioClip Enemy_HurtClip;
    public AudioClip Enemy_DiedClip;

    [Header("SFX Item Clips")]
    public AudioClip Item_HealClip;
    public AudioClip Item_AtkBuffClip;
    public AudioClip Item_PlayerSpeedUpClip;
    public AudioClip Item_AtkSpeedUpClip;
    public AudioClip Item_GetShieldClip;
    public AudioClip Item_CriticalChanceUpClip;
    public AudioClip Item_CriticalMultiplierClip;
    public AudioClip Item_IncreasedBulletClip;
    public AudioClip Item_AddBulletFirePointClip;
    public AudioClip Item_IncreasedRangeClip;
    public AudioClip Item_IncreaseBulletSizeClip;
    public AudioClip Item_ResurrectionClip;
    public AudioClip Item_Add_SpecialWeaponClip;
    public AudioClip Item_MagneticFieldClip;
    public AudioClip Item_EnableExplosionBulletClip;
    public AudioClip Drop_RareItem;
    public AudioClip Drop_LegendItem;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
            sfxSource.PlayOneShot(clip);
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = volume;
    }

    public void MuteSFX(bool mute)
    {
        sfxSource.mute = mute;
    }
}
