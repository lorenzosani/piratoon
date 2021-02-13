using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour {
  [Header("General")]
  public GameObject darkBackground;
  public GameObject buildingsMenu;

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
  public Text shipsButton;
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
  public GameObject leaderboardMenu;
  public GameObject localLeaderboard;
  public GameObject absoluteLeaderboard;
  public GameObject leaderboardEntryPrefab;
  public GameObject localLoading;
  public GameObject absoluteLoading;
  public Text leaderboardTitle;
  public Text localLeaderboardTitle;
  public Text absLeaderboardTitle;
  public Text localLeadPos;
  public Text absLeadPos;
  public Text localLeadUsername;
  public Text absLeadUsername;
  public Text localLeadBounty;
  public Text absLeadBounty;

  [Header("Ships")]
  public GameObject shipyardMenu;
  public GameObject shipInfo;
  public Sprite[] shipSprites;

  [Header("Map error dialog")]
  public GameObject mapErrorMessage;
  public Text mapErrorText;
  public GameObject tryAgainButton;
  public Text tryAgainText;
  public GameObject tryAgainLoading;

  [Header("Building info popup")]
  public GameObject buildingInfoPopup;
  public Buildings buildingsScript;

  ControllerScript controller;
  bool leaderboardLoaded = false;

  void Start() {
    controller = GameObject.Find("GameController").GetComponent<ControllerScript>();

    showLoadingScreen();
    // Populate all texts in the device language
    buildingsButton.text = Language.Field["BUILDINGS"];
    mapButton.text = Language.Field["MAP"];
    shipsButton.text = Language.Field["SHIPS"];
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
    mapErrorText.text = Language.Field["MAP_ERROR"];
    tryAgainText.text = Language.Field["TRY_AGAIN"];
    connectionError.GetComponentInChildren<Text>().text = "Oops!\n" + Language.Field["CONNECTION"];
    InvokeRepeating("updateAccountMenu", 0.0f, 5.0f);
    setUsername();
  }

  void Update() {
    // Update resources on UI
    int[] resources = controller.getUser().getResources();
    woodValue.text = formatNumber(resources[0]);
    stoneValue.text = formatNumber(resources[1]);
    goldValue.text = formatNumber(resources[2]);
    pearlValue.text = controller.getUser().getPearl().ToString();
    // Show darkerBackground if a menu is open
    darkBackground.SetActive(
      buildingsMenu.activeSelf ||
      leaderboardMenu.activeSelf ||
      shipyardMenu.activeSelf ||
      messagePopup.activeSelf ||
      mapErrorMessage.activeSelf ||
      shipInfo.activeSelf
    );
  }

  public void showPopupMessage(string message) {
    Text messageText = messagePopup.GetComponentInChildren(typeof(Text), true)as Text;
    messageText.text = message;
    messagePopup.SetActive(true);
  }

  public void playSuccessSound() {
    messagePopup.GetComponent<AudioSource>().Play();
  }

  public void showLoadingScreen() {
    loadingScreen.SetActive(true);
  }

  public void hideLoadingScreen() {
    Camera.main.GetComponent<PanAndZoom>().Zoom(15, 12);
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
    loadingButtons.SetActive(true);
    loadingSpinner.SetActive(false);
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
    hideLoadingScreen();
  }

  public void populateLeaderboard(List<PlayFab.ClientModels.PlayerLeaderboardEntry> leaderboard, string type) {
    localLoading.SetActive(false);
    float ENTRY_HEIGHT = (float)((localLeaderboard.GetComponent<RectTransform>().rect.height - 25) / 20);
    RectTransform DEFAULT_RECT = leaderboardEntryPrefab.GetComponent<RectTransform>();
    if (type == "local") {
      localLoading.SetActive(false);
    } else {
      absoluteLoading.SetActive(false);
    };
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
      position.y = -(ENTRY_HEIGHT * i) - ENTRY_HEIGHT + 10;
      transf.anchoredPosition = position;
      transf.sizeDelta = new Vector2(transf.sizeDelta.x, ENTRY_HEIGHT - 10);
      // Set all the info from the list entry
      foreach (Transform child in entry.transform) {
        child.GetComponent<Text>().text = (
          child.name == "Position" ?
          (leaderboard[i].Position + 1).ToString() : child.name == "Username" ?
          leaderboard[i].DisplayName : leaderboard[i].StatValue.ToString()
        );
      }
    }
    leaderboardLoaded = true;
  }

  public void showLeaderboardMenu() {
    if (controller.getUser().getUsername() != null) {
      if (!leaderboardLoaded)API.GetLeaderboard((leaderboard, type) => populateLeaderboard(leaderboard, type));
      leaderboardMenu.SetActive(true);
    } else {
      showPopupMessage("Please register and see your position in the leaderboard!");
    }
  }

  public void showShipyardMenu(bool active = true) {
    shipyardMenu.SetActive(active);
  }

  public void showMapError(bool active = true) {
    mapErrorMessage.SetActive(active);
  }

  public void showMapTryAgain(bool active = true) {
    tryAgainButton.SetActive(active);
    tryAgainLoading.SetActive(!active);
  }

  public void showShipInfo(int slot) {
    Ship ship = controller.getUser().getVillage().getShip(slot);
    // Set the correct image 
    Sprite shipSprite = ship.getLevel() <= 4 ? shipSprites[ship.getLevel() - 1] : shipSprites[3];
    shipInfo.transform.Find("Image").GetComponent<Image>().sprite = shipSprite;
    // Set the correct name
    shipInfo.transform.Find("ShipName").GetComponent<Text>().text = ship.getName();
    // Set the correct information about the ship
    Transform info = shipInfo.transform.Find("Info").transform;
    info.Find("Level").GetComponent<Text>().text = Language.Field["LEVEL"] + " " + ship.getLevel();
    info.Find("Condition").GetComponent<Text>().text = Language.Field["CONDITION"] + " " + ship.getCondition();
    // Show the dialog
    shipInfo.SetActive(true);
  }

  public string formatTime(int time) {
    if (time <= 60)return time + Language.Field["SECONDS_FIRST_LETTER"];
    if (time <= 3600) {
      int minutes = (int)Math.Floor((double)time / 60);
      int seconds = time % 60;
      return minutes + Language.Field["MINUTES_FIRST_LETTER"] + " " + seconds + Language.Field["SECONDS_FIRST_LETTER"];
    } else {
      int hours = (int)Math.Floor((double)time / 3600);
      int minutes = (time - hours * 3600) / 60;
      return hours + Language.Field["HOURS_FIRST_LETTER"] + " " + minutes + Language.Field["MINUTES_FIRST_LETTER"];
    }
  }

  public string formatNumber(int number) {
    string stringNumber = number.ToString();
    if (number < 1000)return stringNumber;
    if (number < 10000)return stringNumber[0] + (stringNumber[1] == '0' ? "" : "." + stringNumber[1]) + "K";
    if (number < 100000)return stringNumber[0].ToString() + stringNumber[1].ToString() + "K";
    if (number < 1000000)return stringNumber[0].ToString() + stringNumber[1].ToString() + stringNumber[2].ToString() + "K";
    return stringNumber;
  }

  public void showAttacksSuffered(List<AttackOutcome> attacks) {
    string message = "";
    for (int i = 0; i < attacks.Count; i++) {
      // In case of looting suffered
      AttackOutcome attack = attacks[i];
      string output = "";
      if (attack.getType() == 'p') {
        int[] resStolen = attack.getResources();
        string target = attack.getTarget() == "hideout" ?
          Language.Field["YOUR_VILLAGE"] :
          Language.Field["YOUR_CITY"] + "\"" + attack.getTarget() + "\"";
        output = string.Format(
          Language.Field["PLUNDERED"], attack.getAttacker(), target, resStolen[0], resStolen[1], resStolen[2]);
        // In case a city has been conquered
      } else {
        output = string.Format(Language.Field["CONQUERED"], attack.getAttacker(), attack.getTarget());
      }
      message = "\n" + message + output + "\n";
    }
    // Show the message popup
    showPopupMessage(message);
  }

  public void showBuildingInfo(string buildingName) {
    Building building = controller.getUser().getVillage().getBuildingInfo(buildingName);
    // Populate the title with the building name (internationalised)
    Transform titleObj = buildingInfoPopup.transform.Find("BuildingName");
    titleObj.GetComponent<Text>().text = Language.Field[buildingName.ToUpper()];
    // Populate the current image and next level image (if none, same)
    Transform imageCurrent = buildingInfoPopup.transform.Find("ImageNow");
    Transform imageNext = buildingInfoPopup.transform.Find("ImageAfter");
    imageCurrent.GetComponent<Image>().sprite = getBuildingImage(buildingName, building.getLevel());
    imageNext.GetComponent<Image>().sprite = getBuildingImage(buildingName, building.getLevel() + 1);
    // Populate level with building's level + next level
    Transform lvlCurrent = buildingInfoPopup.transform.Find("LevelNow");
    Transform lvlNext = buildingInfoPopup.transform.Find("LevelAfter");
    lvlCurrent.GetComponent<Text>().text = Language.Field["LEVEL_SHORT"] + " " + building.getLevel().ToString();
    lvlNext.GetComponent<Text>().text = Language.Field["LEVEL_SHORT"] + " " + (building.getLevel() + 1).ToString();
    // Populate building description
    Transform description = buildingInfoPopup.transform.Find("Description");
    description.GetComponent<Text>().text = Language.Field[buildingName.ToUpper() + "_DESC"];
    // Internationalisation of button text
    Transform upgradeBtn = buildingInfoPopup.transform.Find("Upgrade");
    upgradeBtn.GetChild(0).GetComponent<Text>().text = Language.Field["UPGRADE"].ToUpper();
    // Set button functionality
    Buildings spawner = GameObject.Find("GameController").GetComponent<Buildings>();
    Button btn = upgradeBtn.GetComponent<Button>();
    btn.onClick.RemoveAllListeners();
    btn.onClick.AddListener(() => spawner.main(buildingName));
    btn.onClick.AddListener(() => buildingInfoPopup.SetActive(false));
    // Set upgrade cost
    int[] upgradeCost = building.getCost();
    Transform resources = buildingInfoPopup.transform.Find("Resources");
    resources.Find("Wood").GetComponent<Text>().text = formatNumber(upgradeCost[0]);
    resources.Find("Stone").GetComponent<Text>().text = formatNumber(upgradeCost[1]);
    resources.Find("Gold").GetComponent<Text>().text = formatNumber(upgradeCost[2]);
    buildingInfoPopup.SetActive(true);
  }

  Sprite getBuildingImage(string name, int level) {
    return Resources.Load<Sprite>(
      "Images/Hideout/Buildings/" + name + "/" + (level > 3 ? 3 : level).ToString() + "_UI"
    );
  }

  public void showBuildingDescription(string buildingName) {
    showPopupMessage(Language.Field[buildingName.ToUpper() + "_DESC"]);
  }
}