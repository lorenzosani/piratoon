using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pathfinding;
using PlayFab;
using PlayFab.AdminModels;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Attacks : MonoBehaviour {
  static int SHIPS_MAX_NUMBER = 3;
  string selectedTarget = null;

  public GameObject timerPrefab;
  public GameObject lineRendererPrefab;
  public GameObject shipMarkerIcon;
  public GameObject worldSpaceUi;

  ControllerScript controller;
  MapController mapController;
  MapUI ui;

  ShipPath[] paths = new ShipPath[SHIPS_MAX_NUMBER];
  bool[] destinationReached = new bool[SHIPS_MAX_NUMBER];
  AIDestinationSetter[] destinations = new AIDestinationSetter[SHIPS_MAX_NUMBER];
  GameObject[] shipsSpawned = new GameObject[SHIPS_MAX_NUMBER];
  GameObject[] timersSpawned = new GameObject[SHIPS_MAX_NUMBER];
  LineRenderer[] lineRenderers = new LineRenderer[SHIPS_MAX_NUMBER];
  GameObject[] shipMarkers = new GameObject[SHIPS_MAX_NUMBER];

  void Update() {
    detectClick();
    detectDirection();
    updateTimers();
  }

  void Start() {
    controller = GameObject.Find("GameController").GetComponent<ControllerScript>();
    mapController = GetComponent<MapController>();
    ui = mapController.getUI();
    InvokeRepeating("checkIfDestinationReached", 0.5f, 0.5f);

    // Populate ships on the map
    Ship[] ships = controller.getUser().getVillage().getShips();
    for (int i = 0; i < ships.Length; i++) {
      if (ships[i] != null) {
        if (ships[i].getCurrentJourney() != null) {
          spawnShip(i);
        } else {
          // Mark village/city where they're parked
          Vector3 position = ships[i].getCurrentPosition();
          Vector2 position2d = new Vector2(position.x, position.y);
          RaycastHit2D raycastHit = Physics2D.Raycast(position2d, Vector2.zero);
          if (raycastHit) {
            string hideoutName = raycastHit.collider.name;
            if (hideoutName.Split('_')[0] == "hideout" || hideoutName.Split('_')[0] == "city") {
              addShipMarker(GameObject.Find(hideoutName).transform, i);
            }
          }
        }
      }
    }
  }

  //*****************************************************************
  // PUBLIC: call this when you want to start an attack
  //*****************************************************************
  public void startAttack() {
    Ship[] shipsOwned = controller.getUser().getVillage().getShips();
    // If the user has no ships available show a message
    if (shipsOwned.Count(s => s != null) == 0) {
      ui.showPopupMessage(Language.Field["NO_SHIPS"]);
      return;
    }
    // If the user has more than one ship, let them pick one for the attack
    if (shipsOwned.Count(s => s != null) > 1) {
      ui.showHideoutPopup(false);
      ui.showShipPicker(shipsOwned, true);
    } else {
      // Otherwise just set the only ship as the attacking ship
      foreach (Ship s in shipsOwned) {
        if (s != null) {
          ui.showHideoutPopup(false);
          spawnShip(s.getSlot());
          break;
        }
      }
    }
  }

  //*****************************************************************
  // DETECT general clicks or taps on the map
  //*****************************************************************
  void detectClick() {
    if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended) {
      onHideoutClick(Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position));
    } else if (Input.GetMouseButtonUp(0)) {
      onHideoutClick(Camera.main.ScreenToWorldPoint(Input.mousePosition));
    }
  }

  //*****************************************************************
  // CHANGE the ship orientation based on its navigation direction
  //*****************************************************************
  void detectDirection() {
    for (int i = 0; i < SHIPS_MAX_NUMBER; i++) {
      if (shipsSpawned[i] == null)continue;
      // Get the sprite renderer
      SpriteRenderer sr = shipsSpawned[i].transform.GetChild(0).GetComponent<SpriteRenderer>();
      Vector3 speed = paths[i].desiredVelocity;
      float threshold = paths[i].maxSpeed / 2.0f;
      string direction = "/S";
      if (speed.x <= threshold && speed.x >= -threshold) {
        if (speed.y >= threshold) {
          direction = "/N";
        } else {
          direction = "/S";
        }
      } else if (speed.x >= threshold) {
        if (speed.y >= -threshold && speed.y <= threshold) {
          direction = "/E";
        } else if (speed.y >= threshold) {
          direction = "/NE";
        } else {
          direction = "/SE";
        }
      } else {
        if (speed.y >= -threshold && speed.y <= threshold) {
          direction = "/W";
        } else if (speed.y >= threshold) {
          direction = "/NW";
        } else {
          direction = "/SW";
        }
      }
      int shipLevel = controller.getUser().getVillage().getShip(i).getLevel();
      sr.sprite = (Sprite)Resources.Load("Images/Ships/Ship" + (shipLevel > 5 ? "4" : (shipLevel).ToString()) + direction, typeof(Sprite));
    }
  }

  //*****************************************************************
  // UPDATE the timers that indicate the ships' time left until arrival
  //*****************************************************************
  void updateTimers() {
    for (int i = 0; i < SHIPS_MAX_NUMBER; i++) {
      if (timersSpawned[i] == null || paths[i].getPath() == null)continue;
      // Update the position
      Vector3 shipPosition = shipsSpawned[i].transform.position;
      timersSpawned[i].transform.position = new Vector3(shipPosition.x, shipPosition.y + 0.6f, 0.0f);
      // Update the content
      int timeLeft = (int)(paths[i].remainingDistance / paths[i].maxSpeed);
      timersSpawned[i].transform.Find("Text").GetComponent<Text>().text = controller.getUI().formatTime(timeLeft);
    }
  }

  //*****************************************************************
  // CHECK if any ship has reached its destination
  //*****************************************************************
  Ship latestShip = null;
  string latestDestination = "";
  void checkIfDestinationReached() {
    for (int i = 0; i < SHIPS_MAX_NUMBER; i++) {
      if (paths[i] != null && paths[i].reachedEndOfPath && !destinationReached[i]) {
        // Do this when a ship destination is reached
        latestDestination = controller.getUser().getVillage().getShip(i).getCurrentJourney().getDestinationName();
        Transform destination = GameObject.Find(latestDestination).transform;
        addShipMarker(destination, i);
        latestShip = controller.getUser().getVillage().getShip(i);
        latestShip.finishJourney(destination.position);
        destinationReached[i] = true;
        //// TODO: Need to check for a case where two ships arrive almost simultaneously, 
        //// as it stands the second one would overwrite the first
        // Generate attack outcome
        if (latestDestination.Split('_')[0] == "hideout") {
          if (latestDestination.Split('_')[2] != API.playFabId)generatePlunderOutcome();
        } else if (latestDestination.Split('_')[0] == "city") {
          int cityNo = Int32.Parse(latestDestination.Split('_')[1]);
          ui.showAttackOptions(CityNames.getCity(cityNo));
        }
        // Hide the ship on the map and the timer
        Destroy(shipsSpawned[i]);
        Destroy(paths[i]);
        Destroy(destinations[i]);
        Destroy(timersSpawned[i]);
        Destroy(lineRenderers[i]);
      }
    }
  }

  //*****************************************************************
  // Generate the outcome of a plundering attack
  //*****************************************************************
  public void generatePlunderOutcome() {
    int userStrength = 150 * latestShip.getLevel();
    // If the user is attacking an enemy hideout
    if (latestDestination.Split('_')[0] == "hideout") {
      computeHideoutAttack(latestShip, userStrength, latestDestination);
    } else {
      computeCityAttack(latestShip, userStrength, latestDestination);
    }
  }

  //*****************************************************************
  // Generate the outcome of a city conquest
  //*****************************************************************
  async void generateConquestOutcome() {
    bool userDataReceived = false;
    string cityName = latestDestination;
    int cityNumber = Int32.Parse(cityName.Split('_')[1]);
    int userStrength = 150 * latestShip.getLevel();
    Ship ship = latestShip;
    City city = controller.getMap().getCities()[cityNumber];
    User user = null;
    string outcomeMessage = "";
    // Get outcome
    if (getRandomOutcome(userStrength, city.getLevel() * 200)) { // User victory
      controller.getMap().setCityConquered(cityNumber, API.playFabId);
      outcomeMessage = String.Format(Language.Field["CITY_CONQUEST"], CityNames.getCity(cityNumber));
      controller.getUser().addBounty(city.getLevel() * 200);
      // This updates the map
      mapController.reRenderCities();
      // If the city is owned by someone, we need to tell them about the attack
      if (city.getOwner() != "" && city.getOwner() != API.playFabId) {
        API.GetUserData(
          new List<string> { "User" },
          result => {
            if (result.Data.ContainsKey("User"))user = JsonConvert.DeserializeObject<User>(result.Data["User"].Value);
            userDataReceived = true;
          },
          city.getOwner()
        );
        // Wait until that info is received
        while (!userDataReceived) {
          await Task.Delay(10);
        }
        // Register the attack and update user data
        user.registerAttack(new AttackOutcome('c', 'v', controller.getUser().getUsername(), city.getName()));
        PlayFabAdminAPI.UpdateUserData(new UpdateUserDataRequest() {
          PlayFabId = city.getOwner(), Data = new Dictionary<string, string>() {
            {
              "User",
              JsonConvert.SerializeObject(user, new JsonSerializerSettings {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
              })
            }
          }, Permission = UserDataPermission.Public
        }, result => {
          Debug.Log("Enemy data updated successfully.");
        }, e => API.OnPlayFabError(e));
      }
    } else { // User defeat
      Destroy(shipMarkers[ship.getSlot()]);
      controller.getUser().getVillage().setShip(null, ship.getSlot());
      outcomeMessage = Language.Field["ATTACK_DEFEAT"];
    }
    // Show outcome message
    ui.showPopupMessage(outcomeMessage);
  }

  //*****************************************************************
  // Compute the outcome of an attack to an enemy hideout
  //*****************************************************************
  async void computeHideoutAttack(Ship ship, int userStrength, string enemyName) {
    bool userDataReceived = false;
    int[] enemyResources = new int[3];
    string outcomeMessage = "";
    int enemyStrength = 200;
    User user = null;
    Village village = null;
    // Get information about the enemy
    API.GetUserData(
      new List<string> { "User", "Village" },
      result => {
        if (result.Data.ContainsKey("User") && result.Data.ContainsKey("Village")) {
          user = JsonConvert.DeserializeObject<User>(result.Data["User"].Value);
          village = JsonConvert.DeserializeObject<Village>(result.Data["Village"].Value);
          user.setVillage(village);
          enemyStrength = village.getStrength();
          enemyResources = user.getResources();
          userDataReceived = true;
        } else {
          Debug.Log("ERROR: Failed to retrieve enemy strength. Value set to default.");
          userDataReceived = true;
        }
      },
      enemyName.Split('_')[2]
    );
    // Wait until that infromation is received
    while (!userDataReceived) {
      await Task.Delay(10);
    }
    // If victory, compute the resources won
    if (getRandomOutcome(userStrength, enemyStrength)) {
      int[] resourcesWon = computeResourceWon(enemyResources, userStrength, enemyStrength);
      controller.getUser().increaseResource(0, resourcesWon[0]);
      controller.getUser().increaseResource(1, resourcesWon[1]);
      controller.getUser().increaseResource(2, resourcesWon[2]);
      // Add bounty equal to the village strength
      controller.getUser().addBounty(enemyStrength);
      outcomeMessage = String.Format(
        Language.Field["ATTACK_VICTORY"], resourcesWon[0], resourcesWon[1], resourcesWon[2]);
      // Update the resources of the enemy after the attack
      user.setResources(enemyResources.Select((elem, index) => elem - resourcesWon[index]).ToArray());
      user.registerAttack(new AttackOutcome('p', 'v', controller.getUser().getUsername(), "hideout", resourcesWon));
      PlayFabAdminAPI.UpdateUserData(new UpdateUserDataRequest() {
        PlayFabId = enemyName.Split('_')[2], Data = new Dictionary<string, string>() {
          {
            "User",
            JsonConvert.SerializeObject(user, new JsonSerializerSettings {
              ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            })
          }
        }, Permission = UserDataPermission.Public
      }, result => {
        Debug.Log("Enemy data updated successfully.");
      }, e => API.OnPlayFabError(e));
    } else {
      // If loss, compute whether ship is lost
      Destroy(shipMarkers[ship.getSlot()]);
      controller.getUser().getVillage().setShip(null, ship.getSlot());
      outcomeMessage = Language.Field["ATTACK_DEFEAT"];
    }
    // Show outcome message
    ui.showPopupMessage(outcomeMessage);
  }

  //*****************************************************************
  // Compute the outcome of an attack to a city
  //*****************************************************************
  async void computeCityAttack(Ship ship, int userStrength, string cityName) {
    bool userDataReceived = false;
    City city = controller.getMap().getCities()[Int32.Parse(cityName.Split('_')[1])];
    // Compute the attack outcome
    string outcomeMessage = "";
    if (getRandomOutcome(userStrength, city.getLevel() * 100)) { // User victory
      User user = null;
      int[] resourcesWon = computeResourceWon(city.getResources(), userStrength, city.getLevel() * 100);
      for (int i = 0; i < 3; i++) {
        controller.getUser().increaseResource(i, resourcesWon[i]);
        city.setResource(i, city.getResources()[i] - resourcesWon[i]);
      }
      // Add bounty equal to the village strength
      controller.getUser().addBounty(city.getLevel() * 100);
      outcomeMessage = String.Format(
        Language.Field["ATTACK_VICTORY"], resourcesWon[0], resourcesWon[1], resourcesWon[2]);
      // If the city is owned by someone, we need to tell them about the attack
      if (city.getOwner() != "") {
        API.GetUserData(
          new List<string> { "User" },
          result => {
            if (result.Data.ContainsKey("User"))user = JsonConvert.DeserializeObject<User>(result.Data["User"].Value);
            userDataReceived = true;
          },
          city.getOwner()
        );
        // Wait until that info is received
        while (!userDataReceived) {
          await Task.Delay(10);
        }
        // Register the attack and update user data
        user.registerAttack(new AttackOutcome('p', 'v', controller.getUser().getUsername(), city.getName(), resourcesWon));
        PlayFabAdminAPI.UpdateUserData(new UpdateUserDataRequest() {
          PlayFabId = city.getOwner(), Data = new Dictionary<string, string>() {
            {
              "User",
              JsonConvert.SerializeObject(user, new JsonSerializerSettings {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
              })
            }
          }, Permission = UserDataPermission.Public
        }, result => {
          Debug.Log("Enemy data updated successfully.");
        }, e => API.OnPlayFabError(e));
      }
    } else { // User defeat
      outcomeMessage = Language.Field["ATK_DEF_SAVED"];
    }
    // Show outcome message
    ui.showPopupMessage(outcomeMessage);
  }

  int[] computeResourceWon(int[] res, int userStrength, int enemyStrength) {
    double scalar = 0.2 * (userStrength / enemyStrength) + 0.2;
    if (scalar < 0.2)scalar = 0.2;
    if (scalar > 1)scalar = 1;
    return new int[3] {
      (int)(res[0] * scalar), (int)(res[1] * scalar), (int)(res[2] * scalar)
    };
  }

  //*****************************************************************
  // Add some randomness to the outcome of an attack
  //*****************************************************************
  bool getRandomOutcome(int userStrength, int enemyStrength) {
    // This represents the likelihood of a victory out of 10
    int coefficient = 4;
    switch ((double)userStrength / enemyStrength) {
      case double n when(n >= 5):
        coefficient = 8;
        break;
      case double n when(n >= 1.5):
        coefficient = 6;
        break;
      case double n when(n >= 0.8):
        coefficient = 4;
        break;
      case double n when(n >= 0.4):
        coefficient = 2;
        break;
      default:
        coefficient = 0;
        break;
    }
    System.Random rnd = new System.Random();
    return coefficient >= rnd.Next(10);
  }

  //*****************************************************************
  // HANDLE clicks or taps on hideouts
  //*****************************************************************
  void onHideoutClick(Vector3 position) {
    Vector2 position2d = new Vector2(position.x, position.y);
    RaycastHit2D raycastHit = Physics2D.Raycast(position2d, Vector2.zero);
    // Return if the click is on the UI
    if (IsPointerOverUIObject())return;
    if (raycastHit) {
      // Clouds should block the click
      // Debug.Log(raycastHit.collider.gameObject.layer);
      // if (raycastHit.collider.gameObject.layer == 11)return;
      string hideoutName = raycastHit.collider.name;
      // Check if the click is on a hideout
      if (hideoutName.Split('_')[0] == "hideout") {
        selectedTarget = hideoutName;
        if (hideoutName.Split('_')[2] == API.playFabId) {
          // Do this, if the hideout is the own player's one
          ui.showOwnHideoutPopup();
        } else {
          // Otherwise show information about it
          ui.showHideoutPopup();
          API.GetUserData(
            new List<string> {
              "User",
              "Village"
            },
            result => mapController.showHideoutInfo(
              result.Data.ContainsKey("User") ? result.Data["User"].Value : null,
              result.Data.ContainsKey("Village") ? result.Data["Village"].Value : null),
            hideoutName.Split('_')[2]
          );
        }
      } else if (hideoutName.Split('_')[0] == "city") {
        onCityClick(position);
      }
    }
  }

  private bool IsPointerOverUIObject() {
    PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
    eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
    List<RaycastResult> results = new List<RaycastResult>();
    EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
    return results.Count > 0 || EventSystem.current.currentSelectedGameObject != null;
  }

  //*****************************************************************
  // HANDLE clicks or taps on hideouts
  //*****************************************************************
  void onCityClick(Vector3 position) {
    Vector2 position2d = new Vector2(position.x, position.y);
    RaycastHit2D raycastHit = Physics2D.Raycast(position2d, Vector2.zero);
    if (raycastHit) {
      string cityName = raycastHit.collider.name;
      // Check if the click is on a city
      if (cityName.Split('_')[0] == "city") {
        selectedTarget = cityName;
        int cityNo = Int32.Parse(cityName.Split('_')[1]);
        mapController.showCityInfo(controller.getMap().getCities()[cityNo]);
      }
    }
  }

  //*****************************************************************
  // GET the selected target position towards which a ship will navigate
  //*****************************************************************
  Transform getNavigationTarget(int i) {
    if (selectedTarget == null) {
      Ship ship = controller.getUser().getVillage().getShip(i);
      return GameObject.Find(ship.getCurrentJourney().getDestinationName()).transform;
    } else {
      return GameObject.Find(selectedTarget).transform;
    }
  }

  //*****************************************************************
  // START the navigation of a ship by setting the target
  //*****************************************************************
  void startNavigation(int ship) {
    destinations[ship].target = getNavigationTarget(ship);
    destinationReached[ship] = false;
  }

  //*****************************************************************
  // SET which ship is going to attack and start the navigation
  //*****************************************************************
  public async void spawnShip(int shipNumber) {
    Ship ship = controller.getUser().getVillage().getShip(shipNumber);
    Vector3 currentShipPosition = ship.getCurrentPosition();
    // Check if the ship is undergoing upgrading and can't navigate
    if (ship.getCompletionTime() > DateTime.UtcNow) {
      ui.showPopupMessage(Language.Field["UPGRADING"]);
      return;
    }
    // If the ship has arrived, not spawn it but show marker
    Transform destination = getNavigationTarget(shipNumber);
    // Here I check the actual distance between ship and destination, if smaller than 0.05 the ship is arrived
    if (Mathf.Abs(destination.position.x - currentShipPosition.x) <= 0.05f && Mathf.Abs(destination.position.y - currentShipPosition.y) <= 0.05f) {
      string latestDestination = ship.getCurrentJourney().getDestinationName();
      addShipMarker(destination, shipNumber);
      ship.finishJourney(destination.position);
      destinationReached[shipNumber] = true;
      //// TODO: Need to check for a case where two ships arrive almost simultaneously, 
      //// as it stands the second one would overwrite the first
      // Generate attack outcome after 1s, so the page loads properly in the meantime
      await Task.Delay(2000);
      if (latestDestination.Split('_')[0] == "hideout") {
        if (latestDestination.Split('_')[2] != API.playFabId) {
          generatePlunderOutcome();
        }
      } else if (latestDestination.Split('_')[0] == "city") {
        int cityNo = Int32.Parse(latestDestination.Split('_')[1]);
        ui.showAttackOptions(CityNames.getCity(cityNo));
      }
      return;
    }
    // Create ShipJourney object and add it to the ship
    if (selectedTarget != null) {
      ShipJourney journey = new ShipJourney(
        currentShipPosition,
        getNavigationTarget(shipNumber).position,
        DateTime.Now
      );
      journey.setDestinationName(selectedTarget);
      ship.startJourney(journey);
    }
    // Spawn that ship onto the map only if it's not been spawn already
    if (shipsSpawned[shipNumber] == null) {
      int prefabNumber = ship.getLevel() < 7 ? ship.getLevel() : 6;
      shipsSpawned[shipNumber] = (GameObject)Instantiate(
        (GameObject)Resources.Load("Prefabs/Ship" + prefabNumber, typeof(GameObject)),
        getNearestSeaPosition(currentShipPosition, getNavigationTarget(shipNumber).position),
        Quaternion.identity
      );
      paths[shipNumber] = shipsSpawned[shipNumber].GetComponent<ShipPath>();
      destinations[shipNumber] = shipsSpawned[shipNumber].GetComponent<AIDestinationSetter>();
      // Spawn a timer that shows how long until the destination
      Vector3 timerPosition = shipsSpawned[shipNumber].transform.position;
      timersSpawned[shipNumber] = (GameObject)Instantiate(
        timerPrefab,
        worldSpaceUi.transform,
        false
      );
      timersSpawned[shipNumber].transform.position = new Vector3(timerPosition.x, timerPosition.y + 0.6f, 0.0f);
      // Show timer on top of other ui and clouds
      Canvas canvas = timersSpawned[shipNumber].AddComponent(typeof(Canvas))as Canvas;
      canvas.overrideSorting = true;
      canvas.sortingOrder = 101;
      // Add speed up function 
      Button speedUpButton = timersSpawned[shipNumber].transform.Find("SpeedUp").GetComponent<Button>();
      int timeLeft = (int)(paths[shipNumber].remainingDistance / paths[shipNumber].maxSpeed);
      int speedUpCost = (int)Math.Pow(Math.Pow(timeLeft / 60, 2), (double)1 / 3);
      speedUpButton.onClick.AddListener(() => {
        ui.showSpeedUpPopup(shipNumber);
      });
      updateTimers();
      // Remove ship marker
      Destroy(shipMarkers[shipNumber]);
    }
    // The ship starts the navigation towards the destination specified by ShipJourney
    startNavigation(shipNumber);
    showPath(shipNumber);
  }

  public int getSpeedUpCost(int shipNumber) {
    int timeLeft = (int)(paths[shipNumber].remainingDistance / paths[shipNumber].maxSpeed);
    int cost = (int)Math.Pow(Math.Pow(timeLeft / 60, 2), (double)1 / 3);
    return cost < 1 ? 1 : cost;
  }

  public void speedUpShip(int shipNumber) {
    paths[shipNumber].maxSpeed = 2;
  }

  //*****************************************************************
  // RETURNS the closest position that happens to be on the sea, from any given position
  //*****************************************************************
  Vector3 getNearestSeaPosition(Vector3 startPos, Vector3 endPos) {
    Vector3 direction = (startPos - endPos).normalized;
    Vector3 pos = startPos - (direction / 4.0f);
    NNInfo nn = AstarPath.active.GetNearest(pos, NNConstraint.Default);
    if (nn.node != null) {
      return (Vector3)nn.node.position;
    } else {
      return pos;
    }
  }

  //*****************************************************************
  // DRAW the ship navigation path on the scene
  //*****************************************************************
  async void showPath(int shipNumber) {
    // Create a new LineRenderer, which is the object that takes care of drawing the path
    if (lineRenderers[shipNumber] != null) {
      Destroy(lineRenderers[shipNumber].gameObject);
      paths[shipNumber].resetPath();
    }
    GameObject lineRendererSpawned = (GameObject)Instantiate(lineRendererPrefab, Vector3.zero, Quaternion.identity);
    lineRenderers[shipNumber] = lineRendererSpawned.GetComponent<LineRenderer>();
    // Wait until the path has been calculated
    List<Vector3> path = paths[shipNumber].getPath();
    while (path == null) {
      await Task.Delay(10);
      path = paths[shipNumber].getPath();
    }
    // Add information about the path to the ShipJourney object
    ShipJourney journey = controller.getUser().getVillage().getShip(shipNumber).getCurrentJourney();
    journey.setPath(path);
    journey.setDuration((int)(paths[shipNumber].remainingDistance / paths[shipNumber].maxSpeed));
    // Then use the LineRenderer to show the path
    lineRenderers[shipNumber].positionCount = path.Count;
    for (int i = 0; i < path.Count; i++) {
      lineRenderers[shipNumber].SetPosition(i, path[i]);
    }
  }

  //*****************************************************************
  // MARK on the map that a ship is parked at a given place
  //*****************************************************************
  void addShipMarker(Transform place, int shipNumber) {
    shipMarkers[shipNumber] = (GameObject)Instantiate(
      shipMarkerIcon,
      worldSpaceUi.transform,
      false
    );
    // Position of the marker depends on whether there are other ships already in that city
    int shipsInPlace = 0;
    foreach (Ship ship in controller.getUser().getVillage().getShips()) {
      if (ship != null && ship.getSlot() != shipNumber && ship.getCurrentPosition() == place.position)shipsInPlace++;
    }
    shipMarkers[shipNumber].transform.position = new Vector3(place.position.x + 0.35f + (0.20f * (shipsInPlace)), place.position.y - 0.25f, 0.0f);
  }

  //*****************************************************************
  // UPGRADE a city you conquered
  //*****************************************************************
  public void upgradeCity() {
    // Get the city object
    int cityNumber = Int32.Parse(selectedTarget.Split('_')[1]);
    City city = controller.getMap().getCity(cityNumber);
    // Get the upgrade cost
    int[] upgradeCost = city.getUpgradeCost();
    int[] userResources = controller.getUser().getResources();
    int[] remainingResources = new int[3];
    // Check if user can afford
    for (int i = 0; i < 3; i++) { remainingResources[i] = userResources[i] - upgradeCost[i]; }
    if (remainingResources.All(x => x >= 0)) {
      // If yes, increase level of city and remove resources from player
      controller.getMap().upgradeCity(cityNumber);
      controller.getUser().setResources(remainingResources);
    } else {
      // Otherwise show an error message
      ui.showPopupMessage(Language.Field["NOT_RESOURCES"]);
    }
  }
}