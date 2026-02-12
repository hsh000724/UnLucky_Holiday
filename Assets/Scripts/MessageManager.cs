using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MessageManager : MonoBehaviour
{
    public GameObject messagePrefab;
    public Transform messageParent;
    public int maxMessages = 5;

    public Transform target;
    public Vector3 offset = new Vector3(0, 1f, 0);

    public float floatDistance = 50f; // 떠오르는 거리 (픽셀)

    private Queue<GameObject> messageQueue = new Queue<GameObject>();
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
    }

    // 🔹 기존 시그니처 유지
    public void ShowMessage(string msg, float duration = 5f, Color? color = null)
    {
        GameObject messageGO = Instantiate(messagePrefab, messageParent);

        Text msgText = messageGO.GetComponent<Text>();
        msgText.text = msg;
        msgText.color = color ?? Color.white;

        CanvasGroup canvasGroup = messageGO.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = messageGO.AddComponent<CanvasGroup>();

        messageQueue.Enqueue(messageGO);

        if (messageQueue.Count > maxMessages)
        {
            GameObject oldMsg = messageQueue.Dequeue();
            Destroy(oldMsg);
        }

        StartCoroutine(RemoveAfterDelay(messageGO, canvasGroup, duration));
    }

    IEnumerator RemoveAfterDelay(GameObject messageGO, CanvasGroup canvasGroup, float delay)
    {
        float time = 0f;

        while (time < delay && messageGO != null)
        {
            if (target != null)
            {
                Vector3 worldPos = target.position + offset;
                Vector3 screenPos = mainCam.WorldToScreenPoint(worldPos);

                // 위로 떠오르는 값
                float floatOffset = Mathf.Lerp(0f, floatDistance, time / delay);

                messageGO.transform.position = screenPos + Vector3.up * floatOffset;
            }

            // 점점 사라지기
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, time / delay);

            time += Time.deltaTime;
            yield return null;
        }

        if (messageGO != null)
            Destroy(messageGO);
    }
}
