using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour {
  public GameObject tutorialObject;
  public GameObject mainButtons;
  public GameObject step1;

  public void showTutorial() {
    showStep1();
  }

  void showStep1() {
    string step1Text =
      Language.Field["AHOY"] + "\n\n" +
      Language.Field["FIRST_P"] + "\n\n" +
      Language.Field["BETA"] + "\n\n" +
      Language.Field["LOVE"];
    Transform textObj = step1.transform.Find("Text");
    textObj.GetComponent<Text>().text = step1Text;
    tutorialObject.SetActive(true);
    step1.SetActive(true);
    mainButtons.SetActive(false);
  }
}