using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class Bounty : MonoBehaviour {
  int BOUNTY_LEVEL_STEP=200;

  int previousBounty;
  int userLevel;

  public ControllerScript controller; 
  public UI ui; 
  public Slider bountyObject;

  void Start(){
    previousBounty = controller.getUser().getBounty();
    bountyObject.maxValue = BOUNTY_LEVEL_STEP;
    bountyObject.value = getProgressBarValue(previousBounty);
  }

  void Update(){
    // Update bounty progress on UI
    updateBountyUI(controller.getUser().getBounty());
  }

  void updateBountyUI(int value){
    if (value == previousBounty) return;
    if (value >= controller.getUser().getLevel()*BOUNTY_LEVEL_STEP) {
      controller.getUser().increaseLevel(getLevel(value));
      ui.showPopupMessage(Language.Field["BOUNTY_LEVELUP"] + " " + controller.getUser().getLevel() + "!");
      ui.playSuccessSound();
    }
    bountyObject.value = getProgressBarValue(value);
    bountyObject.transform.GetComponentInChildren<Text>().text = ui.formatNumber(value);
    previousBounty = value;
  }

  int getLevel(int bounty){
    return ((int) Math.Floor((decimal) bounty/BOUNTY_LEVEL_STEP))+1;
  }

  int getProgressBarValue(int bounty) {
    return bounty-(BOUNTY_LEVEL_STEP*(getLevel(bounty)-1));
  }
}