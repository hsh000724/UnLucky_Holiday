using UnityEngine;

public class TitleUIManager : MonoBehaviour
{
    public GameObject difficultyPanel;
    public GameObject optionPanel;
    public GameObject helpPanel;
    public GameObject rankingPanel;
    public GameObject rankingIcon;

    // 모든 패널 비활성화
    void HideAllPanels()
    {
        difficultyPanel.SetActive(false);
        optionPanel.SetActive(false);
        helpPanel.SetActive(false);
        rankingPanel.SetActive(false);
        rankingIcon.SetActive(false);
    }

    public void OnStartButtonClicked()
    {
        HideAllPanels();
        difficultyPanel.SetActive(true);
    }

    public void OnOptionButtonClicked()
    {
        HideAllPanels();
        optionPanel.SetActive(true);
    }

    public void OnHelpButtonClicked()
    {
        HideAllPanels();
        helpPanel.SetActive(true);
    }
    public void OnBackButtonClicked()
    {
        HideAllPanels();
        rankingIcon.SetActive(true);
    }
}
