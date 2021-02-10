using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Buildings : MonoBehaviour {
  ControllerScript controller;
  Ships shipsScript;
  Building currentlyBuilding = null;
  Slider loadingSlider;
  Text loadingText;
  AudioSource audioSource;
  bool newBuilding;
  UI ui;

  public FloatingObjects floatScript;
  public GameObject loadingBar;
  public GameObject finishBuildingPopup;
  public bool checkHeadquarter;

  TouchPhase lastTouchPhase = TouchPhase.Ended;

  //*****************************************************************
  // START and UPDATE methods
  //*****************************************************************

  void Start() {
    populateVariables();
  }

  void Update() {
    detectClick();
    if (currentlyBuilding != null && SceneManager.GetActiveScene().name == "Hideout") {
      int totalTime = currentlyBuilding.getConstructionDuration();
      int timeLeft = (int)(currentlyBuilding.getCompletionTime() - System.DateTime.UtcNow).TotalSeconds;
      if (timeLeft <= 0) {
        finishBuilding(currentlyBuilding);
      }
      loadingSlider.value = (int)100 - (timeLeft * 100 / totalTime);
      loadingText.text = ui.formatTime(timeLeft);
    }
  }

  //*****************************************************************
  // PUBLIC: Call this with the name of the building you want to build
  //*****************************************************************
  public void main(string buildingName) {
    Building building = null;
    Building headquarter = null;
    // Check if something is already being built
    if (currentlyBuilding != null) {
      ui.showPopupMessage(Language.Field["DUPLICATE_BUILDING"]);
      return;
    }
    // Check if the building and headquarters have already been built
    foreach (Building b in controller.getUser().getVillage().getBuildings()) {
      if (b.getName() == buildingName) {
        building = b;
        newBuilding = false;
      }
      if (b.getName() == "Headquarter") {
        headquarter = b;
      }
    }
    if (building == null)newBuilding = true;
    if (buildingName != "Headquarter") {
      if (checkHeadquarter && headquarter == null) {
        ui.showPopupMessage(Language.Field["HEADQUARTERS_FIRST"]);
        return;
      }
      if (checkHeadquarter && !newBuilding && headquarter != null && building != null && headquarter.getLevel() <= building.getLevel()) {
        ui.showPopupMessage(Language.Field["UPGRADE_HEADQUARTERS"]);
        newBuilding = true;
        return;
      }
    }
    // If it doesn't exist already, create a new building object
    if (newBuilding)building = createBuilding(buildingName);
    // Check if user can afford the building, if yes pay
    if (!canAfford(building)) {
      ui.showPopupMessage(Language.Field["NOT_RESOURCES"]);
      newBuilding = true;
      return;
    }
    // Register the new building with the controller
    if (newBuilding) {
      controller.getUser().getVillage().addBuilding(building);
    } else {
      building.setCompletionTime(DateTime.UtcNow.AddSeconds(building.getValue() * (building.getLevel() + 1)));
    }
    // Spawn building
    startConstruction(building);
  }

  //*****************************************************************
  // Helper methods
  //*****************************************************************

  // Populate the variables for this script at launch
  void populateVariables() {
    controller = GetComponent<ControllerScript>();
    ui = controller.getUI();
    shipsScript = GetComponent<Ships>();
    floatScript = GameObject.Find("FloatingObjects").GetComponent<FloatingObjects>();
    loadingSlider = loadingBar.GetComponent<Slider>();
    loadingText = loadingBar.GetComponentInChildren(typeof(Text), true)as Text;
    audioSource = GetComponent<AudioSource>();
    newBuilding = true;
  }

  // Show buildings from json data
  public void populateVillage(string buildingsJson) {
    Village village = controller.getUser().getVillage();
    List<Building> buildingsList = new List<Building>();

    JArray buildingsObject = JArray.Parse(buildingsJson);
    removeAllBuildings();
    for (int i = 0; i < buildingsObject.Count; i++) {
      Building b = createBuilding((string)buildingsObject[i]["name"]);
      b.setLevel((int)buildingsObject[i]["level"]);
      b.setPosition(
        new Vector3(
          (float)buildingsObject[i]["position"][0],
          (float)buildingsObject[i]["position"][1],
          (float)buildingsObject[i]["position"][2]
        )
      );
      string t = (string)buildingsObject[i]["completionTime"];
      string l = (string)buildingsObject[i]["lastCollected"];
      t = t.Replace("T", " ").Replace("Z", "");
      b.setCompletionTime(DateTime.ParseExact(t, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture));
      b.setValue((int)buildingsObject[i]["value"]);
      b.setLastCollected(DateTime.ParseExact(l, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture));
      b.setBuilt((bool)buildingsObject[i]["built"]);
      buildingsList.Add(b);
      addBuildingFromServer(b);
    }
    village.setBuildingsFromList(buildingsList);
  }

  // Shows spawned floating objects
  public void populateFloatingObjects() {
    int MAX_OBJS_TO_SHOW = 4;
    FloatingObject[] floatingObjects = controller.getUser().getVillage().getFloatingObjects();
    if (floatingObjects.Length > MAX_OBJS_TO_SHOW)
      controller.getUser().getVillage().setFloatingObjects(floatingObjects.Take(4).ToArray());
    for (int i = 0; i < MAX_OBJS_TO_SHOW; i++) {
      if (DateTime.Compare(floatingObjects[i].getTime(), DateTime.Now) < 0) {
        floatScript.spawn(floatingObjects[i]);
      }
    }
  }

  // Shows spawned floating objects
  public void populateShips() {
    Ship[] ships = controller.getUser().getVillage().getShips();
    for (int i = 0; i < ships.Length; i++) {
      shipsScript.populateShip(ships[i]);
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

  public void finishBuilding(Building b) {
    loadingBar.SetActive(false);
    b.increaseLevel();
    // Add building's value to user's bounty
    controller.getUser().addBounty(b.getValue());
    // Spawn the building on the scene
    spawn(b);
    b.setBuilt(true);
    b.startFunctionality(controller);
    // Reset global variables
    currentlyBuilding = null;
    newBuilding = true;
  }

  public void showPearlsConfirmation() {
    // Get the cost in pearls
    int timeLeft = (int)(currentlyBuilding.getCompletionTime() - System.DateTime.UtcNow).TotalSeconds;
    int cost = (int)Math.Pow(Math.Pow(timeLeft / 60, 2), (double)1 / 3);
    // Populate the popup
    Transform textObj = finishBuildingPopup.transform.Find("Text");
    textObj.GetComponent<Text>().text = String.Format(Language.Field["PAY_PEARL"], cost.ToString());
    Transform buttonObj = finishBuildingPopup.transform.Find("Button");
    buttonObj.Find("Cost").GetComponent<Text>().text = cost.ToString();
    // Show the popup
    finishBuildingPopup.SetActive(true);
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
    finishBuilding(currentlyBuilding);
  }

  //This starts the construction of a building
  void startConstruction(Building b) {
    currentlyBuilding = b;
    b.setBuilt(false);
    loadingBar.transform.position = b.getPosition();
    loadingBar.SetActive(true);
    playBuildingSound();
  }

  //This instantiates the building on the scene and implements its functionality
  void spawn(Building b) {
    // Destroy existing building
    Destroy(GameObject.Find(b.getName()));
    // Instantiate building on the scene
    GameObject buildingObj = (GameObject)Instantiate(b.getPrefab(), b.getPosition(), Quaternion.identity);
    // Set object properties
    buildingObj.name = b.getName();
    buildingObj.layer = 9;
  }

  void addBuildingFromServer(Building b) {
    b.setPosition(b.getPrefab().transform.position);
    foreach (Building bu in controller.getUser().getVillage().getBuildings()) {
      if (bu.getName() == b.getName()) {
        newBuilding = false;
      }
    }
    if ((b.getCompletionTime() - System.DateTime.UtcNow).TotalSeconds > 0) {
      if (!newBuilding)spawn(b);
      startConstruction(b);
    } else if (b.isBuilt() == false) {
      finishBuilding(b);
    } else {
      spawn(b);
      if (b.getName() == "Woodcutter" || b.getName() == "Stonecutter" || b.getName() == "Inn") {
        if (b.getLocalStorage() > 0) {
          GetComponent<UserResources>().showTooltip(b.getName());
        }
      }
    }
  }

  void removeAllBuildings() {
    loadingBar.SetActive(false);
    var objects = FindObjectsOfType(typeof(GameObject));
    foreach (GameObject obj in objects) {
      if (obj.layer == 9) {
        Destroy(obj);
      }
    }
  }

  //This checks wether the suer can afford to buy a building; if yes, pay the price
  bool canAfford(Building b) {
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

  void detectClick() {
    if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended && lastTouchPhase != TouchPhase.Moved) {
      onBuildingClick(Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position));
    } else if (Input.GetMouseButtonUp(0) && Input.touchCount == 0) {
      onBuildingClick(Camera.main.ScreenToWorldPoint(Input.mousePosition));
    }
    if (Input.touchCount > 0 && Input.GetTouch(0).phase != lastTouchPhase) {
      lastTouchPhase = Input.GetTouch(0).phase;
    }
  }

  // HANDLE clicks or taps on a building
  void onBuildingClick(Vector3 position) {
    Vector2 position2d = new Vector2(position.x, position.y);
    RaycastHit2D raycastHit = Physics2D.Raycast(position2d, Vector2.zero);
    if (IsPointerOverUIObject())return;
    if (raycastHit) {
      string hitName = raycastHit.collider.name;
      List<Building> buildings = controller.getUser().getVillage().getBuildings();
      string[] buildingNames = buildings.Select(b => b.getName()).ToArray();
      if (buildingNames.Contains(hitName) && controller.getUser().getVillage().getBuildingInfo(hitName).isBuilt()) {
        ui.showBuildingInfo(hitName);
      }
    }
  }

  bool IsPointerOverUIObject() {
    if (EventSystem.current.IsPointerOverGameObject())
      return true;
    if (EventSystem.current.IsPointerOverGameObject(0))
      return true;
    if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended) {
      if (EventSystem.current.currentSelectedGameObject) {
        return true;
      }
    }
    return false;
  }

  void playBuildingSound() {
    audioSource.Play();
  }
}