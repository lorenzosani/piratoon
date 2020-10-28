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
  AudioSource audioSource;
  UI ui;
  Text loadingText;

  Ship currentlyBuilding = null;
  Vector3[] shipPositions = new Vector3[3] {
    new Vector3(),
    new Vector3(),
    new Vector3()
  };

  public ShipyardMenu shipyardMenu;
  public GameObject[] shipPrefabs;

  //*****************************************************************
  // START and UPDATE methods
  //*****************************************************************

  void Start() {
    populateVariables();
  }

  void Update() {
    if (currentlyBuilding != null && SceneManager.GetActiveScene().name == "Hideout") {
      int totalTime = (currentlyBuilding.getSlot() + 1) * currentlyBuilding.getLevel() * 300;
      int timeLeft = (int)(currentlyBuilding.getCompletionTime() - System.DateTime.UtcNow).TotalSeconds;
      if (timeLeft <= 0) {
        finishShip(currentlyBuilding);
      }
      loadingText.text = timeLeft > 60 ?
        Math.Floor((double)timeLeft / 60) + Language.Field["MINUTES_FIRST_LETTER"] + " " + timeLeft % 60 + Language.Field["SECONDS_FIRST_LETTER"] :
        timeLeft + Language.Field["SECONDS_FIRST_LETTER"];
    }
  }

  //*****************************************************************
  // PUBLIC: Call this with the slot number of the ship you want to build
  //*****************************************************************
  public void buildShip(int slot) {
    Ship ship = controller.getUser().getVillage().getShip(slot);
    Ship newShip = null;
    if (ship == null) {
      // If it doesn't exist already, create a new ship object
      newShip = new Ship(
        "My Ship", MapPositions.get(controller.getUser().getVillage().getPosition()), slot
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
    // TODO: Spawn the ship on the scene
    spawn(s);
    currentlyBuilding = null;
    shipyardMenu.setConstructionFinished();
  }

  //This starts the construction of a building
  void startConstruction(Ship s, bool fromServer = false) {
    loadingText = shipyardMenu.getConstructionTimer(s.getSlot());
    currentlyBuilding = s;
    if (!fromServer)playBuildingSound();
  }

  // Populate ships from server
  public void populateShip(Ship ship) {
    // If the ship is null do nothing
    if (ship == null)return;
    // If the ship is in construction set it in construction
    int timeLeft = (int)(ship.getCompletionTime() - System.DateTime.UtcNow).TotalSeconds;
    if (timeLeft > 0) {
      startConstruction(ship, true);
    }
    // If the ship is built, spawn it
    //spawn(ship);
  }

  //This instantiates the building on the scene and implements its functionality
  void spawn(Ship s) {
    // Instantiate ship on the scene
    GameObject shipObj = (GameObject)Instantiate(getShipPrefab(s.getLevel()), getPositionInHideout(s.getSlot()), Quaternion.identity);
    // If the ship is navigating, hide it in the hideout
    shipObj.SetActive(s.getCurrentPosition() == MapPositions.get(controller.getUser().getVillage().getPosition()));
    // Set object properties
    shipObj.name = "ship_" + s.getSlot();
    shipObj.layer = 9;
    // TODO: Add click listener to the ship
  }

  // Return the correct image of the ship, based on the level
  GameObject getShipPrefab(int level) {
    return level > 4 ? shipPrefabs[3] : shipPrefabs[level - 1];
  }

  // Return the correct position of the ship in the hideout, based on its slot
  Vector3 getPositionInHideout(int slot) {
    return new Vector3();
  }

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

  void playBuildingSound() {
    audioSource.Play();
  }
}