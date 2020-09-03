using System;
using System.Collections.Generic;
using UnityEngine;

public class UserResources : MonoBehaviour {
  ControllerScript controller;
  string[] productionBuildings = new string[3] { "Woodcutter", "Stonecutter", "Inn" };
  string[] tooltipNames = new string[3] { "TooltipWood", "TooltipStone", "TooltipGold" };
  bool[] shownTooltips = new bool[3] { false, false, false };

  void Awake() {
    controller = GetComponent<ControllerScript>();
    InvokeRepeating("checkLocalResources", 0.0f, 5.0f);
  }

  void checkLocalResources() {
    for (int i = 0; i < productionBuildings.Length; i++) {
      Building building = controller.getUser().getVillage().getBuildingInfo(productionBuildings[i]);
      if (building != null && building.getLocalStorage() > 0 && !shownTooltips[i]) {
        showTooltip(productionBuildings[i]);
      }
    }
  }

  public void showTooltip(string buildingName) {
    int resourceCode = -1;
    switch (buildingName) {
      case "Woodcutter":
        resourceCode = 0;
        break;
      case "Stonecutter":
        resourceCode = 1;
        break;
      case "Inn":
        resourceCode = 2;
        break;
      default:
        Debug.Log(gameObject.name + ": This building does not produce resources");
        break;
    }
    if (resourceCode < 0)return;
    Building building = controller.getUser().getVillage().getBuildingInfo(buildingName);
    GameObject tooltip = GameObject.Find(tooltipNames[resourceCode]);
    tooltip.transform.position = new Vector3(building.getPosition().x, resourceCode == 1 ? building.getPosition().y + 2 : building.getPosition().y + 1, building.getPosition().z);
    tooltip.SetActive(true);
    shownTooltips[resourceCode] = true;
  }

  public void collectResources(int resourceCode) {
    Building building = controller.getUser().getVillage().getBuildingInfo(productionBuildings[resourceCode]);
    GameObject tooltip = GameObject.Find(tooltipNames[resourceCode]);
    int storageSpaceLeft = controller.getUser().getStorageSpaceLeft()[resourceCode];
    int newResources = building.getLocalStorage();
    if (storageSpaceLeft == 0) {
      controller.getUI().showPopupMessage(Language.Field["STORAGE_SPACE"]);
      return;
    }
    controller.getUser().increaseResource(resourceCode, newResources);
    building.resetLocalStorage();
    tooltip.SetActive(false);
    shownTooltips[resourceCode] = false;
  }
}