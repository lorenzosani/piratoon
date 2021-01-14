using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapUI : MonoBehaviour {
  ControllerScript controller;
  DateTime cooldown;

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
  public GameObject cityAttackButton;
  public GameObject cityCooldown;
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

  public void showCityPopup(string name, int level, int[] production, int[] resources, DateTime cooldownEnd) {
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
    cityPopup.SetActive(true);
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