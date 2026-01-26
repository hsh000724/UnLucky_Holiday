using Firebase;
using Firebase.Auth;
using UnityEngine;

public class FirebaseInit : MonoBehaviour
{
    private FirebaseAuth auth;

    private void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            auth = FirebaseAuth.DefaultInstance;
            Debug.Log("Firebase 초기화 성공!");
        });
    }
}
