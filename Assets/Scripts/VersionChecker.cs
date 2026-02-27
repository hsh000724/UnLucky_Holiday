using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class VersionChecker : MonoBehaviour
{
    string versionURL = "https://hsh000724.github.io/UnLucky_Holiday/version.json";

    public GameObject forceUpdateUI;
    public GameObject optionalUpdateUI;

    private string storeUrl; // 🔥 JSON에서 받아올 스토어 URL

    void Start()
    {
        StartCoroutine(CheckVersion());
    }

    IEnumerator CheckVersion()
    {
        UnityWebRequest request = UnityWebRequest.Get(versionURL);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("버전 체크 실패 → 그냥 실행");
            yield break;
        }

        Debug.Log("JSON 내용: " + request.downloadHandler.text);

        VersionData data = JsonUtility.FromJson<VersionData>(request.downloadHandler.text);

        if (data == null)
        {
            Debug.LogError("JSON 파싱 실패");
            yield break;
        }

        storeUrl = data.update_url; // 🔥 URL 저장
        Debug.Log("스토어 URL: " + storeUrl);

        Version current = new Version(Application.version);
        Version min = new Version(data.min_version);
        Version latest = new Version(data.latest_version);

        Debug.Log("현재 버전: " + current);
        Debug.Log("최소 버전: " + min);
        Debug.Log("최신 버전: " + latest);

        if (current.CompareTo(min) < 0)
        {
            Debug.Log("강제 업데이트 필요");
            forceUpdateUI.SetActive(true);
        }
        else if (current.CompareTo(latest) < 0)
        {
            Debug.Log("선택 업데이트 가능");
            optionalUpdateUI.SetActive(true);
        }
        else
        {
            Debug.Log("최신 버전 → 정상 실행");
        }
    }

    // 🔥 버튼에서 호출할 함수 (매개변수 없음)
    public void OpenStore()
    {
        if (!string.IsNullOrEmpty(storeUrl))
        {
            Application.OpenURL(storeUrl);
        }
        else
        {
            Debug.LogError("storeUrl이 비어있음!");
        }
    }

    public void ContinueGame()
    {
        optionalUpdateUI.SetActive(false);
    }
}

[Serializable]
public class VersionData
{
    public string min_version;
    public string latest_version;
    public string update_url;
}