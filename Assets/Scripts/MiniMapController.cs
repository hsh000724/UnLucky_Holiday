using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MiniMapController : MonoBehaviour
{
    [Header("Reference")]
    public Transform player;
    public RectTransform miniMapRect;
    public RectTransform boxContainer;

    [Header("Prefabs")]
    public GameObject normalBoxDotPrefab;
    public GameObject rareBoxDotPrefab;

    [Header("Settings")]
    public float worldToMiniMapScale = 0.05f;
    public float miniMapRadius = 120f;

    private Dictionary<Transform, RectTransform> boxDots = new();

    void Update()
    {
        foreach (var pair in boxDots)
        {
            Transform box = pair.Key;
            RectTransform dot = pair.Value;

            Vector2 offset = box.position - player.position;
            Vector2 mapPos = offset * worldToMiniMapScale;

            if (mapPos.magnitude > miniMapRadius)
            {
                dot.gameObject.SetActive(false);
                continue;
            }

            dot.gameObject.SetActive(true);
            dot.anchoredPosition = mapPos;
        }
    }

    public void RegisterBox(Transform box, bool isRare)
    {
        GameObject prefab = isRare ? rareBoxDotPrefab : normalBoxDotPrefab;
        RectTransform dot = Instantiate(prefab, boxContainer).GetComponent<RectTransform>();
        boxDots.Add(box, dot);
    }

    public void UnregisterBox(Transform box)
    {
        if (!boxDots.ContainsKey(box)) return;
        Destroy(boxDots[box].gameObject);
        boxDots.Remove(box);
    }
}
