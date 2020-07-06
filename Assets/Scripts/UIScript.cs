using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class UIScript : MonoBehaviour
{
  public ControllerScript controller;
  public Text woodValue;
  public Text stoneValue;
  public Text goldValue;
  public Text pearlValue;
  public Slider bountyObject;
  public GameObject messagePopup;

  int previousBounty;

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
    if (value >= (int) bountyObject.maxValue){
      controller.getUser().increaseLevel();
      bountyObject.maxValue = bountyObject.maxValue*3;
      bountyObject.value = value;
      showPopupMessage("Woohooa! You have reached bounty level " + controller.getUser().getLevel() + "!");
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
}