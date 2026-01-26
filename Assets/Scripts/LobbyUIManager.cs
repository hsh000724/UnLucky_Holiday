using UnityEngine;
using UnityEngine.UI; // Legacy UI 사용을 위해 추가

public class LobbyUIManager : MonoBehaviour
{
    // TMP_Text 대신 기본 Text 컴포넌트를 사용합니다.
    public Text welcomeText;

    void Start()
    {
        // AuthManager에 캐싱된 닉네임 정보를 가져와 텍스트를 갱신합니다.
        if (AuthManager.Instance != null && !string.IsNullOrEmpty(AuthManager.Instance.UserNickname))
        {
            welcomeText.text = $"{AuthManager.Instance.UserNickname}님 환영합니다!";
        }
        else
        {
            welcomeText.text = "도전자님 환영합니다!";
        }
    }
}