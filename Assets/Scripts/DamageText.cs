using TMPro;
using UnityEngine;

public class DamageText : MonoBehaviour
{
    public float moveUpSpeed = 1f;
    public float lifeTime = 1f;

    public TextMeshProUGUI textMesh;

    void Start()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        Destroy(gameObject, lifeTime);
    }

    public void SetDamage(int damage)
    {
        if (textMesh != null)
        {
            Debug.Log("데미지 출력");
            textMesh.text = damage.ToString();
        }
        else
        {
            Debug.LogWarning("TextMeshPro가 연결되지 않았습니다.");
        }
    }

    void Update()
    {
        transform.position += Vector3.up * moveUpSpeed * Time.deltaTime;
    }
}
