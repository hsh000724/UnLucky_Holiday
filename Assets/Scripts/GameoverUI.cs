using UnityEngine;
using UnityEngine.UI;

public class GameoverUI : MonoBehaviour
{
    public Text survivalTimeText;
    public Text stageCountText;
    public Button restartButton;

    private void Start()
    {

        float survivalTime = GameManager.instance.gameTime;
        int stageCount = GameManager.instance.stageCount;

        survivalTimeText.text = "Survival Time: " + FormatTime(survivalTime);
        stageCountText.text = "Stage: " + stageCount;

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(() =>
            {
                ModeManager.instance.RestartGame();
            });
        }
    }

    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);
        return string.Format("{0:D2}:{1:D2}", minutes, seconds);
    }
}
