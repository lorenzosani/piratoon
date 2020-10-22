using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pathfinding;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapController : MonoBehaviour {
  ControllerScript controller;

  public MapUI ui;
  public GameObject worldSpaceUi;
  public GameObject hideoutsParent;

  AIPath path;
  AIDestinationSetter destination;
  public GameObject ship;

  void Start() {
    ui.showLoadingScreen();
    controller = GameObject.Find("GameController").GetComponent<ControllerScript>();

    populateMap();
    moveCameraOverHideout(controller.getUser().getVillage().getPosition());
    ui.showLoadingScreen(false);
    Camera.main.GetComponent<PanAndZoom>().Zoom(2, 5);
  }

  void Update() {
    path = ship.GetComponent<AIPath>();
    destination = ship.GetComponent<AIDestinationSetter>();
    detectDirection();
    detectClick();
  }

  //*****************************************************************
  // CHANGE the ship orientation based on its navigation direction
  //*****************************************************************
  void detectDirection() {
    // TODO: Improve to have 8 directions
    // TODO: for each direction show a different sprite
    if (path.desiredVelocity.x >= 0.01f) {
      ship.transform.localScale = new Vector3(-1f, 1f, 1f);
    } else if (path.desiredVelocity.x <= -0.01f) {
      ship.transform.localScale = new Vector3(1f, 1f, 1f);
    }
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
  // HANDLE clicks or taps on hideouts
  //*****************************************************************
  void onHideoutClick(Vector3 position) {
    Vector2 position2d = new Vector2(position.x, position.y);
    RaycastHit2D raycastHit = Physics2D.Raycast(position2d, Vector2.zero);
    if (raycastHit) {
      string hideoutName = raycastHit.collider.name;
      // Check if the click is on a hideout
      if (hideoutName.Split('_')[0] == "hideout") {
        if (hideoutName.Split('_')[2] == API.playFabId) { // If the hideout is the player's one, open it
          close();
        } else { // Otherwise show information about it
          ui.showHideoutPopup();
          API.GetUserData(
            new List<string> {
              "User",
              "Village"
            },
            result => showHideoutInfo(
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

  //*****************************************************************
  // HANDLE clicks or taps on hideouts
  //*****************************************************************
  public void onCityClick(Vector3 position) {
    Vector2 position2d = new Vector2(position.x, position.y);
    RaycastHit2D raycastHit = Physics2D.Raycast(position2d, Vector2.zero);
    if (raycastHit) {

      string cityName = raycastHit.collider.name;
      Debug.Log("Raycast hit " + cityName);
      if (cityName.Split('_')[0] == "city") { // Check if the click is on a city
        Debug.Log("Set target");
        destination.target = raycastHit.transform; // If yes, set target to the pathfinder
      }
    }
  }

  //*****************************************************************
  // SHOW information about a hideout
  //*****************************************************************
  void showHideoutInfo(string userData, string villageData) {
    User user = JsonConvert.DeserializeObject<User>(userData);
    Village village = JsonConvert.DeserializeObject<Village>(villageData);
    ui.populateHideoutPopup(
      user.getUsername(),
      user.getLevel(),
      user.getResources(),
      village.getStrength()
    );
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
}