using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Google;
using UnityEngine;
using UnityEngine.UI;

public class GoogleSignInDemo : MonoBehaviour
{
    private FirebaseAuth auth;
    private GoogleSignInConfiguration googleConfig;

    private void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        googleConfig = new GoogleSignInConfiguration
        {
            WebClientId = "853773184056-rbt60sjs34um28k79sdpi5cr4djupa99.apps.googleusercontent.com",
            RequestEmail = true,
            RequestIdToken = true
        };
    }

    public void SignInWithGoogle()
    {
        GoogleSignIn.Configuration = googleConfig;
        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnGoogleSignIn);
    }

    private void OnGoogleSignIn(Task<GoogleSignInUser> task)
    {
        if (task.IsFaulted)
        {
            Debug.LogError("Google Sign-In failed: " + task.Exception.Message);
        }
        else if (task.IsCanceled)
        {
            Debug.Log("Google Sign-In was canceled.");
        }
        else
        {
            Debug.Log("Google Sign-In Success");
            FirebaseGoogleAuth(task.Result.IdToken);
        }
    }

    private void FirebaseGoogleAuth(string idToken)
    {
        Credential credential = GoogleAuthProvider.GetCredential(idToken, null);
        auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Firebase Sign-In failed: " + task.Exception.Message);
            }
            else
            {
                Debug.Log("Firebase Sign-In Success" + auth.CurrentUser.DisplayName);
                // Handle successful sign-in, e.g., update UI or load next scene
            }
        });
    }
}