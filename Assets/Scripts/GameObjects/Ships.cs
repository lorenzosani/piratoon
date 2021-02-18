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
    new Vector3(2.5f, -5.5f, 0),
    new Vector3(3, -7, 0),
    new Vector3(7.5f, -11, 0)
  };

  public ShipyardMenu shipyardMenu;
  public GameObject[] shipPrefabs;
  public GameObject finishShipPopup;

  //*****************************************************************
  // START and UPDATE methods
  //*****************************************************************
  void Start() {
    populateVariables();
  }

  void Update() {
    detectClick();
    if (currentlyBuilding != null && SceneManager.GetActiveScene().name == "Hideout") {
      int totalTime = (currentlyBuilding.getSlot() + 1) * currentlyBuilding.getLevel() * 300;
      int timeLeft = (int)(currentlyBuilding.getCompletionTime() - System.DateTime.UtcNow).TotalSeconds;
      if (timeLeft <= 0) {
        finishShip(currentlyBuilding);
      }
      loadingText.text = ui.formatTime(timeLeft);
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
      newShip.setCurrentPosition(new Vector3(1000, 1000, 1000));
    }
    // Check if user can afford the building, if yes pay
    if (!canAfford(newShip)) {
      ui.showPopupMessage(Language.Field["NOT_RESOURCES"]);
      return;
    }
    // Register the new ship with the controller
    newShip.increaseLevel();
    controller.getUser().getVillage().setShip(newShip, slot);
    // Spawn ships in the hideout view
    startConstruction(newShip);
  }

  //*****************************************************************
  // Helper methods
  //*****************************************************************

  // DETECT general clicks or taps on the map
  void detectClick() {
    if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended) {
      onShipClick(Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position));
    } else if (Input.GetMouseButtonUp(0)) {
      onShipClick(Camera.main.ScreenToWorldPoint(Input.mousePosition));
    }
  }

  // HANDLE clicks or taps on ship
  void onShipClick(Vector3 position) {
    Vector2 position2d = new Vector2(position.x, position.y);
    RaycastHit2D raycastHit = Physics2D.Raycast(position2d, Vector2.zero);
    if (IsPointerOverUIObject())return;
    if (raycastHit) {
      Debug.Log(raycastHit.collider.name);
      string shipName = raycastHit.collider.name;
      // Check if the click is on a hideout
      if (shipName.Split('_')[0] == "ship") {
        ui.showShipInfo(Int32.Parse(shipName.Split('_')[1]));
      }
    }
  }

  bool IsPointerOverUIObject() {
    if (EventSystem.current.IsPointerOverGameObject())
      return true;
    if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended) {
      if (EventSystem.current.currentSelectedGameObject) {
        return true;
      }
    }
    return false;
  }

  // Populate the variables for this script at launch
  void populateVariables() {
    controller = GetComponent<ControllerScript>();
    ui = controller.getUI();
    audioSource = GetComponent<AudioSource>();
  }

  void finishShip(Ship s) {
    // Add ship's value to user's bounty
    controller.getUser().addBounty(s.getLevel() * s.getSlot() * 100);
    // Spawn the ship on the scene
    s.setCurrentPosition(MapPositions.get(controller.getUser().getVillage().getPosition()));
    s.setBuilt(true);
    spawn(s);
    currentlyBuilding = null;
    shipyardMenu.setConstructionFinished(s.getSlot());
  }

  //This starts the construction of a building
  void startConstruction(Ship s, bool fromServer = false) {
    loadingText = shipyardMenu.getConstructionTimer(s.getSlot());
    currentlyBuilding = s;
    s.setBuilt(false);
    if (!fromServer)playBuildingSound();
  }

  // Populate ships from server
  public void populateShip(Ship ship) {
    // If the ship is null do nothing
    if (ship == null)return;
    int timeLeft = (int)(ship.getCompletionTime() - System.DateTime.UtcNow).TotalSeconds;
    if (timeLeft > 0) {
      // If the ship is in construction set it in construction
      startConstruction(ship, true);
    } else if (!ship.isBuilt()) {
      // If the construction complete when the user was not logged in, do this
      finishShip(ship);
    } else {
      // If the ship is built, spawn it
      spawn(ship);
    }
  }

  //This instantiates the building on the scene and implements its functionality
  void spawn(Ship s) {
    // Instantiate ship on the scene
    GameObject shipObj = (GameObject)Instantiate(getShipPrefab(s.getLevel()), shipPositions[s.getSlot()], Quaternion.identity);
    // Show the ship with the adequate size
    shipObj.transform.GetChild(0).localScale = new Vector3(0.5f, 0.5f, 1.0f);
    // If the ship is navigating, hide it in the hideout
    shipObj.SetActive(s.getCurrentPosition() == MapPositions.get(controller.getUser().getVillage().getPosition()));
    // Set object properties
    shipObj.name = "ship_" + s.getSlot();
    shipObj.layer = 9;
    // Add click listener to the ship

  }

  // Return the correct image of the ship, based on the level
  GameObject getShipPrefab(int level) {
    return level > 6 ? shipPrefabs[5] : shipPrefabs[level - 1];
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

  public void showPearlsConfirmation() {
    // Get the cost in pearls
    int timeLeft = (int)(currentlyBuilding.getCompletionTime() - System.DateTime.UtcNow).TotalSeconds;
    int cost = (int)Math.Pow(Math.Pow(timeLeft / 60, 2), (double)1 / 3);
    // Populate the popup
    Transform textObj = finishShipPopup.transform.Find("Text");
    textObj.GetComponent<Text>().text = String.Format(Language.Field["PAY_PEARL"], cost.ToString());
    Transform buttonObj = finishShipPopup.transform.Find("Button");
    buttonObj.Find("Cost").GetComponent<Text>().text = cost.ToString();
    // Show the popup
    finishShipPopup.SetActive(true);
  }

  public void buildWithPearls() {
    // Get the cost in pearls
    int timeLeft = (int)(currentlyBuilding.getCompletionTime() - System.DateTime.UtcNow).TotalSeconds;
    int cost = (int)Math.Pow(Math.Pow(timeLeft / 60, 2), (double)1 / 3);
    cost = cost < 1 ? 1 : cost;
    // Check if the user has enough pearls
    if (cost > controller.getUser().getPearl()) {
      // Show message stating there are not enough pearls
      controller.getUI().showPopupMessage(Language.Field["NOT_PEARL"]);
      return;
    }
    controller.getUser().setPearl(controller.getUser().getPearl() - cost);
    finishShip(currentlyBuilding);
  }

  void playBuildingSound() {
    audioSource.Play();
  }
}