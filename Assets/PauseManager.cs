using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public GameObject StatusUI;
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
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Controller.SetActive(true);
        Collection.SetActive(true);
        Atk_Btn.SetActive(true);
        Shield_Btn.SetActive(true);
        Time.timeScale = 1f;  // 게임 시간 재개
        isPaused = false;
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Controller.SetActive(false);
        Collection.SetActive(false);
        Atk_Btn.SetActive(false);
        Shield_Btn.SetActive(false);
        Time.timeScale = 0f;  // 게임 시간 정지
        isPaused = true;
    }
    public void Pause_Status()
    {
        StatusUI.SetActive(true);
        Status_Btn.SetActive(false);
        Controller.SetActive(false);
        Collection.SetActive(false);
        Atk_Btn.SetActive(false);
        Shield_Btn.SetActive(false);
        Time.timeScale = 0f;  // 게임 시간 정지
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
        Time.timeScale = 1f;  // 게임 시간 재개
        isPaused = false;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // 재시작 전에 시간 되돌리기
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
    public void QuitGame()
    {
        Debug.Log("게임 종료!");
        Application.Quit();  // 실행 파일일 때 종료됨
    }

    public void BackToTitle()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Title");
    }
}
