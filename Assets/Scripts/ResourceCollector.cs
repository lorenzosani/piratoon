using UnityEngine;
using System;
using System.Collections.Generic;

public class ResourceCollector : MonoBehaviour{

  public ControllerScript controller;
  public string buildingName;

  Building building;
  int resourcePosition;
  bool tooltipIsShown;  

  void Awake(){
    tooltipIsShown = false;
    InvokeRepeating("checkLocalResources", 3.0f, 3.0f);
    gameObject.SetActive(false);
  }

  void checkLocalResources(){
    building = controller.getUser().getVillage().getBuildingInfo(buildingName);
    if (building!=null && building.getLocalStorage()>0 && !tooltipIsShown) {
      showTooltip();
    }
  }

  void showTooltip() {
    switch (buildingName){
      case "Woodcutter": 
        resourcePosition = 0;
        break;
      case "Stonecutter": 
        resourcePosition = 1;
        break;
      case "Inn": 
        resourcePosition = 2;
        break;
      default: Debug.Log(gameObject.name + ": This building does not produce resources");
        break;
    }
    transform.position = new Vector3(building.getPosition().x, building.getPosition().y+1, building.getPosition().z);
    gameObject.SetActive(true);
    tooltipIsShown = true;
  }

  public void collectResources(){
    controller.getUser().increaseResource(resourcePosition, building.getLocalStorage());
    building.resetLocalStorage();
    gameObject.SetActive(false);
    tooltipIsShown = false;
  }
}