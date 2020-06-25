using UnityEngine;
using UnityEngine.UI;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;

public class BuildingSpawner : MonoBehaviour
{
  ControllerScript controller;
  public UIScript ui;

  void Start()
  {
    controller = GetComponent<ControllerScript>();
  }

  //*****************************************************************
  // PUBLIC: Call this with the name of the building you want to build
  //*****************************************************************
  public void main(string buildingName)
  {
    // Check if the building has already been built
    foreach (Building b in controller.getUser().getVillage().getBuildings())
    {
      if (b.getName() == buildingName)
      {
        ui.showPopupMessage("Oops, you can't build this twice!");
        return;
      }
    }
    // Create a new building object
    Building building = createBuilding(buildingName);
    // Check if user can afford the building, if yes pay
    if (!canAfford(building))
    {
      ui.showPopupMessage("Oops, it looks like you don't have enough resources to build this.");
      return;
    }
    // Register the new building with the controller
    controller.getUser().getVillage().addBuilding(building);
    // Add building's value to user's bounty
    controller.getUser().addBounty(building.getValue());
    // Spawn building
    spawn(building);
  }

  //*****************************************************************
  // Helper methods
  //*****************************************************************

  //This creates a new Building object for your building
  Building createBuilding(string buildingName)
  {
    GameObject prefab = getPrefab(buildingName);
    return new Building(buildingName, prefab.transform.position, prefab);
  }

  //This fetches the prefab of the building to be shown
  GameObject getPrefab(string buildingName)
  {
    return (GameObject)Resources.Load("Prefabs/" + new CultureInfo("en-US", false).TextInfo.ToTitleCase(buildingName), typeof(GameObject));
  }

  //This instantiates the building on the scene
  void spawn(Building b)
  {
    if (b != null)
    {
      GameObject building = (GameObject)Instantiate(b.getPrefab(), b.getPosition(), Quaternion.identity);
    }
  }

  //This checks wether the suer can afford to buy a building; if yes, pay the price
  bool canAfford(Building b)
  {
    int[] resources = controller.getUser().getResources();
    int[] cost = b.getCost();
    int[] remainingResources = new int[3];
    for (int i = 0; i < resources.Length; i++)
    {
      remainingResources[i] = resources[i] - cost[i];
      if (remainingResources[i] < 0) return false;
    }
    controller.getUser().setResources(remainingResources);
    return true;
  }
}
