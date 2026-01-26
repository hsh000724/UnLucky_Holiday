using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CreditRoll : MonoBehaviour
{
    // 크레딧의 모든 요소(이미지, 텍스트)를 담는 컨테이너 RectTransform
    public RectTransform creditsContainer;

    // 크레딧 스크롤 속도
    public float scrollSpeed = 50f;

    // 크레딧 스크롤이 끝나는 위치 (보통 컨테이너의 높이)
    // 이 값을 Unity 에디터에서 설정하거나, Start()에서 동적으로 계산할 수 있습니다.
    private float endPosition;

    private bool isRolling = false;

    void Start()
    {
        // 컨테이너의 전체 높이를 계산하여 스크롤이 끝나는 지점을 설정합니다.
        // 예를 들어, 컨테이너의 높이 + 화면 높이의 절반 정도를 설정하여 
        // 마지막 요소가 화면 중앙을 지나 완전히 사라지도록 할 수 있습니다.
        endPosition = creditsContainer.rect.height + Screen.height * 20f;

        StartCredits();
    }

    void Update()
    {
        if (isRolling)
        {
            // 위로 스크롤
            creditsContainer.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;

            // 크레딧 스크롤 종료 확인
            // 현재 위치(y)가 계산된 끝 지점(endPosition)보다 커지면 멈춥니다.
            if (creditsContainer.anchoredPosition.y >= endPosition)
            {
                StopCredits();
            }
        }
    }

    public void StartCredits()
    {
        isRolling = true;

        // 시작 위치: 화면 하단에 컨테이너의 맨 아래가 위치하도록 설정
        // -Screen.height는 컨테이너의 맨 위가 화면 하단에 오는 위치이므로,
        // 컨테이너가 화면 바깥(아래)에서 시작하려면 컨테이너 높이의 절반을 고려합니다.
        // 하지만 간단하게 컨테이너의 맨 아래가 화면 하단에 오도록 하려면:
        // creditsContainer.anchoredPosition = new Vector2(0, -creditsContainer.rect.height); // 컨테이너 맨 위가 화면 아래

        // 보다 간단한 방법: 컨테이너의 맨 아래가 화면 하단에 위치하도록 설정 (앵커가 중앙일 경우)
        // creditsContainer.anchoredPosition = new Vector2(0, -Screen.height / 2f - creditsContainer.rect.height / 2f); 
        // **가장 간단하게** 앵커를 하단 중앙(0.5, 0)으로 설정하고, 시작 Y 위치를 0 또는 -50 정도로 설정합니다.

        // 앵커가 중앙(0.5, 0.5)이라고 가정하고, 화면 아래에서 시작하도록 설정
        creditsContainer.anchoredPosition = new Vector2(0, 0);
    }

    public void StopCredits()
    {
        isRolling = false;
        Debug.Log("크레딧이 종료되었습니다!");
        SceneManager.LoadScene("Result");
    }
}