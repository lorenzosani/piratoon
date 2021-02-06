using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour {
  public GameObject tutorialObject;
  public GameObject mainButtons;
  public GameObject step1;
  public GameObject step2;
  public GameObject step3;
  public GameObject step4;
  public GameObject step5;
  public GameObject step6;
  public GameObject step7;
  public GameObject step8;

  public void showTutorial() {
    showStep1();
  }

  public void hideTutorial() {
    tutorialObject.SetActive(false);
    mainButtons.SetActive(true);
  }

  public void showStep1() {
    string step1Text =
      Language.Field["AHOY"] + "\n\n\n" +
      Language.Field["FIRST_P"] + "\n\n\n" +
      Language.Field["BETA"] + "\n\n\n" +
      Language.Field["LOVE"];
    Transform textObj = step1.transform.Find("Text");
    textObj.GetComponent<Text>().text = step1Text;
    Transform buttonObj = step1.transform.Find("Button");
    buttonObj.GetChild(0).GetComponent<Text>().text = Language.Field["CONTINUE"].ToUpper();
    tutorialObject.SetActive(true);
    setActiveStep(1);
    mainButtons.SetActive(false);
  }

  public void showStep2() {
    string step2Text =
      Language.Field["CAP_ESC"] + "\n" +
      Language.Field["LANDED"] + "\n\n" +
      Language.Field["SPOT"];
    Transform textObj = step2.transform.Find("Text");
    textObj.GetComponent<Text>().text = step2Text;
    Transform buttonObj = step1.transform.Find("Button");
    buttonObj.GetChild(0).GetComponent<Text>().text = Language.Field["SURE"].ToUpper();
    setActiveStep(2);
  }

  public void showStep3() {
    string step3Text =
      Language.Field["START"] + "\n\n" +
      Language.Field["BUILD_HEAD"];
    Transform textObj = step3.transform.Find("Text");
    textObj.GetComponent<Text>().text = step3Text;
    setActiveStep(3);
    mainButtons.SetActive(true);
  }

  public void showStep4() {
    string step4Text = Language.Field["TAP_H"];
    Transform textObj = step4.transform.Find("Text");
    textObj.GetComponent<Text>().text = step4Text;
    setActiveStep(4);
    mainButtons.SetActive(false);
  }

  public void showStep5() {
    string step5Text =
      Language.Field["GREAT"] + "\n" +
      Language.Field["SPEED_UP"];
    Transform textObj = step5.transform.Find("Text");
    textObj.GetComponent<Text>().text = step5Text;
    setActiveStep(5);
  }

  public void showStep6() {
    string step6Text =
      Language.Field["BRILLIANT"] + "\n\n" +
      Language.Field["CALL_PIRATE"] + "\n\n" +
      Language.Field["BECOME"];
    Transform textObj = step6.transform.Find("Text");
    textObj.GetComponent<Text>().text = step6Text;
    setActiveStep(6);
  }

  public void showStep7() {
    string step7Text =
      Language.Field["FOLLOW_TIPS"] + "\n\n" +
      Language.Field["TIP_ONE"] + "\n\n" +
      Language.Field["TIP_TWO"] + "\n\n" +
      Language.Field["TIP_THREE"];
    Transform textObj = step7.transform.Find("Text");
    textObj.GetComponent<Text>().text = step7Text;
    setActiveStep(7);
  }

  public void showStep8() {
    string step8Text =
      Language.Field["TUTOREG"];
    Transform textObj = step8.transform.Find("Text");
    textObj.GetComponent<Text>().text = step8Text;
    mainButtons.SetActive(true);
    setActiveStep(8);
  }

  void setActiveStep(int n) {
    step1.SetActive(n == 1);
    step2.SetActive(n == 2);
    step3.SetActive(n == 3);
    step4.SetActive(n == 4);
    step5.SetActive(n == 5);
    step6.SetActive(n == 6);
    step7.SetActive(n == 7);
    step8.SetActive(n == 8);
  }
}