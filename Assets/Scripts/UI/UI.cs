using System;
using System.Collections.Generic;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour {
  [Header("Resources and Bounty")]
  public GameObject hud;
  public Text woodValue;
  public Text stoneValue;
  public Text goldValue;
  public Text pearlValue;

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
  public GameObject leaderboardButton;

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

  [Header("Leaderboard menu")]
  public GameObject localLeaderboard;
  public GameObject absoluteLeaderboard;
  public GameObject leaderboardEntryPrefab;
  public Text leaderboardTitle;
  public Text localLeaderboardTitle;
  public Text absLeaderboardTitle;
  public Text localLeadPos;
  public Text absLeadPos;
  public Text localLeadUsername;
  public Text absLeadUsername;
  public Text localLeadBounty;
  public Text absLeadBounty;

  ControllerScript controller;

  void Start() {
    controller = GameObject.Find("GameController").GetComponent<ControllerScript>();

    showLoadingScreen();
    // Populate all texts in the device language
    buildingsButton.text = Language.Field["BUILDINGS"];
    mapButton.text = Language.Field["MAP"];
    accountButton.text = Language.Field["ACCOUNT"];
    accountMenuTitle.text = Language.Field["ACCOUNT"];
    notRegisteredText.text = Language.Field["NOT_REGISTERED"];
    levelText.text = Language.Field["LEVEL"] + ":";
    bountyText.text = Language.Field["BOUNTY"] + ":";
    storageText.text = Language.Field["STORAGE"] + ":";
    passwordRecoveryButton.text = Language.Field["FORGOT_PASSWORD"];
    passwordRecoveryText.text = Language.Field["RECOVER_PASSWORD"];
    sendEmailButton.text = Language.Field["SEND"].ToUpper();
    notLoggedRegisterButton.text = Language.Field["REGISTER"].ToUpper();
    notLoggedLoginButton.text = Language.Field["LOGIN_BTN"].ToUpper();
    registrationRegisterButton.text = Language.Field["REGISTER"].ToUpper();
    loginFormLoginButton.text = Language.Field["LOGIN_BTN"].ToUpper();
    leaderboardTitle.text = Language.Field["LEADERBOARD"];
    localLeaderboardTitle.text = Language.Field["YOUR_POS"];
    absLeaderboardTitle.text = Language.Field["TOP_PLAYERS"];
    localLeadPos.text = Language.Field["SHORT_POSITION"];
    absLeadPos.text = Language.Field["SHORT_POSITION"];
    localLeadUsername.text = Language.Field["USERNAME"];
    absLeadUsername.text = Language.Field["USERNAME"];
    localLeadBounty.text = Language.Field["BOUNTY"];
    absLeadBounty.text = Language.Field["BOUNTY"];
    connectionError.GetComponentInChildren<Text>().text = "Oops!\n" + Language.Field["CONNECTION"];
    InvokeRepeating("updateAccountMenu", 0.0f, 5.0f);
    setUsername();
  }

  void Update() {
    // Update resources on UI
    int[] resources = controller.getUser().getResources();
    woodValue.text = resources[0].ToString();
    stoneValue.text = resources[1].ToString();
    goldValue.text = resources[2].ToString();
    pearlValue.text = controller.getUser().getPearl().ToString();
  }

  public void showPopupMessage(string message) {
    Text messageText = messagePopup.GetComponentInChildren(typeof(Text), true)as Text;
    messageText.text = message;
    messagePopup.SetActive(true);
  }

  public string formatNumber(int number) {
    string stringNumber = number.ToString();
    if (number < 1000)return stringNumber;
    if (number < 10000)return stringNumber[0] + (stringNumber[1] == '0' ? "" : "." + stringNumber[1]) + "K";
    if (number < 100000)return stringNumber[0].ToString() + stringNumber[1].ToString() + "K";
    if (number < 1000000)return stringNumber[0].ToString() + stringNumber[1].ToString() + stringNumber[2].ToString() + "K";
    return stringNumber;
  }

  public void playSuccessSound() {
    messagePopup.GetComponent<AudioSource>().Play();
  }

  public void showLoadingScreen() {
    loadingScreen.SetActive(true);
  }

  public void hideLoadingScreen() {
    accountMenuClose.SetActive(true);
    loadingScreen.SetActive(false);
  }

  public void hideAccountMenu() {
    accountMenu.SetActive(false);
  }

  public void showConnectionError(bool error) {
    connectionError.SetActive(error);
  }

  public void showButtons(bool show = true) {
    buttons.SetActive(show);
  }

  public void updateAccountMenu() {
    User user = controller.getUser();
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
    buttons.SetActive(true);
    passwordRecoveryText.text = Language.Field["RECOVER_PASSWORD"];
    showPopupMessage(message);
  }

  public void showNewGameOrLogin() {
    loadingSpinner.SetActive(false);
    loadingButtons.SetActive(true);
  }

  public void setUsername() {
    accountMenuUsername.text = API.GetUsername();
  }

  public void onLogin() {
    updateAccountMenu();
    hideAccountMenu();
    hud.SetActive(true);
    showButtons();
    setUsername();
    Invoke("hideLoadingScreen", 0.5f);
  }

  public void populateLeaderboard(List<PlayerLeaderboardEntry> leaderboard, string type) {
    float ENTRY_HEIGHT = 55.0f;
    RectTransform DEFAULT_RECT = leaderboardEntryPrefab.GetComponent<RectTransform>();
    // Go through leaderboard list
    for (int i = 0; i < leaderboard.Count; i++) {
      // For each entry create a new UI object
      GameObject entry = Instantiate(
        leaderboardEntryPrefab,
        type == "local" ? localLeaderboard.transform : absoluteLeaderboard.transform
      );
      // Set its top margin based on the position
      RectTransform transf = entry.GetComponent<RectTransform>();
      Vector3 position = transf.anchoredPosition;
      position.y = -(ENTRY_HEIGHT * i) - 45;
      transf.anchoredPosition = position;
      // Set all the info from the list entry

      foreach (Transform child in entry.transform) {
        child.GetComponent<Text>().text = (
          child.name == "Position" ?
          (leaderboard[i].Position + 1).ToString() : child.name == "Username" ?
          leaderboard[i].DisplayName : leaderboard[i].StatValue.ToString()
        );
      }
    }
    // TODO: Remember to add internationalisation to this menu
  }
}