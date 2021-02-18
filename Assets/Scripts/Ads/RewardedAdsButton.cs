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
  public int pearlsNumber = 0;

  void Start() {
    myButton = GetComponent<Button>();
    controller = GameObject.Find("GameController").GetComponent<ControllerScript>();
    // Set interactivity to be dependent on the Ad Unit or legacy Placement’s status:
    gameObject.SetActive(Advertisement.IsReady(mySurfacingId));
    // Map the ShowRewardedVideo function to the button’s click listener:
    if (myButton)myButton.onClick.AddListener(ShowRewardedVideo);
    // Initialize the Ads listener and service:
    Advertisement.AddListener(this);
    Advertisement.Initialize(gameId, true);
  }

  // Implement a function for showing a rewarded video ad:
  void ShowRewardedVideo() {
    Advertisement.Show(mySurfacingId);
  }

  // Implement IUnityAdsListener interface methods:
  public void OnUnityAdsReady(string surfacingId) {
    // If the ready Ad Unit or legacy Placement is rewarded, activate the button: 
    if (surfacingId == mySurfacingId) {
      gameObject.SetActive(true);
    }
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
  }

  public void OnUnityAdsDidError(string message) {
    // Log the error.
  }

  public void OnUnityAdsDidStart(string surfacingId) {
    // Optional actions to take when the end-users triggers an ad.
  }
}