using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ModeManager : MonoBehaviour
{
    public enum GameMode { Classic, Infinity, Hardcore, Battle }
    public static ModeManager instance;
    public GameMode currentMode;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 씬이 전환되어도 파괴되지 않게
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetMode(GameMode mode)
    {
        currentMode = mode;
    }

    public void RestartGame()
    {
        switch (currentMode)
        {
            case GameMode.Classic:
                FadeManager.instance.FadeToScene("ClassicMode");
                break;
            case GameMode.Infinity:
                FadeManager.instance.FadeToScene("InfinityMode");
                break;
            case GameMode.Hardcore:
                FadeManager.instance.FadeToScene("HardcoreMode");
                break;
            case GameMode.Battle:
                FadeManager.instance.FadeToScene("BattleMode"); // ← 추가
                break;
        }
    }

}
