using UnityEngine;
using UnityEngine.UI;
using System;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;

public class BuildingSpawner : MonoBehaviour
{
  ControllerScript controller;
  Building currentlyBuilding = null;
  Slider loadingSlider;
  Text loadingText;

  public UIScript ui;
  public GameObject loadingBar;

  //*****************************************************************
  // START and UPDATE methods
  //*****************************************************************

  void Start()
  {
    controller = GetComponent<ControllerScript>();
    loadingSlider = loadingBar.GetComponent<Slider>();
    loadingText = loadingBar.GetComponentInChildren(typeof(Text), true) as Text;
  }

  void Update()
  {
    if (currentlyBuilding != null)
    {
      int totalTime = currentlyBuilding.getValue();
      int timeLeft = (int)(currentlyBuilding.getCompletionTime() - System.DateTime.UtcNow).TotalSeconds;
      if (timeLeft == 0)
      {
        loadingBar.SetActive(false);
        spawn(currentlyBuilding);
      }
      loadingSlider.value = (int)100 - (timeLeft * 100 / totalTime);
      loadingText.text = timeLeft > 60 ? Math.Floor((double)timeLeft / 60) + "m " + timeLeft % 60 + "s" : timeLeft + "s";
    }
  }

  //*****************************************************************
  // PUBLIC: Call this with the name of the building you want to build
  //*****************************************************************
  public void main(string buildingName)
  {
    // Check if something is already been built
    if (currentlyBuilding != null)
    {
      ui.showPopupMessage("There's already a building in construction! You can build only one at a time");
      return;
    }
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
    startConstruction(building);
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

  //This starts the construction of a building
  void startConstruction(Building b)
  {
    currentlyBuilding = b;
    loadingBar.transform.position = b.getPosition(); ;
    loadingBar.SetActive(true);
  }

  //This instantiates the building on the scene
  void spawn(Building b)
  {
    GameObject building = (GameObject)Instantiate(b.getPrefab(), b.getPosition(), Quaternion.identity);
    currentlyBuilding = null;
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
