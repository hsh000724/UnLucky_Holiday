using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MessageManager : MonoBehaviour
{
    public GameObject messagePrefab;         // 텍스트 프리팹 (Text or TMP)
    public Transform messageParent;          // 메시지를 담을 부모 (Vertical Layout Group 추천)
    public int maxMessages = 5;

    private Queue<GameObject> messageQueue = new Queue<GameObject>();

    public void ShowMessage(string msg, float duration = 5f, Color? color = null)
    {
        // 메시지 오브젝트 생성
        GameObject messageGO = Instantiate(messagePrefab, messageParent);
        Text msgText = messageGO.GetComponent<Text>();
        msgText.text = msg;
        msgText.color = color ?? Color.white;

        messageQueue.Enqueue(messageGO);

        // 오래된 메시지 제거
        if (messageQueue.Count > maxMessages)
        {
            GameObject oldMsg = messageQueue.Dequeue();
            Destroy(oldMsg);
        }

        // 일정 시간 후 자동 제거
        StartCoroutine(RemoveAfterDelay(messageGO, duration));
    }

    IEnumerator RemoveAfterDelay(GameObject messageGO, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (messageQueue.Contains(messageGO))
        {
            messageQueue = new Queue<GameObject>(messageQueue); // 안전하게 복사
            messageQueue.Dequeue(); // 하나 빼기
        }

        Destroy(messageGO);
    }
}
