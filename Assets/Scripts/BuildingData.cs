using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class BuildingData : MonoBehaviour
{
  public ControllerScript controller;

  void Start(){
    InvokeRepeating("populateBuildingsMenu", 0.0f, 3.0f);
  }

  public void populateBuildingsMenu() {
    Village village = controller.getUser().getVillage();
    // Go through each building in the menu
    foreach (Transform child in transform){
      // Inventor and Watchtower are unlocked only after headquarters are upgraded
      if (child.name == "Inventor" || child.name == "Watchtower"){
        Building headquarter = village.getBuildingInfo("Headquarter");
        // Lock buildings
        if((headquarter == null || headquarter.getLevel()<5) && child.name == "Inventor") {
          lockBuilding(child, 5);
          continue;
        }
        if((headquarter == null || headquarter.getLevel()<3) && child.name == "Watchtower"){
          lockBuilding(child, 3);
          continue;
        } 
        // Unlock buildings
        if(headquarter != null && headquarter.getLevel()==5 && child.name == "Inventor") unlockBuilding(child);
        if(headquarter != null && headquarter.getLevel()==3 && child.name == "Watchtower") unlockBuilding(child);
      }
      // All other buildings are unlocked from the start
      Building building = village.getBuildingInfo(child.name);
      bool beenBuilt = true;
      if(building == null){
        // If building hasn't been built yet, get default info
        beenBuilt = false;
        building = createBuilding(child.name);
      }
      int level = !beenBuilt ? level = 1 : building.getLevel()+1;
      int[] cost = !beenBuilt ? building.getBaseCost() : building.getCost();
      // Show info for the current building
      populateBuilding(child, level, cost);
    }
  }

  // Factory method that returns the correct building object
  Building createBuilding(string name){
    switch(name){
      case "Inventor": return new Inventor();
      case "Woodcutter": return new Woodcutter();
      case "Stonecutter": return new Stonecutter();
      case "Watchtower": return new Watchtower();
      case "Headquarter": return new Headquarter();
      case "Defence": return new Defence();
      case "Shipyard": return new Shipyard();
      case "Storage": return new Storage();
      case "Inn": return new Inn();
      default: return null;
    }
  }

  void populateBuilding(Transform child, int level, int[] cost){
    // Get the UI objects
    GameObject levelObject = child.Find("Level").gameObject;
    Transform resorucesObject = child.Find("Resources");
    GameObject woodObject = resorucesObject.Find("Wood").gameObject;
    GameObject stoneObject = resorucesObject.Find("Stone").gameObject;
    GameObject goldObject = resorucesObject.Find("Gold").gameObject;
    // Set building level
    levelObject.GetComponent<Text>().text = "Lev."+level.ToString();
    // Set building cost
    woodObject.GetComponent<Text>().text = cost[0].ToString();
    stoneObject.GetComponent<Text>().text = cost[1].ToString();
    goldObject.GetComponent<Text>().text = cost[2].ToString();
  }

  void lockBuilding(Transform building, int level){
    // Get child objects
    Button btn = building.GetComponent<Button>();
    Color titleColor = building.Find("Text").GetComponent<Text>().color;
    Text levelText = building.Find("Level").GetComponent<Text>();
    GameObject resources = building.Find("Resources").gameObject;
    // Set objects to blocked
    btn.interactable = false;
    titleColor.a = 0.66f;
    building.Find("Text").GetComponent<Text>().color = titleColor;
    levelText.text = "Unlock at level " + level;
    resources.SetActive(false);
  }

  void unlockBuilding(Transform building){
    // Get child objects
    Button btn = building.GetComponent<Button>();
    Color titleColor = building.Find("Text").GetComponent<Text>().color;
    Text level = building.Find("Level").GetComponent<Text>();
    GameObject resources = building.Find("Resources").gameObject;
    // Set objects to blocked
    btn.interactable = true;
    titleColor.a = 1f;
    building.Find("Text").GetComponent<Text>().color = titleColor;
    resources.SetActive(true);
  }
}