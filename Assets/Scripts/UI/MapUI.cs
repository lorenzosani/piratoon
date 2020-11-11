using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapUI : MonoBehaviour {

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

  [Header("Ship Picker Menu")]
  public GameObject shipPickerDialog;
  public Sprite[] shipSprites;

  void Start() {
    hideoutLevelTitle.text = Language.Field["USER_LVL"];
    hideoutStrengthTitle.text = Language.Field["STRENGTH"];
    attackButton.text = Language.Field["ATTACK_VERB"];
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

  public void populateHideoutPopup(string username, int level, int[] resources, int strength) {
    // Assign the correct values to the differnet UI bits
    hideoutUsername.text = username;
    hideoutLevel.text = level.ToString();
    hideoutWood.text = formatNumber(resources[0]);
    hideoutStone.text = formatNumber(resources[1]);
    hideoutGold.text = formatNumber(resources[2]);
    hideoutStrength.text = strength.ToString();
    // Show all the information
    hideoutPopupLoading.SetActive(false);
    hideoutPopupInfo.SetActive(true);
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
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, (containerHeight - 40) / ships.Length);
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
        }
        i++;
      }
    }
    // TODO: Add internationalisation to ship picker
    shipPickerDialog.SetActive(show);
  }
}