using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro 사용

public class NicknameUI : MonoBehaviour
{
    public GameObject panel;
    public TMP_InputField nicknameInput;
    public Button confirmButton;
    private FirestoreManager firestore;

    void Start()
    {
        firestore = FindFirstObjectByType<FirestoreManager>();

        // 최초 실행 시 닉네임 등록창 표시
        if (!PlayerPrefs.HasKey("nickname"))
        {
            panel.SetActive(true);
        }
        else
        {
            panel.SetActive(false);
        }

        confirmButton.onClick.AddListener(OnConfirmClicked);
    }

    private async void OnConfirmClicked()
    {
        string nickname = nicknameInput.text.Trim();

        if (string.IsNullOrEmpty(nickname))
        {
            Debug.Log("닉네임을 입력하세요.");
            return;
        }

        bool available = await firestore.IsNicknameAvailable(nickname);

        if (available)
        {
            await firestore.SaveNickname(nickname);
            panel.SetActive(false);
            Debug.Log("닉네임 등록 완료: " + nickname);
        }
        else
        {
            Debug.Log("이미 사용 중인 닉네임입니다.");
        }
    }
}
