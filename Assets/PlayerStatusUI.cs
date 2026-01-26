using UnityEngine;
using UnityEngine.UI;

public class PlayerStatusUI : MonoBehaviour
{
    // 1. 닉네임 표시를 위한 변수 추가
    public Text Player_Nickname;

    public Text Player_HP;
    public Text Bullet_Damage;
    public Text Bullet_Size;
    public Text Player_Speed;
    public Text Attack_Range;
    public Text Attack_Cooldown;
    public Text Multi_Bullet;
    public Text Bullet_Speed;
    public Text Multi_Firepoint;
    public Text Resurrection;
    public Text Bullet_Bomb;
    public Text Lucky_Level;
    public Text Critical_Chance;
    public Text MagneticField;
    public Text Shield;
    public Text Critical_Multiplier;

    private Player player;

    void Start()
    {
        player = FindFirstObjectByType<Player>();

        // 2. 시작할 때 닉네임 설정
        UpdateNickname();
    }

    // 3. 데이터가 나중에 바뀔 수도 있으므로 OnEnable에서도 갱신
    void OnEnable()
    {
        UpdateNickname();

        // 만약 게임 도중에 닉네임이 바뀔 수 있다면 이벤트 구독
        if (AuthManager.Instance != null)
        {
            AuthManager.Instance.OnUserDataChanged += UpdateNickname;
        }
    }

    void OnDisable()
    {
        if (AuthManager.Instance != null)
        {
            AuthManager.Instance.OnUserDataChanged -= UpdateNickname;
        }
    }

    private void UpdateNickname()
    {
        if (AuthManager.Instance != null && Player_Nickname != null)
        {
            // AuthManager에 저장된 닉네임 가져오기
            string nick = AuthManager.Instance.UserNickname;
            Player_Nickname.text = string.IsNullOrEmpty(nick) ? "Unknown" : nick;
        }
    }

    void Update()
    {
        if (player == null || !gameObject.activeSelf) return;

        Player_HP.text = $"{player.currentHealth} / {player.maxHealth}";
        Bullet_Damage.text = $"공격력: {player.bulletDamage}";
        Bullet_Size.text = $"탄환 크기: {player.bulletScaleMultiplier:F1}";
        Player_Speed.text = $"이동속도: {player.speed:F1}";
        Attack_Range.text = $"공격 사거리: {player.bulletLifeTime:F2}";
        Attack_Cooldown.text = $"공격 쿨타임: {player.maxCount}";
        Multi_Bullet.text = $"연사: {player.bulletCount}";
        Bullet_Speed.text = $"탄환 속도: {player.bulletSpeed:F1}";
        Multi_Firepoint.text = $"발사 지점 수: {player.firePoints.Count}";
        Resurrection.text = $"부활 횟수: {player.Resurrection_Count}";
        Bullet_Bomb.text = $"폭발탄 {(TestBullet.Explosion_Bullet ? "활성화" : "비활성화")}";
        Lucky_Level.text = $"행운 : {player.Lucky_Level} 단계";
        Critical_Chance.text = $"치명타 확률: {player.criticalChance * 100f:F0}%";
        MagneticField.text = $"자기장 {(player.Have_MagneticField ? "활성화" : "비활성화")}";
        Shield.text = $"쉴드 수: {player.shieldItemCount}";
        Critical_Multiplier.text = $"치명타 배율: x{player.criticalMultiplier:F1}";
    }
}