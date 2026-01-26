using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossDialogueManager : MonoBehaviour
{
    [Header("대사 목록")]
    [SerializeField] private List<BossDialogue> dialogueList;

    [Header("UI 요소")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private Text dialogueText;

    public IEnumerator ShowDialogueById(string id)
    {
        // ID로 대사 검색
        BossDialogue target = dialogueList.Find(d => d.id == id);

        if (target != null)
        {
            dialoguePanel.SetActive(true);
            dialogueText.text = target.dialogueText;
            yield return new WaitForSeconds(target.delayTime);
            dialoguePanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning($"ID '{id}'에 해당하는 대사를 찾을 수 없습니다.");
        }
    }
}
