using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject pauseMenuUI;
    public GameObject StatusUI;
    public GameObject MiniMap;

    [Header("Game UI")]
    public GameObject Controller;
    public GameObject Collection;
    public GameObject Status_Btn;
    public GameObject Atk_Btn;
    public GameObject Shield_Btn;

    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 1️⃣ 퍼즈 중 + 스테이터스 창 열림 → 스테이터스 먼저 닫기
            if (isPaused && StatusUI.activeSelf)
            {
                Resume_Status();
                return;
            }

            // 2️⃣ 퍼즈 중 → 퍼즈 해제
            if (isPaused)
            {
                Resume();
            }
            // 3️⃣ 게임 중 → 퍼즈
            else
            {
                Pause();
            }
        }
    }

    // =========================
    // Pause / Resume
    // =========================

    public void Pause()
    {
        pauseMenuUI.SetActive(true);

        Controller.SetActive(false);
        Collection.SetActive(false);
        Atk_Btn.SetActive(false);
        Shield_Btn.SetActive(false);

        MiniMap.SetActive(false);
        Status_Btn.SetActive(true);

        Time.timeScale = 0f;
        isPaused = true;
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);

        Controller.SetActive(true);
        Collection.SetActive(true);
        Atk_Btn.SetActive(true);
        Shield_Btn.SetActive(true);

        MiniMap.SetActive(true);
        Status_Btn.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;
    }

    // =========================
    // Status UI
    // =========================

    public void Pause_Status()
    {
        StatusUI.SetActive(true);

        Status_Btn.SetActive(false);
        Controller.SetActive(false);
        Collection.SetActive(false);
        Atk_Btn.SetActive(false);
        Shield_Btn.SetActive(false);

        MiniMap.SetActive(false);

        Time.timeScale = 0f;
        isPaused = true;
    }

    public void Resume_Status()
    {
        StatusUI.SetActive(false);

        Status_Btn.SetActive(true);
        Controller.SetActive(true);
        Collection.SetActive(true);
        Atk_Btn.SetActive(true);
        Shield_Btn.SetActive(true);

        MiniMap.SetActive(false); // 퍼즈 상태 유지

        Time.timeScale = 0f;
        isPaused = true;
    }

    // =========================
    // Scene Control
    // =========================

    public void RestartGame()
    {
        Time.timeScale = 1f;
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void BackToTitle()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Title");
    }

    public void QuitGame()
    {
        Debug.Log("게임 종료!");
        Application.Quit();
    }
}
