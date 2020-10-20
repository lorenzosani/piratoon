using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ShipyardMenu : MonoBehaviour {
  public Text shipyardMenuTitle;
  public GameObject shipyardMenuLoading;
  public GameObject shipyardMenuSlots;
  ControllerScript controller;

  void Start() {
    controller = GameObject.Find("GameController").GetComponent<ControllerScript>();
    // Set language
    shipyardMenuTitle.text = Language.Field["SHIPYARD"];
  }

  void Update() {
    if (gameObject.activeInHierarchy) {
      populateShipyardMenu();
    }
  }

  public void populateShipyardMenu() {
    Ship[] ships = controller.getUser().getVillage().getShips();
    // Go through each slot in the menu
    float containerHeight = shipyardMenuSlots.GetComponent<RectTransform>().rect.height;
    float heightTaken = 0;
    int i = 0;
    foreach (Transform slot in shipyardMenuSlots.transform) {
      // Adjust slot position and height
      RectTransform rt = slot.GetComponent<RectTransform>();
      rt.sizeDelta = new Vector2(
        rt.sizeDelta.x,
        (containerHeight - 40) / ships.Length
      );
      rt.offsetMax = new Vector2(0, heightTaken);
      heightTaken = heightTaken - 20 - rt.sizeDelta.y;
      // Add correct ship name
      slot.Find("Description").Find("Title").GetComponent<Text>().text = ships[i] == null ?
        Language.Field["NEW_SHIP"] :
        ships[i].getName();
      // Add correct price

      // Set price as red if user can't afford

      // Add correct ship icon

      // Lock ships depending on the level of the shipyard

      // Add logic for building and upgrading ships

      i++;
    }
    shipyardMenuLoading.SetActive(false);
    shipyardMenuSlots.SetActive(true);
  }

  public string formatNumber(int number) {
    string stringNumber = number.ToString();
    if (number < 1000)return stringNumber;
    if (number < 10000)return stringNumber[0] + (stringNumber[1] == '0' ? "" : "." + stringNumber[1]) + "K";
    if (number < 100000)return stringNumber[0].ToString() + stringNumber[1].ToString() + "K";
    if (number < 1000000)return stringNumber[0].ToString() + stringNumber[1].ToString() + stringNumber[2].ToString() + "K";
    return stringNumber;
  }
}