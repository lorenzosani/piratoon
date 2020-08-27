using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class Bounty : MonoBehaviour {
  int previousBounty = -1;
  int previousLevel = -1;

  int userLevel;
  int currentLevelEnd;
  int currentLevelLength;

  ControllerScript controller; 
  UI ui;

  public Slider bountyObject;

  void Start(){
    controller = GetComponent<ControllerScript>();
    ui = controller.getUI();
  }

  void Update(){
    int bounty = controller.getUser().getBounty();
    userLevel = controller.getUser().getLevel();
    currentLevelEnd = (userLevel+1)*userLevel*50;
    currentLevelLength = userLevel*100;
    // Update bounty progress on UI
    if (bounty != previousBounty) updateBountyUI(bounty);
  }

  void updateBountyUI(int value){
    if (value >= currentLevelEnd) {
      controller.getUser().increaseLevel(getNewLevel(value));
      if (previousLevel > 0) {
        ui.showPopupMessage(Language.Field["BOUNTY_LEVELUP"] + " " + userLevel + "!");
        ui.playSuccessSound();
      }
    }
    bountyObject.maxValue = currentLevelLength;
    bountyObject.value = currentLevelLength-(currentLevelEnd-value);
    bountyObject.transform.GetComponentInChildren<Text>().text = ui.formatNumber(value);
    previousBounty = value;
    previousLevel = controller.getUser().getLevel();
  }

  int getNewLevel(int bounty){
    int level = 0;
    int levelBounty = 0;
    while (bounty < levelBounty){
      level+=1;
      levelBounty = (level+1)*(level/2)*100;
    }
    return level;
  }
}