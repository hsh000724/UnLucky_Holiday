using UnityEngine;
using UnityEngine.UI;

public class OptionPanelManager : MonoBehaviour
{
    [Header("패널들")]
    public GameObject volumePanel;
    public GameObject gamePanel;
    public GameObject accountPanel;
    public GameObject tutorialPanel;

    [Header("버튼들")]
    public Button volumeButton;
    public Button gameButton;
    public Button accountButton;
    public Button tutorialButton;

    void Start()
    {
        // 버튼 클릭 시 해당 패널 열기
        volumeButton.onClick.AddListener(() => ShowPanel(volumePanel));
        gameButton.onClick.AddListener(() => ShowPanel(gamePanel));
        accountButton.onClick.AddListener(() => ShowPanel(accountPanel));
        tutorialButton.onClick.AddListener(() => ShowPanel(tutorialPanel));

        // 시작 시 첫 번째 패널(음량 설정)만 열기
        ShowPanel(volumePanel);
    }

    private void ShowPanel(GameObject targetPanel)
    {
        // 모든 패널 닫기
        volumePanel.SetActive(false);
        gamePanel.SetActive(false);
        accountPanel.SetActive(false);
        tutorialPanel.SetActive(false);

        // 선택된 패널만 열기
        if (targetPanel != null)
            targetPanel.SetActive(true);
    }
}
