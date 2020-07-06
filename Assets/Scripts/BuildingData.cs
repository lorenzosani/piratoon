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

    foreach (Transform child in transform){
      Building building = village.getBuildingInfo(child.name);
      bool beenBuilt = true;
      if(building == null){
        // If building hasn't been built yet, get default info
        beenBuilt = false;
        building = createBuilding(child.name);
      }
      int level = !beenBuilt ? level = 1 : building.getLevel()+1;
      int[] cost = !beenBuilt ? building.getBaseCost() : building.getCost();
      GameObject levelObject = child.Find("Level").gameObject;
      Transform resorucesObject = child.Find("Resources");
      GameObject woodObject = resorucesObject.Find("Wood").gameObject;
      GameObject stoneObject = resorucesObject.Find("Stone").gameObject;
      GameObject goldObject = resorucesObject.Find("Gold").gameObject;

      // Set correct building level
      levelObject.GetComponent<Text>().text = "Lev."+level.ToString();
      // Set correct building cost
      woodObject.GetComponent<Text>().text = cost[0].ToString();
      stoneObject.GetComponent<Text>().text = cost[1].ToString();
      goldObject.GetComponent<Text>().text = cost[2].ToString();
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
}