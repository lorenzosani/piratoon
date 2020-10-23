using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ShipyardMenu : MonoBehaviour {
  public Text shipyardMenuTitle;
  public GameObject shipyardMenuLoading;
  public GameObject shipyardMenuSlots;
  public Sprite[] shipSprites;

  ControllerScript controller;
  bool currentlyBuilding = false;

  void Start() {
    controller = GameObject.Find("GameController").GetComponent<ControllerScript>();
    shipyardMenuTitle.text = Language.Field["SHIPYARD"];
  }

  void Update() {
    if (gameObject.activeInHierarchy) {
      populateShipyardMenu();
    }
  }

  public void populateShipyardMenu() {
    Ship[] ships = controller.getUser().getVillage().getShips();
    string[] resourcesNames = new string[3] { "Wood", "Stone", "Gold" };
    int shipyardLevel = controller.getUser().getVillage().getBuildingInfo("Shipyard").getLevel();

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
      // If a ship is being built prevent the user from building others
      slot.Find("Description").Find("Button").gameObject.SetActive(currentlyBuilding);
      // Add correct ship icon and name
      if (i != 0 && i * 4 > shipyardLevel) {
        // Lock ships depending on the level of the shipyard
        slot.Find("Image").GetComponent<Image>().sprite = shipSprites[0];
        slot.Find("Description").Find("Title").GetComponent<Text>().text =
          Language.Field["UPGRADE_BUILDING"] + " " + (i * 4).ToString();
        slot.Find("Description").Find("Button").gameObject.SetActive(false);
      } else {
        slot.Find("Image").GetComponent<Image>().sprite = (ships[i] == null ?
          shipSprites[1] :
          shipSprites[ships[i].getLevel()]);
        slot.Find("Description").Find("Title").GetComponent<Text>().text = ships[i] == null ?
          Language.Field["NEW_SHIP"] :
          ships[i].getName();
        slot.Find("Description").Find("Button").Find("Text").GetComponent<Text>().text = ships[i] == null ?
          Language.Field["BUILD"] :
          Language.Field["UPGRADE"];
      }
      // Add correct price
      int[] cost = ships[i] == null ?
        new int[3] { 100 + 100 * (i * 4), 50 + 50 * (i * 4), 50 + 50 * (i * 4) } :
        ships[i].getCost(i);
      int[] resOwned = controller.getUser().getResources();
      for (int j = 0; j < 3; j++) {
        Text textObj = slot.Find("Description").Find("Cost").Find(resourcesNames[j]).gameObject.GetComponent<Text>();
        textObj.text = formatNumber(cost[j]);
        // Set price as red if user can't afford
        if (cost[0] > resOwned[0] || cost[1] > resOwned[1] || cost[2] > resOwned[2]) {
          textObj.color = new Color(1.0f, 0.66f, 0.66f, 1.0f);
        }
      }
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

  public GameObject setCurrentlyBuilding(bool building, int slot) {
    currentlyBuilding = building;
    return null;
    // TODO: Hide buttons of all slots

    // TODO: Show text of currently building 

    // TODO: Return that text GameObject

  }
}