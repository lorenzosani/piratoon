using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BuildingsMenu : MonoBehaviour {
  public Text buildingsMenuTitle;
  ControllerScript controller;

  void Start() {
    GameObject gameController = GameObject.Find("GameController");
    buildingsMenuTitle.text = Language.Field["BUILDINGS"];
    controller = gameController.GetComponent<ControllerScript>();
    InvokeRepeating("populateBuildingsMenu", 0.0f, 2.0f);
  }

  public void populateBuildingsMenu() {
    Village village = controller.getUser().getVillage();
    // Go through each building in the menu
    foreach (Transform child in transform) {
      string buildingName = child.name.Substring(0, child.name.Length - 4);
      // Populate building name in the correct language
      Text title = child.Find("Title").GetComponent<Text>();
      title.text = Language.Field[buildingName.ToUpper()];
      // Inventor and Watchtower are unlocked only after headquarters are upgraded
      if (buildingName == "Inventor" || buildingName == "Watchtower") {
        Building headquarter = village.getBuildingInfo("Headquarter");
        // Lock buildings
        // TODO: Once inventor is implemented allow unlocking at level 5
        if ((headquarter == null || headquarter.getLevel() < 30) && buildingName == "Inventor") {
          lockBuilding(child);
          continue;
        }
        if ((headquarter == null || headquarter.getLevel() < 2) && buildingName == "Watchtower") {
          lockBuilding(child, 2);
          continue;
        }
        // Unlock buildings
        if (headquarter != null && headquarter.getLevel() == 2 && buildingName == "Watchtower")unlockBuilding(child);
      }
      // All other buildings are unlocked from the start
      Building building = village.getBuildingInfo(buildingName);
      bool beenBuilt = true;
      if (building == null) {
        // If building hasn't been built yet, get default info
        beenBuilt = false;
        building = createBuilding(buildingName);
      }
      int level = !beenBuilt ? level = 1 : building.getLevel() + 1;
      int[] cost = !beenBuilt ? building.getBaseCost() : building.getCost();
      // Show info for the current building
      populateBuilding(child, level, cost, building.getName());
    }
  }

  // Factory method that returns the correct building object
  Building createBuilding(string name) {
    switch (name) {
      case "Inventor":
        return new Inventor();
      case "Woodcutter":
        return new Woodcutter();
      case "Stonecutter":
        return new Stonecutter();
      case "Watchtower":
        return new Watchtower();
      case "Headquarter":
        return new Headquarter();
      case "Defence":
        return new Defence();
      case "Shipyard":
        return new Shipyard();
      case "Storage":
        return new Storage();
      case "Inn":
        return new Inn();
      default:
        return null;
    }
  }

  void populateBuilding(Transform child, int level, int[] cost, string buildingName) {
    string[] resourcesNames = new string[3] { "Wood", "Stone", "Gold" };
    int[] resOwned = controller.getUser().getResources();
    bool canAfford = true;

    // Get the UI objects
    GameObject levelObject = child.Find("Level").gameObject;
    Transform resorucesObject = child.Find("Resources");
    GameObject levelUpObject = child.Find("LevelUp").gameObject;
    // Set building level
    levelObject.GetComponent<Text>().text = Language.Field["LEVEL_SHORT"] + " " + level.ToString();
    // Check if the user can afford the building
    if (cost[0] > resOwned[0] || cost[1] > resOwned[1] || cost[2] > resOwned[2])canAfford = false;
    // Set building cost
    for (int i = 0; i < 3; i++) {
      Text textObj = resorucesObject.Find(resourcesNames[i]).gameObject.GetComponent<Text>();
      textObj.text = controller.getUI().formatNumber(cost[i]);
      if (!canAfford) {
        textObj.color = new Color(1.0f, 0.66f, 0.66f, 1.0f);
      } else {
        textObj.color = new Color(1.0f, 1.00f, 1.00f, 1.0f);
      }
    }
    // Show level up icon
    if (level > 1)levelUpObject.SetActive(true);
    // Set the correct image based on level
    child.Find("Image").GetComponent<Image>().sprite = getBuildingImage(buildingName, level);
  }

  Sprite getBuildingImage(string name, int level) {
    Sprite image = null;
    level = level <= 0 ? 1 : level;
    while (image == null) {
      image = Resources.Load<Sprite>(
        "Images/Hideout/Buildings/" + name + "/" + level.ToString() + "_UI"
      );
      level = level - 1;
    }
    return image;
  }

  void lockBuilding(Transform building, int level = -1) {
    // Get child objects
    Button btn = building.GetComponent<Button>();
    Color titleColor = building.Find("Title").GetComponent<Text>().color;
    Text levelText = building.Find("Level").GetComponent<Text>();
    GameObject resources = building.Find("Resources").gameObject;
    // Set objects to blocked
    btn.interactable = false;
    titleColor.a = 0.66f;
    building.Find("Title").GetComponent<Text>().color = titleColor;
    // If level is -1, just put 'coming soon'
    levelText.text = level == -1 ? Language.Field["SOON"] : Language.Field["LEVEL_UNLOCK"] + " " + level;
    resources.SetActive(false);
  }

  void unlockBuilding(Transform building) {
    // Get child objects
    Button btn = building.GetComponent<Button>();
    Color titleColor = building.Find("Title").GetComponent<Text>().color;
    Text level = building.Find("Level").GetComponent<Text>();
    GameObject resources = building.Find("Resources").gameObject;
    // Set objects to blocked
    btn.interactable = true;
    titleColor.a = 1f;
    building.Find("Title").GetComponent<Text>().color = titleColor;
    resources.SetActive(true);
  }
}