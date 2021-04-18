using System.Collections;
using Facebook.Unity;
using UnityEngine;

public class FacebookAnalytics : MonoBehaviour {

  void Awake() {
    if (Application.platform == RuntimePlatform.IPhonePlayer) {
      Application.targetFrameRate = 60;
    }
    FB.Init(FBInitCallback);
  }

  private void FBInitCallback() {
    if (FB.IsInitialized) {
      FB.ActivateApp();
    }
  }

  public void OnApplicationPause​(bool paused) {
    if (!paused) {
      if (FB.IsInitialized) {
        FB.ActivateApp();
      }
    }
  }
}