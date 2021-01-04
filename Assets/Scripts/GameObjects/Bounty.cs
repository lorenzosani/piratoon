using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Bounty : MonoBehaviour {
  int previousBounty = -1;
  int previousLevel = -1;

  int userLevel;
  int currentLevelEnd;
  int currentLevelLength;

  ControllerScript controller;
  UI ui;

  public Slider bountyObject;

  void Start() {
    controller = GameObject.Find("GameController").GetComponent<ControllerScript>();
    ui = controller.getUI();
    Invoke("CheckBounty", 1.0f);
  }

  //*****************************************************************
  // CHECK if the bounty has changed once every second and update the UI accordingly
  //*****************************************************************
  void CheckBounty() {
    int bounty = controller.getUser().getBounty();
    userLevel = controller.getUser().getLevel();
    // Check the user level is correct, given the bounty. If not, update it
    if (getNewLevel(bounty) != userLevel && bounty == previousBounty) {
      controller.getUser().increaseLevel(getNewLevel(bounty));
      updateBountyUI(bounty);
    }
    // If the bounty has changed, update bounty progress on UI
    if (bounty != previousBounty && bounty != 0)updateBountyUI(bounty);
    Invoke("CheckBounty", 1.0f);
  }

  void updateBountyUI(int value) {
    if (value >= currentLevelEnd) {
      userLevel = getNewLevel(value);
      currentLevelEnd = getLevelMax(userLevel);
      currentLevelLength = userLevel * 100;
      controller.getUser().increaseLevel(userLevel);
      if (previousLevel > 0 && SceneManager.GetActiveScene().buildIndex == 0) {
        ui.showPopupMessage(Language.Field["BOUNTY_LEVELUP"] + " " + userLevel + "!");
        ui.playSuccessSound();
      }
    }
    bountyObject.maxValue = currentLevelLength;
    bountyObject.value = currentLevelLength - (currentLevelEnd - value);
    bountyObject.transform.GetComponentInChildren<Text>().text = formatNumber(value);
    previousBounty = value;
    previousLevel = controller.getUser().getLevel();
  }

  int getNewLevel(int bounty) {
    int level = 1;
    float levelMaxBounty = getLevelMax(level);
    while (bounty > levelMaxBounty) {
      level += 1;
      levelMaxBounty = getLevelMax(level);
    }
    return level;
  }

  int getLevelMax(int level) {
    return (int)((level + 1.0f) * (level / 2.0f) * 100.0f);
  }

  string formatNumber(int number) {
    string stringNumber = number.ToString();
    if (number < 1000)return stringNumber;
    if (number < 10000)return stringNumber[0] + (stringNumber[1] == '0' ? "" : "." + stringNumber[1]) + "K";
    if (number < 100000)return stringNumber[0].ToString() + stringNumber[1].ToString() + "K";
    if (number < 1000000)return stringNumber[0].ToString() + stringNumber[1].ToString() + stringNumber[2].ToString() + "K";
    return stringNumber;
  }
}