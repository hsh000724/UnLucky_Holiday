using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("광고 보상 팝업 UI")]
    [SerializeField] private GameObject adRewardPopup;

    private void OnEnable()
    {
        if (AdManager.Instance != null)
            AdManager.Instance.OnRewardedAdCompleted += HandleAdResult;
    }

    private void OnDisable()
    {
        if (AdManager.Instance != null)
            AdManager.Instance.OnRewardedAdCompleted -= HandleAdResult;
    }

    private void HandleAdResult(bool success)
    {
        if (success)
        {
            if (ItemRechargeManager.instance != null)
                ItemRechargeManager.instance.AddRewardItem();
            CloseAdPopup();
        }
    }

    public void OnWatchAdButtonClick()
    {
        if (AdManager.Instance != null) AdManager.Instance.ShowRewardedAd();
    }

    public void ShowAdPopup() { if (adRewardPopup != null) adRewardPopup.SetActive(true); }
    public void CloseAdPopup() { if (adRewardPopup != null) adRewardPopup.SetActive(false); }

    private bool CheckAndConsumeItem()
    {
        if (ItemRechargeManager.instance != null)
        {
            bool hasItem = ItemRechargeManager.instance.TryUseItem();
            if (!hasItem) ShowAdPopup();
            return hasItem;
        }
        return false;
    }

    // --- 게임 시작 버튼용 메서드들 ---
    public void StartClassicMode() { if (CheckAndConsumeItem()) LoadSceneWithFade("ClassicMode", ModeManager.GameMode.Classic); }
    public void StartInfinityMode() { if (CheckAndConsumeItem()) LoadSceneWithFade("InfinityMode", ModeManager.GameMode.Infinity); }
    public void StartHardcoreMode() { if (CheckAndConsumeItem()) LoadSceneWithFade("HardcoreMode", ModeManager.GameMode.Hardcore); }

    private void LoadSceneWithFade(string sceneName, ModeManager.GameMode mode)
    {
        if (ModeManager.instance != null) ModeManager.instance.SetMode(mode);
        if (FadeManager.instance != null) FadeManager.instance.FadeToScene(sceneName);
        else SceneManager.LoadScene(sceneName);
    }

    public void OnRetryButton()
    {
        if (CheckAndConsumeItem())
        {
            Time.timeScale = 1f;
            if (ModeManager.instance != null) ModeManager.instance.RestartGame();
        }
    }

    public void QuitGame() { Application.Quit(); }
    public void ResumeAndLoadScene(string sceneName) { Time.timeScale = 1f; LoadScene(sceneName); }

    public void LoadScene(string sceneName)
    {
        if (FadeManager.instance != null) FadeManager.instance.FadeToScene(sceneName);
        else SceneManager.LoadScene(sceneName);
    }
}