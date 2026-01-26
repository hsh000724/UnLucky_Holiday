using UnityEngine;
using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;

public class FirestoreInit : MonoBehaviour
{
    public FirebaseFirestore db;

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                Debug.Log("Firestore 초기화 성공");
                db = FirebaseFirestore.DefaultInstance;
            }
            else
            {
                Debug.LogError("Firestore 초기화 실패");
            }
        });
    }
}
