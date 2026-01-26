using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon1 : MonoBehaviour
{
    public int magneticFieldPrefabId; // 자기장 프리팹 ID
    public float damage = 10f;        // 기본 데미지 설정
    public int count = 1;             // 생성할 자기장 개수

    private List<GameObject> activeMagneticFields = new List<GameObject>();

    public void Init()
    {
        BatchMagneticFields();
    }

    public void BatchMagneticFields()
    {
        for (int index = 0; index < count; index++)
        {
            GameObject magneticField = GameManager.instance.pool.Get(magneticFieldPrefabId);
            magneticField.transform.parent = transform;

            magneticField.transform.localPosition = new Vector3(0, -5, 0);
            magneticField.transform.localRotation = Quaternion.identity;

            magneticField.SetActive(true);
            activeMagneticFields.Add(magneticField);
        }
    }
}