using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class RewardedAdsButton : MonoBehaviour, IUnityAdsListener {

#if UNITY_IOS
  private string gameId = "4016688";
  string mySurfacingId = "Rewarded_iOS";
#elif UNITY_ANDROID || UNITY_EDITOR
  private string gameId = "4016689";
  string mySurfacingId = "Rewarded_Android";
#endif

  Button myButton;
  ControllerScript controller;

  [Header("Reward")]
  public int pearlsNumber = 3;

  void Start() {
    myButton = GetComponent<Button>();
    controller = GameObject.Find("GameController").GetComponent<ControllerScript>();
    // Map the ShowRewardedVideo function to the button’s click listener:
    if (myButton)myButton.onClick.AddListener(ShowRewardedVideo);
    // Initialize the Ads listener and service:
    Advertisement.Initialize(gameId, false);
  }

  // Implement a function for showing a rewarded video ad:
  void ShowRewardedVideo() {
    Advertisement.RemoveListener(this);
    Advertisement.AddListener(this);
    Advertisement.Show(mySurfacingId);
  }

  public void OnUnityAdsDidFinish(string surfacingId, ShowResult showResult) {
    // Define conditional logic for each ad completion status:
    if (showResult == ShowResult.Finished) {
      // Reward the user for watching the ad to completion.
      if (pearlsNumber > 0) {
        controller.getUser().increasePearl(pearlsNumber);
        controller.getUI().showPopupMessage(string.Format(Language.Field["REWARD"], pearlsNumber.ToString()));
      }
    } else if (showResult == ShowResult.Skipped) {
      // Do not reward the user for skipping the ad.
      Debug.LogWarning("The ad did not finish.");
    } else if (showResult == ShowResult.Failed) {
      Debug.LogWarning("The ad did not finish due to an error.");
    }
    Advertisement.RemoveListener(this);
  }

  public void OnUnityAdsReady(string surfacingId) {
    // If the ready Ad Unit or legacy Placement is rewarded, activate the button: 
  }

  public void OnUnityAdsDidError(string message) {
    Debug.Log("ADS ERROR: " + message);
  }

  public void OnUnityAdsDidStart(string surfacingId) {
    Debug.Log("AD SHOWN, ID: " + surfacingId);
  }
}