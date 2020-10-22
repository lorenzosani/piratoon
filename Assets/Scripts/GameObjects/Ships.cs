using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Ships : MonoBehaviour {
  ControllerScript controller;
  Ship currentlyBuilding = null;
  AudioSource audioSource;
  UI ui;
  Text loadingText;

  public ShipyardMenu shipyardMenu;

  //*****************************************************************
  // START and UPDATE methods
  //*****************************************************************

  void Start() {
    populateVariables();
  }

  void Update() {
    if (currentlyBuilding != null && SceneManager.GetActiveScene().name == "Hideout") {
      int totalTime = currentlyBuilding.getSlot() * currentlyBuilding.getLevel() * 300;
      int timeLeft = (int)(currentlyBuilding.getCompletionTime() - System.DateTime.UtcNow).TotalSeconds;
      if (timeLeft <= 0) {
        finishShip(currentlyBuilding);
      }
      loadingText.text = timeLeft > 60 ? Math.Floor((double)timeLeft / 60) + Language.Field["MINUTES_FIRST_LETTER"] + " " + timeLeft % 60 + Language.Field["SECONDS_FIRST_LETTER"] : timeLeft + Language.Field["SECONDS_FIRST_LETTER"];
    }
  }

  //*****************************************************************
  // PUBLIC: Call this with the slot number of the ship you want to build
  //*****************************************************************
  public void buildShip(int slot) {
    Ship ship = controller.getUser().getVillage().getShip(slot);
    Ship newShip = null;
    if (ship != null) {
      // If it doesn't exist already, create a new ship object
      newShip = new Ship(
        "Ship" + slot, MapPositions.get(controller.getUser().getVillage().getPosition()), slot
      );
    } else {
      // Otherwise create a duplicate of the current ship
      newShip = JsonConvert.DeserializeObject<Ship>(
        JsonConvert.SerializeObject(ship),
        new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace }
      );
      newShip.increaseLevel();
    }
    // Check if user can afford the building, if yes pay
    if (!canAfford(newShip)) {
      ui.showPopupMessage(Language.Field["NOT_RESOURCES"]);
      return;
    }
    // Register the new ship with the controller
    controller.getUser().getVillage().setShip(newShip, slot);
    // Spawn ships in the hideout view
    startConstruction(newShip);
  }

  //*****************************************************************
  // Helper methods
  //*****************************************************************

  // Populate the variables for this script at launch
  void populateVariables() {
    controller = GetComponent<ControllerScript>();
    ui = controller.getUI();
    audioSource = GetComponent<AudioSource>();
  }

  void finishShip(Ship s) {
    // Add ship's value to user's bounty
    controller.getUser().addBounty(s.getLevel() * s.getSlot() * 100);
    // Spawn the building on the scene
    //spawn(s);
    currentlyBuilding = null;
  }

  //This starts the construction of a building
  void startConstruction(Ship s) {
    currentlyBuilding = s;
    loadingText = shipyardMenu.setCurrentlyBuilding(true, s.getSlot()).GetComponent<Text>();
    playBuildingSound();
  }

  // //This instantiates the building on the scene and implements its functionality
  // void spawn(Building b) {
  //   // Instantiate building on the scene
  //   GameObject buildingObj = (GameObject)Instantiate(b.getPrefab(), b.getPosition(), Quaternion.identity);
  //   // Set object properties
  //   buildingObj.name = b.getName();
  //   buildingObj.layer = 9;
  //   // Add click listener to some types of buildings
  //   if (b.getName() == "Shipyard") {
  //     addClickListener(buildingObj, () => ui.showShipyardMenu());
  //   }
  // }

  //This checks wether the suer can afford to buy a building; if yes, pay the price
  bool canAfford(Ship b) {
    int[] resources = controller.getUser().getResources();
    int[] cost = b.getCost();
    int[] remainingResources = new int[3];
    for (int i = 0; i < resources.Length; i++) {
      remainingResources[i] = resources[i] - cost[i];
      if (remainingResources[i] < 0)return false;
    }
    controller.getUser().setResources(remainingResources);
    return true;
  }

  public void playBuildingSound() {
    audioSource.Play();
  }
}