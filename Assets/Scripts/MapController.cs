using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapController : MonoBehaviour {
  ControllerScript controller;

  public MapUI ui;
  public GameObject worldSpaceUi;
  public GameObject hideoutsParent;

  void Start() {
    ui.showLoadingScreen();
    controller = GameObject.Find("GameController").GetComponent<ControllerScript>();

    populateMap();
    moveCameraOverHideout(controller.getUser().getVillage().getPosition());
    ui.showLoadingScreen(false);
    Camera.main.GetComponent<PanAndZoom>().Zoom(2, 5);
  }

  void Update() {
    if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended) {
      onHideoutClick(Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position));
    } else if (Input.GetMouseButtonUp(0)) {
      onHideoutClick(Camera.main.ScreenToWorldPoint(Input.mousePosition));
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
  // DETECT clicks or taps on hideouts
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