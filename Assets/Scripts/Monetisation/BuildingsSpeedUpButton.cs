using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BuildingsSpeedUpButton : MonoBehaviour, IUnityAdsListener {

#if UNITY_IOS
  private string gameId = "4016688";
  string mySurfacingId = "Rewarded_iOS";
#elif UNITY_ANDROID || UNITY_EDITOR
  private string gameId = "4016689";
  string mySurfacingId = "Rewarded_Android";
#endif

  Button myButton;
  string buildingName = "";
  ControllerScript controller;

  void Start() {
    myButton = GetComponent<Button>();
    controller = GameObject.Find("GameController").GetComponent<ControllerScript>();
    // Set interactivity to be dependent on the Ad Unit or legacy Placement’s status:
    // Map the ShowRewardedVideo function to the button’s click listener:
    if (myButton)myButton.onClick.AddListener(ShowRewardedVideo);
    // Initialize the Ads listener and service:
    Advertisement.Initialize(gameId, false);
    myButton.interactable = Advertisement.IsReady(mySurfacingId);
    InvokeRepeating("activateButton", 0.5f, 0.5f);
  }

  public void activateButton() {
    myButton.interactable = Advertisement.IsReady(mySurfacingId);
  }

  // Implement a function for showing a rewarded video ad:
  void ShowRewardedVideo() {
    Advertisement.RemoveListener(this);
    Advertisement.AddListener(this);
    Advertisement.Show(mySurfacingId);
  }

  // Implement IUnityAdsListener interface methods:
  public void OnUnityAdsReady(string surfacingId) {
    // If the ready Ad Unit or legacy Placement is rewarded, activate the button: 
  }

  public void OnUnityAdsDidFinish(string surfacingId, ShowResult showResult) {
    // Define conditional logic for each ad completion status:
    if (showResult == ShowResult.Finished) {
      // Reward the user for watching the ad to completion.
      GameObject.Find("GameController").GetComponent<Buildings>().buildWithVideo();
      if (buildingName != "")controller.getUI().showPopupMessage(string.Format(Language.Field["BUILT"], buildingName));
    } else if (showResult == ShowResult.Skipped) {
      // Do not reward the user for skipping the ad.
      Debug.LogWarning("The ad did not finish.");
    } else if (showResult == ShowResult.Failed) {
      Debug.LogWarning("The ad did not finish due to an error.");
    }
    Advertisement.RemoveListener(this);
  }

  public void OnUnityAdsDidError(string message) {
    Debug.Log("ADS ERROR: " + message);
  }

  public void OnUnityAdsDidStart(string surfacingId) {
    Debug.Log("AD SHOWN, ID: " + surfacingId);
  }

  public void setBuildingName(string b) {
    buildingName = b;
  }
}