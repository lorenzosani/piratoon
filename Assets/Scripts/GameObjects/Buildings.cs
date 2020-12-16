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
  public bool checkHeadquarter;

  //*****************************************************************
  // START and UPDATE methods
  //*****************************************************************

  void Start() {
    populateVariables();
  }

  void Update() {
    if (currentlyBuilding != null && SceneManager.GetActiveScene().name == "Hideout") {
      int totalTime = currentlyBuilding.getFutureValue() / 4;
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
    if (buildingName != "Headquarter") {
      if (checkHeadquarter && headquarter == null) {
        ui.showPopupMessage(Language.Field["HEADQUARTERS_FIRST"]);
        return;
      }
      if (checkHeadquarter && !newBuilding && headquarter != null && headquarter.getLevel() <= building.getLevel()) {
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
    FloatingObject[] floatingObjects = controller.getUser().getVillage().getFloatingObjects();
    for (int i = 0; i < floatingObjects.Length; i++) {
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

  void finishBuilding(Building b) {
    loadingBar.SetActive(false);
    b.setBuilt(true);
    b.increaseLevel();
    // Add building's value to user's bounty
    controller.getUser().addBounty(b.getValue());
    // Spawn the building on the scene
    if (newBuilding)spawn(b);
    b.startFunctionality(controller);
    // Reset global variables
    currentlyBuilding = null;
    newBuilding = true;
  }

  //This fetches the prefab of the building to be shown
  GameObject getPrefab(string buildingName) {
    return (GameObject)Resources.Load("Prefabs/" + new CultureInfo("en-US", false).TextInfo.ToTitleCase(buildingName), typeof(GameObject));
  }

  //This starts the construction of a building
  void startConstruction(Building b) {
    currentlyBuilding = b;
    b.setBuilt(false);
    loadingBar.transform.position = b.getPosition();
    loadingBar.SetActive(true);
    playBuildingSound();
    API.SetUserData(new string[] {
      "Buildings",
      "User",
      "Village"
    });
  }

  //This instantiates the building on the scene and implements its functionality
  void spawn(Building b) {
    // Instantiate building on the scene
    GameObject buildingObj = (GameObject)Instantiate(b.getPrefab(), b.getPosition(), Quaternion.identity);
    // Set object properties
    buildingObj.name = b.getName();
    buildingObj.layer = 9;
    // Add click listener to some types of buildings
    if (b.getName() == "Shipyard") {
      addClickListener(buildingObj, () => ui.showShipyardMenu());
    }
  }

  // Add a click listener to a building and the function it calls
  void addClickListener(GameObject obj, Action functionality) {
    EventTrigger.Entry entry = new EventTrigger.Entry();
    entry.eventID = EventTriggerType.PointerClick;
    entry.callback.AddListener(new UnityAction<BaseEventData>((baseData) => functionality()));
    obj.layer = 10;
    obj.GetComponent<EventTrigger>().triggers.Add(entry);
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

  void playBuildingSound() {
    audioSource.Play();
  }
}