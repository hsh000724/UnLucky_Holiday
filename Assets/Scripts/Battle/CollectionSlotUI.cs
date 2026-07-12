using UnityEngine;
using UnityEngine.UI;

public class CollectionSlotUI : MonoBehaviour
{
    [Header("아이템 ID (CollectibleType과 동일하게)")]
    public string collectionId;  // ex) "Fire", "Water", "Wind" 등

    [Header("아이템 원본 스프라이트")]
    public Sprite itemSprite;    // 실제 아이템 이미지

    [Header("컴포넌트")]
    public Image itemImage;      // 아이템 이미지 렌더링

    // 수집 전: 검은 실루엣
    // 수집 후: 원본 컬러 이미지
    public void SetAcquired(bool acquired)
    {
        if (itemImage == null || itemSprite == null) return;

        itemImage.sprite = itemSprite;

        if (acquired)
        {
            // 원본 컬러로 복원
            itemImage.color = Color.white;
        }
        else
        {
            // 검은 실루엣
            itemImage.color = Color.black;
        }
    }

    // 수집 애니메이션 (선택사항)
    public System.Collections.IEnumerator AcquireAnimation()
    {
        float duration = 0.4f;
        float elapsed = 0f;

        Color startColor = Color.black;
        Color endColor = Color.white;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            itemImage.color = Color.Lerp(startColor, endColor, elapsed / duration);
            yield return null;
        }

        itemImage.color = Color.white;
    }
}