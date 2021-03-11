using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapTutorial : MonoBehaviour {
  public GameObject tutorialObject;
  public GameObject step1;
  public GameObject step2;
  public GameObject step3;
  public GameObject step4;
  public GameObject step5;

  public void showTutorial() {
    showStep1();
  }

  public void interruptTutorial() {
    tutorialObject.SetActive(false);
    setTutorialCompleted(true);
  }

  public void showStep1() {
    string step1Text =
      Language.Field["HERE_WE"] + "\n\n" +
      Language.Field["MAP_TUTORIAL"] + "\n\n" +
      Language.Field["CLOUDS"];
    Transform textObj = step1.transform.Find("Text");
    textObj.GetComponent<Text>().text = step1Text;
    Transform buttonObj = step1.transform.Find("Button");
    buttonObj.GetChild(0).GetComponent<Text>().text = Language.Field["CONTINUE"].ToUpper();
    tutorialObject.SetActive(true);
    setActiveStep(1);
  }

  public void showStep2() {
    string step2Text =
      Language.Field["CANT_SEE"] + "\n\n" +
      Language.Field["BUILD_WATCH"];
    Transform textObj = step2.transform.Find("Text");
    textObj.GetComponent<Text>().text = step2Text;
    Transform buttonObj = step2.transform.Find("Button");
    buttonObj.GetChild(0).GetComponent<Text>().text = Language.Field["CONTINUE"].ToUpper();
    setActiveStep(2);
  }

  public void showStep3() {
    string step3Text =
      Language.Field["EXPECTS_YOU"];
    Transform textObj = step3.transform.Find("Text");
    textObj.GetComponent<Text>().text = step3Text;
    Transform buttonObj = step3.transform.Find("Button");
    buttonObj.GetChild(0).GetComponent<Text>().text = Language.Field["CONTINUE"].ToUpper();
    setActiveStep(3);
  }

  public void showStep4() {
    Transform textObj = step4.transform.Find("Text");
    textObj.GetComponent<Text>().text = Language.Field["WILL_FIND"];
    // Set the text of Cities
    Transform citiesObj = step4.transform.Find("Cities");
    citiesObj.Find("Title").GetComponent<Text>().text = Language.Field["CITIES"];
    citiesObj.Find("Description").GetComponent<Text>().text = Language.Field["CITIES_DESC"];
    // Set the text of hideouts
    Transform hideoutsObj = step4.transform.Find("Hideouts");
    hideoutsObj.Find("Title").GetComponent<Text>().text = Language.Field["HIDEOUTS"];
    hideoutsObj.Find("Description").GetComponent<Text>().text = Language.Field["HIDEOUTS_DESC"];
    Transform buttonObj = step4.transform.Find("Button");
    buttonObj.GetChild(0).GetComponent<Text>().text = Language.Field["OKAY"].ToUpper();
    setActiveStep(4);
  }

  public void showStep5() {
    string step5Text =
      Language.Field["TAP_CITY"];
    Transform textObj = step5.transform.Find("Text");
    textObj.GetComponent<Text>().text = step5Text;
    Transform buttonObj = step5.transform.Find("Button");
    buttonObj.GetChild(0).GetComponent<Text>().text = Language.Field["LETS_GO"].ToUpper();
    setActiveStep(5);
  }

  void setActiveStep(int n) {
    step1.SetActive(n == 1);
    step2.SetActive(n == 2);
    step3.SetActive(n == 3);
    step4.SetActive(n == 4);
    step5.SetActive(n == 5);
  }

  public void setTutorialCompleted(bool completed) {
    GameObject.Find("GameController").GetComponent<Tutorial>().setTutorialCompleted(completed);
  }

  public bool getTutorialCompleted() {
    return GameObject.Find("GameController").GetComponent<Tutorial>().getTutorialCompleted();
  }
}