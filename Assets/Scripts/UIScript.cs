using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class UIScript : MonoBehaviour
{
  [Header("Resources and Bounty")]
  public Text woodValue;
  public Text stoneValue;
  public Text goldValue;
  public Text pearlValue;
  public Slider bountyObject;

  [Header("Loading and Connection")]
  public GameObject loadingScreen;
  public GameObject loadingSpinner;
  public GameObject loadingButtons;
  public GameObject connectionError;

  [Header("Buttons")]
  public GameObject buttons;
  public Text buildingsButton;
  public Text mapButton;
  public Text accountButton;

  [Header("Message Popup")]
  public GameObject messagePopup;

  [Header("Account page")]
  public GameObject accountMenu;
  public GameObject accountMenuClose;
  public GameObject defaultMenuScreen;
  public Text accountMenuTitle;
  public Text notRegisteredText;
  public Text accountMenuUsername;
  public Text levelText;
  public Text accountMenuLevel;
  public Text bountyText;
  public Text accountMenuBounty;
  public Text storageText;
  public Text accountMenuStorage;
  public Text passwordRecoveryButton;
  public Text passwordRecoveryText;
  public Text sendEmailButton;
  public Text notLoggedRegisterButton;
  public Text notLoggedLoginButton;
  public Text registrationRegisterButton;
  public Text loginFormLoginButton;

  int previousBounty;
  int userLevel;
  ControllerScript controller;

  void Start(){
    controller = GameObject.Find("GameController").GetComponent<ControllerScript>();
    showLoadingScreen();
    // Populate all texts in the device language
    buildingsButton.text = Language.Field["BUILDINGS"];
    mapButton.text = Language.Field["MAP"];
    accountButton.text = Language.Field["ACCOUNT"];
    accountMenuTitle.text = Language.Field["ACCOUNT"];
    notRegisteredText.text = Language.Field["NOT_REGISTERED"];
    levelText.text = Language.Field["LEVEL"]+":";
    bountyText.text = Language.Field["BOUNTY"]+":";
    storageText.text = Language.Field["STORAGE"]+":";
    passwordRecoveryButton.text = Language.Field["FORGOT_PASSWORD"];
    passwordRecoveryText.text = Language.Field["RECOVER_PASSWORD"];
    sendEmailButton.text = Language.Field["SEND"].ToUpper();
    notLoggedRegisterButton.text = Language.Field["REGISTER"].ToUpper();
    notLoggedLoginButton.text = Language.Field["LOGIN_BTN"].ToUpper();
    registrationRegisterButton.text = Language.Field["REGISTER"].ToUpper();
    loginFormLoginButton.text = Language.Field["LOGIN_BTN"].ToUpper();
    connectionError.GetComponentInChildren<Text>().text = "Oops!\n"+ Language.Field["CONNECTION"];
    InvokeRepeating("updateAccountMenu", 0.0f, 10.0f);
  }

  void Update()
  {
    // Update resources on UI
    int[] resources = controller.getUser().getResources();
    woodValue.text = resources[0].ToString();
    stoneValue.text = resources[1].ToString();
    goldValue.text = resources[2].ToString();
    pearlValue.text = controller.getUser().getPearl().ToString();
    // Update bounty progress on UI
    updateBounty(controller.getUser().getBounty());
  }

  public void showPopupMessage(string message)
  {
    Text messageText = messagePopup.GetComponentInChildren(typeof(Text), true) as Text;
    messageText.text = message;
    messagePopup.SetActive(true);
  }

  void updateBounty(int value){
    if (value == previousBounty) return;
    userLevel = controller.getUser().getLevel();
    bountyObject.maxValue = userLevel*100;
    if (value >= userLevel*100){
      controller.getUser().increaseLevel();
      bountyObject.maxValue = controller.getUser().getLevel()*100;
      bountyObject.value = value;
      showPopupMessage(Language.Field["BOUNTY_LEVELUP"] + " " + controller.getUser().getLevel() + "!");
      playSuccessSound();
    } else {
      bountyObject.value = value;
    }
    bountyObject.transform.GetComponentInChildren<Text>().text = formatNumber(value);
    previousBounty = value;
  }

  string formatNumber(int number){
    string stringNumber = number.ToString();
    if (number < 1000) return stringNumber;
    if (number < 10000) return stringNumber[0] + (stringNumber[1]=='0' ? "" : "."+stringNumber[1])+"K";
    if (number < 100000) return stringNumber[0].ToString() + stringNumber[1].ToString() + "K";
    if (number < 1000000) return stringNumber[0].ToString() + stringNumber[1].ToString() + stringNumber[2].ToString() + "K";
    return stringNumber;
  }
  
  public void playSuccessSound(){
    messagePopup.GetComponent<AudioSource>().Play();
  }

  public void showLoadingScreen(){
    loadingScreen.SetActive(true);
  }

  public void hideLoadingScreen(){
    accountMenuClose.SetActive(true);
    loadingScreen.SetActive(false);
  }

  public void hideAccountMenu(){
    accountMenu.SetActive(false);
  }

  public void showConnectionError(bool error){
    connectionError.SetActive(error);
  }

  public void showButtons(bool show=true){
    buttons.SetActive(show);
  }

  public void updateAccountMenu(){
    User user = controller.getUser();
    accountMenuUsername.text = API.GetUsername();
    accountMenuLevel.text = user.getLevel().ToString();
    accountMenuBounty.text = user.getBounty().ToString();
    accountMenuStorage.text = user.getStorage().ToString();
  }

  public void emailRecoveryError(string message) {
    GameObject recoveryScreen = GameObject.Find("PasswordRecovery");
    foreach (Transform child in recoveryScreen.transform) {
      if (child.name == "Spinner") {
        child.gameObject.SetActive(false);
      } else if (child.name == "Send") {
        child.gameObject.SetActive(true);
      } else if (child.name == "Input") {
        Outline outline = child.GetComponent<Outline>();
        outline.enabled = true;
      }
    }
    passwordRecoveryText.text = message;
  }

  public void emailRecoverySuccess(string message) {
    GameObject recoveryScreen = GameObject.Find("PasswordRecovery");
    foreach (Transform child in recoveryScreen.transform) {
      if (child.name == "Spinner") {
        child.gameObject.SetActive(false);
      } else if (child.name == "Send") {
        child.gameObject.SetActive(true);
      }
    }
    recoveryScreen.SetActive(false);
    defaultMenuScreen.SetActive(true);
    accountMenu.SetActive(false);
    buttons.SetActive(true);
    passwordRecoveryText.text = Language.Field["RECOVER_PASSWORD"];
    showPopupMessage(message);
  }

  public void showNewGameOrLogin() {
    loadingSpinner.SetActive(false);
    loadingButtons.SetActive(true);
  }
}