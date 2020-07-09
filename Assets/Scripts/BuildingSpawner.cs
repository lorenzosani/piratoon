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
  AudioSource audioSource;
  Building building;
  bool newBuilding;

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
    audioSource = GetComponent<AudioSource>();
    newBuilding = true;
  }

  void Update()
  {
    if (currentlyBuilding != null)
    {
      int totalTime = currentlyBuilding.getFutureValue()/4;
      int timeLeft = (int)(currentlyBuilding.getCompletionTime() - System.DateTime.UtcNow).TotalSeconds;
      if (timeLeft == 0)
      {
        loadingBar.SetActive(false);
        currentlyBuilding.increaseLevel();
        // Add building's value to user's bounty
        controller.getUser().addBounty(currentlyBuilding.getValue());
        // Spawn the building on the scene
        if (newBuilding) spawn(currentlyBuilding);
        // Reset global variables
        currentlyBuilding = null;
        building = null;
        newBuilding = true;
      }
      loadingSlider.value = (int)100 - (timeLeft * 100 / totalTime);
      loadingText.text = timeLeft > 60 ? Math.Floor((double)timeLeft / 60) + Language.Field["MINUTES_FIRST_LETTER"] + " " + timeLeft % 60 + Language.Field["SECONDS_FIRST_LETTER"] : timeLeft + Language.Field["SECONDS_FIRST_LETTER"];
    }
  }

  //*****************************************************************
  // PUBLIC: Call this with the name of the building you want to build
  //*****************************************************************
  public void main(string buildingName)
  {
    // Check if something is already being built
    if (currentlyBuilding != null)
    {
      ui.showPopupMessage(Language.Field["DUPLICATE_BUILDING"]);
      return;
    }
    // Check if the building and headquarters have already been built
    Building headquarter = null;
    foreach (Building b in controller.getUser().getVillage().getBuildings())
    {
      if (b.getName() == buildingName)
      {
        building = b;
        newBuilding = false;
      }
      if (b.getName() == "Headquarter"){
        headquarter = b;
      }
    }
    if (headquarter==null && buildingName != "Headquarter") {
      ui.showPopupMessage(Language.Field["HEADQUARTERS_FIRST"]);
      return;
    }
    if (!newBuilding && headquarter.getLevel()==building.getLevel()){
      ui.showPopupMessage(Language.Field["UPGRADE_HEADQUARTERS"]);
      building = null;
      newBuilding = true;
      return;
    }
    // If it doesn't exist already, create a new building object
    if (newBuilding) building = createBuilding(buildingName);
    // Check if user can afford the building, if yes pay
    if (!canAfford(building))
    {
      ui.showPopupMessage(Language.Field["NOT_RESOURCES"]);
      building = null;
      newBuilding = true;
      return;
    }
    // Register the new building with the controller
    if (newBuilding) controller.getUser().getVillage().addBuilding(building);
    // Spawn building
    startConstruction(building);
  }

  //*****************************************************************
  // Helper methods
  //*****************************************************************

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

  //This fetches the prefab of the building to be shown
  GameObject getPrefab(string buildingName)
  {
    return (GameObject)Resources.Load("Prefabs/" + new CultureInfo("en-US", false).TextInfo.ToTitleCase(buildingName), typeof(GameObject));
  }

  //This starts the construction of a building
  void startConstruction(Building b)
  {
    if (!newBuilding) b.setCompletionTime(DateTime.UtcNow.AddSeconds(b.getValue()/4 * (b.getLevel()+1)));
    currentlyBuilding = b;
    loadingBar.transform.position = b.getPosition(); ;
    loadingBar.SetActive(true);
    playBuildingSound();
  }

  //This instantiates the building on the scene and implements its functionality
  void spawn(Building b)
  {
    // Instantiate building on the scene
    GameObject buildingObj = (GameObject)Instantiate(b.getPrefab(), b.getPosition(), Quaternion.identity);
    // Implement buildings functionality
    b.startFunctionality(controller);
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


  public void playBuildingSound(){
    audioSource.Play();
  }
}
