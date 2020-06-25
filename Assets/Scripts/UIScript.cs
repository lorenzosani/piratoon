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

  void Update()
  {
    // Update resources on UI
    int[] resources = controller.getUser().getResources();
    woodValue.text = resources[0].ToString();
    stoneValue.text = resources[1].ToString();
    goldValue.text = resources[2].ToString();
    pearlValue.text = controller.getUser().getPearl().ToString();
    // Update bounty progress on UI
    bountyObject.value = controller.getUser().getBounty();
  }

  public void showPopupMessage(string message)
  {
    Text messageText = messagePopup.GetComponentInChildren(typeof(Text), true) as Text;
    messageText.text = message;
    messagePopup.SetActive(true);
  }
}