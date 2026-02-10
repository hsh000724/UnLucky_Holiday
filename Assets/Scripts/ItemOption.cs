using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemOption : MonoBehaviour
{
    public static ItemOption instance;
    private MessageManager messageManager;
    private float baseDropProbability = 0.05f;
    private float baseDropProbability2 = 0.02f;
    private float baseDropProbability3 = 0.006f;
    private float baseDropProbability4 = 0.0025f;
    private float baseDropProbability5 = 0.001f;

    private float luckBonusFactor = 0.1f;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        // 메시지 매니저 캐싱
        messageManager = GameObject.Find("MessageManager").GetComponent<MessageManager>();
    }

    // 회복
    public void Heal(int Heal_Size, int Add_Health_Size)
    {
        SoundManager.instance.PlaySFX(SoundManager.instance.Item_HealClip);

        if (GameManager.instance.health != GameManager.instance.maxHealth)
        {
            GameManager.instance.health += Heal_Size;
            if (GameManager.instance.health > GameManager.instance.maxHealth)
                GameManager.instance.health = GameManager.instance.maxHealth;

            messageManager.ShowMessage($"체력 회복!", 2f);
        }
        else
        {
            GameManager.instance.maxHealth += Add_Health_Size;
            GameManager.instance.health = GameManager.instance.maxHealth;

            messageManager.ShowMessage($"최대 체력 증가!");
        }
    }

    // 공격력 증가
    public void AtkBuff(Player player, int AtkbuffAmount)
    {
        SoundManager.instance.PlaySFX(SoundManager.instance.Item_AtkBuffClip);
        player.bulletDamage += AtkbuffAmount;

        messageManager.ShowMessage($"공격력 증가!");
    }

    // 이동속도 증가
    public void PlayerSpeedUp(Player player, float SpeedbuffAmount)
    {
        SoundManager.instance.PlaySFX(SoundManager.instance.Item_PlayerSpeedUpClip);

        if (player.speed > player.max_speed)
        {
            messageManager.ShowMessage($"이동속도가 이미 최대치입니다.");
        }
        else
        {
            player.speed *= SpeedbuffAmount;
            if (player.speed > player.max_speed)
                player.speed = player.max_speed;

            messageManager.ShowMessage($"이동속도 증가!");
        }
    }

    // 공격속도 증가
    public void AtkSpeedUp(Player player, int CooldownAmount)
    {
        if (player.maxCount > 1)
        {
            SoundManager.instance.PlaySFX(SoundManager.instance.Item_AtkSpeedUpClip);
            player.maxCount -= CooldownAmount;
            player.cooldownUI.UpdateMaxCount(player.maxCount);
            
            messageManager.ShowMessage($"공격 쿨타임");
        }
        else
        {
            messageManager.ShowMessage($"최소 쿨타임 달성!\n추가 공격력 증가!");
            AtkBuff(player, 3);
        }
    }

    // 방패 아이템 획득
    public void GetShield(Player player)
    {
        SoundManager.instance.PlaySFX(SoundManager.instance.Item_GetShieldClip);
        player.shieldItemCount += 1;
        player.UpdateShieldCountUI();

        messageManager.ShowMessage("방패 획득!");
    }

    // 치명타 확률 증가
    public void CriticalChanceUp(Player player, float CriticalChance_IncreaseAmount)
    {
        SoundManager.instance.PlaySFX(SoundManager.instance.Item_CriticalChanceUpClip);

        if (player.criticalChance < 1)
        {
            player.criticalChance += CriticalChance_IncreaseAmount;
            if (player.criticalChance > 1)
            {
                player.criticalChance = 1;
                messageManager.ShowMessage("치명타 확률 MAX");
            }
            else
            {
                messageManager.ShowMessage($"치명타 확률 증가!");
            }
        }
        else
        {
            CriticalMultiplier(player, 0.2f);
        }
    }

    // 치명타 데미지 증가
    public void CriticalMultiplier(Player player, float CriticalMultiplier_IncreaseAmount)
    {
        SoundManager.instance.PlaySFX(SoundManager.instance.Item_CriticalMultiplierClip);
        player.criticalMultiplier += CriticalMultiplier_IncreaseAmount;

        messageManager.ShowMessage($"치명타 데미지 증가!");
    }

    // 공격 횟수 증가
    public void IncreaseBullet(Player player)
    {
        SoundManager.instance.PlaySFX(SoundManager.instance.Item_IncreasedBulletClip);
        if(player.bulletCount >= 10)
        {
            messageManager.ShowMessage($"최대 발사 수치 도달!\n추가 공격력 증가!");
            AtkBuff(player, 30);
            return;
        }
        player.bulletCount += 1;

        messageManager.ShowMessage($"다중 발사 +1");
    }

    // 발사대 추가
    public void AddBulletFirePoint(Player player)
    {
        SoundManager.instance.PlaySFX(SoundManager.instance.Item_AddBulletFirePointClip);
        player.AddFirePoint();

        messageManager.ShowMessage($"멀티 샷 +1");
    }

    // 사거리 증가
    public void IncreasedRange(Player player, float Range_ScopeSize)
    {
        SoundManager.instance.PlaySFX(SoundManager.instance.Item_IncreasedRangeClip);
        player.bulletLifeTime += player.bulletLifeTime * Range_ScopeSize;

        messageManager.ShowMessage($"공격 사거리 증가!");
    }

    // 탄환 크기 증가
    public void IncreaseBulletSize(Player player, float BulletScale_IncreaseAmount)
    {
        SoundManager.instance.PlaySFX(SoundManager.instance.Item_IncreaseBulletSizeClip);
        player.bulletScaleMultiplier += BulletScale_IncreaseAmount;

        messageManager.ShowMessage($"탄환 크기 증가!");
    }

    // 부활
    public void Resurrection(Player player)
    {
        SoundManager.instance.PlaySFX(SoundManager.instance.Item_ResurrectionClip);
        player.Resurrection_Count += 1;
        player.UpdateResurrectionState();

        messageManager.ShowMessage($"부활아이템 획득!");
    }

    // 특수무기 추가
    public void Add_SpecialWeapon(Player player)
    {
        SoundManager.instance.PlaySFX(SoundManager.instance.Item_Add_SpecialWeaponClip);

        if (GameManager.instance.weapon != null)
        {
            GameManager.instance.weapon.LevelUp(player.bulletDamage, 1);
            messageManager.ShowMessage("특수무기가 추가됩니다.");
        }
    }

    public void MagneticField(Player player)
    {
        SoundManager.instance.PlaySFX(SoundManager.instance.Item_MagneticFieldClip);

        if (GameManager.instance.weapon1 != null)
        {
            if (!player.Have_MagneticField)
            {
                // 처음 획득 시: 자기장 생성 및 활성화
                GameManager.instance.weapon1.BatchMagneticFields();
                player.Have_MagneticField = true;
                messageManager.ShowMessage("자기장이 활성화 되었습니다.");
            }
            else
            {
                // 🔥기존 데미지에 1.5을 곱하여 50%씩 위력 강화
                GameManager.instance.weapon1.damage *= 1.5f;

                messageManager.ShowMessage("자기장의 위력이 대폭 강화됩니다!");
                Debug.Log("현재 자기장 데미지: " + GameManager.instance.weapon1.damage);
            }
        }
    }

    public void EnableExplosionBullet()
    {
        SoundManager.instance.PlaySFX(SoundManager.instance.Item_EnableExplosionBulletClip);
        TestBullet.Explosion_Bullet = true;

        messageManager.ShowMessage("폭발탄이 활성화 되었습니다.");
    }

    public void IncreaseDropProbability(Player player, int Add_Lucky_Level)
    {
        if (player.Lucky_Level >= 10)
        {
            player.Lucky_Level = 10;
            messageManager.ShowMessage("이미 최대 행운레벨에 도달하였습니다.");
            return;
        }

        player.Lucky_Level += Add_Lucky_Level;
        if (player.Lucky_Level > 10) player.Lucky_Level = 10;

        // 드랍률 재계산
        Enemy.dropProbability  = baseDropProbability  * (1 + player.Lucky_Level * luckBonusFactor);
        Enemy.dropProbability2 = baseDropProbability2 * (1 + player.Lucky_Level * luckBonusFactor);
        Enemy.dropProbability3 = baseDropProbability3 * (1 + player.Lucky_Level * luckBonusFactor);
        Enemy.dropProbability4 = baseDropProbability4 * (1 + player.Lucky_Level * luckBonusFactor);
        Enemy.dropProbability5 = baseDropProbability5 * (1 + player.Lucky_Level * luckBonusFactor);

        messageManager.ShowMessage("행운 레벨 증가!");

        Debug.Log(
            "드랍률1: " + Enemy.dropProbability + "\n" +
            "드랍률2: " + Enemy.dropProbability2 + "\n" +
            "드랍률3: " + Enemy.dropProbability3 + "\n" +
            "드랍률4: " + Enemy.dropProbability4 + "\n" +
            "드랍률5: " + Enemy.dropProbability5
        );
    }
}
