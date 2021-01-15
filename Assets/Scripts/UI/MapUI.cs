using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MapUI : MonoBehaviour {
  ControllerScript controller;
  DateTime cooldown;
  string formattedCooldown = "";

  [Header("Resources and Bounty")]
  public GameObject hud;
  public Text woodValue;
  public Text stoneValue;
  public Text goldValue;
  public Text pearlValue;

  [Header("Loading and errors")]
  public GameObject loadingScreen;

  [Header("Hideout popup")]
  public GameObject hideoutPopup;
  public GameObject hideoutPopupLoading;
  public GameObject hideoutPopupInfo;
  public Text hideoutUsername;
  public Text hideoutLevel;
  public Text hideoutWood;
  public Text hideoutStone;
  public Text hideoutGold;
  public Text hideoutStrength;
  public Text hideoutLevelTitle;
  public Text hideoutStrengthTitle;
  public Text attackButton;

  [Header("Player's own Hideout popup")]
  public GameObject ownHideoutPopup;
  public Text ownHideoutTitle;
  public Text navigateButton;
  public Text showButton;

  [Header("City popup")]
  public GameObject cityPopup;
  public GameObject cityPopupInfo;
  public GameObject cityPopupLoading;
  public GameObject cityAttackButton;
  public GameObject cityCooldown;
  public GameObject cityOwnerObject;
  public Text ownedBy;
  public Text cooldownTitle;
  public Text cooldownTimer;
  public Text cityName;
  public Text cityLevel;
  public Text cityWoodProduction;
  public Text cityStoneProduction;
  public Text cityGoldProduction;
  public Text cityWood;
  public Text cityStone;
  public Text cityGold;
  public Text cityLevelTitle;
  public Text cityProductionTitle;
  public Text cityResourcesTitle;

  [Header("Conquered City popup")]
  public GameObject conqCityPopup;
  public Text conqCityUpgradeButtonText;
  public Text youConqueredText;
  public Text conqCooldownText;
  public Text conqCityName;
  public Text conqCityLevel;
  public Text conqCityWoodProduction;
  public Text conqCityStoneProduction;
  public Text conqCityGoldProduction;
  public Text conqCityWood;
  public Text conqCityStone;
  public Text conqCityGold;
  public Text conqCityWoodUpgradeCost;
  public Text conqCityStoneUpgradeCost;
  public Text conqCityGoldUpgradeCost;
  public Text conqCityLevelTitle;
  public Text conqCityProductionTitle;
  public Text conqCityResourcesTitle;
  public Text conqCityUpgradeTitle;

  [Header("Message Popup")]
  public GameObject messagePopup;

  [Header("Ship Picker Menu")]
  public GameObject shipPickerDialog;
  public Text shipPickerTitle;
  public Sprite[] shipSprites;

  [Header("Attack Options Popup")]
  public GameObject attackOptionsPopup;
  public Text plunderButton;
  public Text conquerButton;

  void Start() {
    controller = GameObject.Find("GameController").GetComponent<ControllerScript>();
    hideoutLevelTitle.text = Language.Field["USER_LVL"];
    hideoutStrengthTitle.text = Language.Field["STRENGTH"];
    cityLevelTitle.text = Language.Field["CITY_LVL"];
    cityProductionTitle.text = Language.Field["PROD"];
    cityResourcesTitle.text = Language.Field["RES"];
    conqCityLevelTitle.text = Language.Field["CITY_LVL"];
    conqCityProductionTitle.text = Language.Field["PROD"];
    conqCityResourcesTitle.text = Language.Field["RES"];
    conqCityUpgradeTitle.text = Language.Field["UPGRADE_COST"];
    youConqueredText.text = Language.Field["YOU_CONQ"];
    conqCityUpgradeButtonText.text = Language.Field["UPGRADE"].ToUpper();
    attackButton.text = Language.Field["ATTACK_VERB"].ToUpper();
    ownHideoutTitle.text = Language.Field["YOUR_H"];
    navigateButton.text = Language.Field["NAVIGATE"].ToUpper();
    showButton.text = Language.Field["SHOW"].ToUpper();
    shipPickerTitle.text = Language.Field["SHIP_PICK"];
  }

  void Update() {
    // Update resources on UI
    int[] resources = controller.getUser().getResources();
    woodValue.text = formatNumber(resources[0]);
    stoneValue.text = formatNumber(resources[1]);
    goldValue.text = formatNumber(resources[2]);
    pearlValue.text = controller.getUser().getPearl().ToString();
    // Set city conquests cooldown timer
    if (cooldown != null && DateTime.Compare(cooldown, DateTime.UtcNow) > 0) {
      int timeLeft = (int)(cooldown - System.DateTime.UtcNow).TotalSeconds;
      if (timeLeft <= 0) {
        cityCooldown.SetActive(false);
        cityAttackButton.SetActive(true);
      }
      cooldownTimer.text = formatTime(timeLeft);
      conqCooldownText.text = Language.Field["UNATTACKABLE"] + " " + formatTime(timeLeft);
    }
  }

  public void showLoadingScreen(bool show = true) {
    loadingScreen.SetActive(show);
  }

  public void showHideoutPopup(bool show = true) {
    if (show) {
      hideoutPopup.SetActive(true);
    } else {
      hideoutPopupInfo.SetActive(false);
      hideoutPopupLoading.SetActive(true);
      hideoutPopup.SetActive(false);
    }
  }

  public void populateHideoutPopup(string username, int level, int[] resources, int strength = 0) {
    // Assign the correct values to the different UI bits
    hideoutUsername.text = username;
    hideoutLevel.text = level.ToString();
    hideoutWood.text = formatNumber(resources[0]);
    hideoutStone.text = formatNumber(resources[1]);
    hideoutGold.text = formatNumber(resources[2]);
    hideoutStrength.text = strength == 0 ? "" : strength.ToString();
    // Show all the information
    hideoutPopupLoading.SetActive(false);
    hideoutPopupInfo.SetActive(true);
  }

  public void showCityPopup(string name, int level, int[] production, int[] resources, DateTime cooldownEnd, string owner) {
    // Assign the correct values to the different UI bits
    cityName.text = name;
    cityLevel.text = level.ToString();
    cityWoodProduction.text = formatNumber(production[0]);
    cityStoneProduction.text = formatNumber(production[1]);
    cityGoldProduction.text = formatNumber(production[2]);
    cityWood.text = formatNumber(resources[0]);
    cityStone.text = formatNumber(resources[1]);
    cityGold.text = formatNumber(resources[2]);
    // Check if the city can be attacked or has just been conquered
    if (cooldownEnd != null && DateTime.Compare(cooldownEnd, DateTime.UtcNow) > 0) {
      cooldownTitle.text = Language.Field["COOLDOWN"];
      cooldown = cooldownEnd;
      cityAttackButton.SetActive(false);
      cityCooldown.SetActive(true);
    } else {
      cityCooldown.SetActive(false);
      cityAttackButton.SetActive(true);
    }
    // Show all the information
    cityPopupLoading.SetActive(true);
    cityPopupInfo.SetActive(false);
    cityPopup.SetActive(true);
    // Check if the city is owned by some player
    if (owner != "") {
      string ownerUsername = "";
      API.GetUserData(new List<string>() { "User" }, (result) => {
        if (result != null && result.Data.ContainsKey("User") && result.Data["User"].Value != "{}") {
          // If yes, de-serialize and set the data objects
          User user = JsonConvert.DeserializeObject<User>((string)result.Data["User"].Value);
          ownerUsername = user.getUsername();
          ownedBy.text = Language.Field["OWNED_BY"] + " " + ownerUsername;
          cityOwnerObject.SetActive(true);
          cityPopupLoading.SetActive(false);
          cityPopupInfo.SetActive(true);
        }
      }, owner);
    } else {
      cityOwnerObject.SetActive(false);
      cityPopupLoading.SetActive(false);
      cityPopupInfo.SetActive(true);
    }
  }

  public void showConqueredCityPopup(string name, int level, int[] production, int[] resources, int[] upgradeCost, DateTime cooldownEnd) {
    // Populate ui with the correct text
    cooldown = cooldownEnd;
    conqCityName.text = name;
    conqCityLevel.text = level.ToString();
    conqCityWoodProduction.text = formatNumber(production[0]);
    conqCityStoneProduction.text = formatNumber(production[1]);
    conqCityGoldProduction.text = formatNumber(production[2]);
    conqCityWood.text = formatNumber(resources[0]);
    conqCityStone.text = formatNumber(resources[1]);
    conqCityGold.text = formatNumber(resources[2]);
    conqCityWoodUpgradeCost.text = formatNumber(upgradeCost[0]);
    conqCityStoneUpgradeCost.text = formatNumber(upgradeCost[1]);
    conqCityGoldUpgradeCost.text = formatNumber(upgradeCost[2]);
    conqCityPopup.SetActive(true);
  }

  public string formatNumber(int number) {
    string stringNumber = number.ToString();
    if (number < 1000)return stringNumber;
    if (number < 10000)return stringNumber[0] + (stringNumber[1] == '0' ? "" : "." + stringNumber[1]) + "K";
    if (number < 100000)return stringNumber[0].ToString() + stringNumber[1].ToString() + "K";
    if (number < 1000000)return stringNumber[0].ToString() + stringNumber[1].ToString() + stringNumber[2].ToString() + "K";
    return stringNumber;
  }

  public void showShipPicker(Ship[] ships, bool show = true) {
    if (show) {
      // Populate with the correct ships
      Transform slots = shipPickerDialog.transform.Find("Slots");
      float containerHeight = slots.GetComponent<RectTransform>().rect.height;
      float heightTaken = 0;
      int i = 0;
      foreach (Transform slot in slots.transform) {
        // Adjust slot position and height
        RectTransform rt = slot.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, (containerHeight - 40) / 3);
        rt.offsetMax = new Vector2(0, heightTaken);
        heightTaken = heightTaken - 20 - rt.sizeDelta.y;
        // Add correct ship icon and name
        slot.Find("Image").GetComponent<Image>().sprite = (ships[i] == null ?
          shipSprites[0] :
          shipSprites[ships[i].getLevel() < 5 ? ships[i].getLevel() : 4]);
        slot.Find("Description").Find("Title").GetComponent<Text>().text = ships[i] == null ?
          " " : ships[i].getName();
        // Hide attack button on empty slots
        if (ships[i] == null) {
          slot.Find("Description").Find("Button").gameObject.SetActive(false);
        } else {
          Transform btnText = slot.Find("Description").Find("Button").Find("Text");
          btnText.GetComponent<Text>().text = Language.Field["ATTACK_VERB"];
        }
        i++;
      }
    }
    shipPickerDialog.SetActive(show);
  }

  public void showPopupMessage(string message) {
    Text messageText = messagePopup.GetComponentInChildren(typeof(Text), true)as Text;
    messageText.text = message;
    messagePopup.SetActive(true);
  }

  public void showOwnHideoutPopup(bool show = true) {
    ownHideoutPopup.SetActive(show);
  }

  public void showAttackOptions(string cityName) {
    string message = Language.Field["CITY_REACHED"] + " " + cityName + "." + "\n";
    string question = Language.Field["CHOICE"];
    attackOptionsPopup.transform.Find("Text").GetComponent<Text>().text = message + question;
    plunderButton.text = Language.Field["PLUNDER"].ToUpper();
    conquerButton.text = Language.Field["CONQUER"].ToUpper();
    attackOptionsPopup.SetActive(true);
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
}