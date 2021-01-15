using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapController : MonoBehaviour {
  ControllerScript controller;
  GameObject[] cityTooltips = new GameObject[CityNames.getCitiesNumber()];

  public MapUI ui;
  public GameObject worldSpaceUi;
  public GameObject hideoutsParent;
  public Sprite conqueredCityIcon;
  public Sprite[] resourcesIcons;

  void Start() {
    ui.showLoadingScreen();
    controller = GameObject.Find("GameController").GetComponent<ControllerScript>();

    populateMap();
    moveCameraOverHideout(controller.getUser().getVillage().getPosition());
    ui.showLoadingScreen(false);
    Camera.main.GetComponent<PanAndZoom>().Zoom(2, 5);
    checkCityProduction();
    InvokeRepeating("checkCityProduction", 5.0f, 5.0f);
  }

  //*****************************************************************
  // GET the script that manages the map UI
  //*****************************************************************
  public MapUI getUI() {
    return ui;
  }

  //*****************************************************************
  // POPULATE the map with the correct players' hideouts
  //*****************************************************************
  void populateMap() {
    MapUser[] players = controller.getMap().getPlayers();
    for (int i = 0; i < players.Length; i++) {
      if (players[i] != null && players[i].getId() != null && players[i].getId() != "") {
        // Instantiate the hideout icon at the right position
        GameObject hideout = (GameObject)Instantiate(
          (GameObject)Resources.Load("Prefabs/Hideout", typeof(GameObject)), MapPositions.get(i),
          Quaternion.identity
        );
        hideout.transform.parent = hideoutsParent.transform;
        hideout.layer = 8;
        // The hideout object has name: 'hideout_[position]_[userId]' DO NOT CHANGE THIS
        hideout.name = String.Format("hideout_{0}_{1}", i, players[i].getId());
        // Put the username above the hideout in the world space UI
        GameObject username = (GameObject)Instantiate(
          (GameObject)Resources.Load("Prefabs/MapUsername", typeof(GameObject)),
          worldSpaceUi.transform,
          false
        );
        username.GetComponent<Text>().text = players[i].getUsername();
        username.transform.position = new Vector3(MapPositions.get(i).x, MapPositions.get(i).y + 0.8f, 0.0f);
      }
    }
    AstarPath.active.Scan();
  }

  //*****************************************************************
  // SHOW information about a hideout or a city
  //*****************************************************************
  public void showHideoutInfo(string userData, string villageData) {
    User user = JsonConvert.DeserializeObject<User>(userData);
    Village village = JsonConvert.DeserializeObject<Village>(villageData);
    ui.populateHideoutPopup(
      user.getUsername(),
      user.getLevel(),
      user.getResources(),
      village.getStrength()
    );
  }
  public void showCityInfo(City city) {
    if (city.getOwner() == API.playFabId) {
      ui.showConqueredCityPopup(
        city.getName(),
        city.getLevel(),
        city.getHourlyProduction(),
        city.getResources(),
        city.getUpgradeCost(),
        city.getCooldownEnd());
    } else {
      ui.showCityPopup(
        city.getName(),
        city.getLevel(),
        city.getHourlyProduction(),
        city.getResources(),
        city.getCooldownEnd(),
        city.getOwner()
      );
    }
  }

  //*****************************************************************
  // PUTS the camera just above the user's hideout
  //*****************************************************************
  void moveCameraOverHideout(int hideoutPosition) {
    Camera.main.transform.position = new Vector3(
      MapPositions.get(hideoutPosition).x, MapPositions.get(hideoutPosition).y, -10.0f
    );
  }

  //*****************************************************************
  // CLOSE the map and show the hideout
  //*****************************************************************
  public async void close() {
    Camera.main.GetComponent<PanAndZoom>().Zoom(Camera.main.orthographicSize, 3);
    await Task.Delay(200);
    ui.showLoadingScreen();
    Destroy(GameObject.Find("GameController"));
    SceneManager.LoadScene("Hideout", LoadSceneMode.Single);
  }

  //*****************************************************************
  // CHECK whether resources produced by conquered cities can be collected
  //*****************************************************************
  public void checkCityProduction() {
    Debug.Log("CHECKCITYPROD");
    // Get all cities owned by the user
    int[] cities = controller.getMap().getCitiesOwnedBy(API.playFabId);
    // For each city check whether there are resources to collect
    foreach (int cityNumber in cities) {
      City city = controller.getMap().getCity(cityNumber);
      GameObject cityObject = GameObject.Find("city_" + cityNumber);
      Vector3 pos = cityObject.transform.position;
      // Set the correct icon
      cityObject.GetComponent<SpriteRenderer>().sprite = conqueredCityIcon;
      int[] resources = city.getResources();
      if (resources.All(x => x <= 0) || cityTooltips[cityNumber] != null)return;
      // Spawn a new tooltip prefab at that position
      cityTooltips[cityNumber] = (GameObject)Instantiate(
        getTooltipPrefab(resources), worldSpaceUi.transform, false);
      cityTooltips[cityNumber].transform.position = new Vector3(pos.x, pos.y + 0.5f, 0.0f);
      // Populate its children with the correct icons and trigger functions
      int counter = 0;
      for (int i = 0; i < 3; i++) {
        if (resources[i] > 0) {
          GameObject iconObject = cityTooltips[cityNumber].transform.GetChild(counter).gameObject;
          iconObject.GetComponent<Image>().sprite = resourcesIcons[i];
          if (i == 0)addClickListener(iconObject, 0, resources, city, cityNumber);
          if (i == 1)addClickListener(iconObject, 1, resources, city, cityNumber);
          if (i == 2)addClickListener(iconObject, 2, resources, city, cityNumber);
          counter++;
        }
      }
    }
  }
  // Generate a resources tooltip based on the type of resources to collect
  GameObject getTooltipPrefab(int[] resources) {
    int size = resources.Where(x => x > 0).Count();
    return (GameObject)Resources.Load("Prefabs/CityTooltip" + size.ToString(), typeof(GameObject));
  }
  // Add a click listener to a building and the function it calls
  void addClickListener(GameObject obj, int resNo, int[] resources, City city, int cityNumber) {
    EventTrigger.Entry entry = new EventTrigger.Entry();
    entry.eventID = EventTriggerType.PointerClick;
    entry.callback.AddListener((baseData) => {
      int freeSpace = controller.getUser().getStorageSpaceLeft()[resNo];
      if (freeSpace == 0) {
        ui.showPopupMessage(Language.Field["CITY_STORAGE"]);
        return;
      }
      int resourcesToCollect = resources[resNo] < freeSpace ? resources[resNo] : freeSpace;
      int remainingResources = resources[resNo] - resourcesToCollect;
      city.setResource(resNo, remainingResources);
      controller.getUser().increaseResource(resNo, resourcesToCollect);
      Destroy(cityTooltips[cityNumber]);
      cityTooltips[cityNumber] = null;
      checkCityProduction();
    });
    obj.layer = 10;
    obj.GetComponent<EventTrigger>().triggers.Add(entry);
  }
}