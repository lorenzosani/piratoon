using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PlayFab;
using PlayFab.DataModels;
using PlayFab.GroupsModels;
using PlayFab.ServerModels;
using UnityEngine;
using UnityEngine.UI;

public static class Mapmaking {
  static bool stopped = false;

  static string fetchedMapId;
  static int fetchedPosition;
  static List<string> serverData = new List<string>() {
    "currentMap",
    "availablePosition"
  };

  static ControllerScript controller;
  static Ships ships;
  public static void RegisterScripts(ControllerScript c, Ships s) {
    controller = c;
    ships = s;
  }

  //*****************************************************************
  // GET the most recent map id and available position
  //*****************************************************************
  public static void Start() {
    Debug.Log("********START ADD TO MAP***********"); // TO BE REMOVED

    // Disable map button and tutorial button while mapmaking
    controller.getUI().mapButton.GetComponent<Button>().interactable = false;
    controller.getUI().tutorialMapButton.GetComponent<Button>().interactable = false;
    PlayFabServerAPI.GetTitleData(
      new GetTitleDataRequest {
        Keys = serverData
      },
      result => {
        if (!result.Data.Keys.Contains("currentMap")) {
          Debug.Log("No title data found");
          CreateNewMap();
          return;
        }
        fetchedMapId = result.Data["currentMap"];
        fetchedPosition = Int32.Parse(result.Data["availablePosition"]);
        Debug.Log("Fetched id: " + fetchedMapId);
        AddToMap(fetchedMapId);
      },
      error => {
        Debug.Log(error);
        Stop();
      }
    );
  }

  //*****************************************************************
  // CREATE a new map from scratch and add the player at position 0
  //*****************************************************************
  static void CreateNewMap() {
    Debug.Log("********CREATE MAP***********"); // TO BE REMOVED
    string mapId = Guid.NewGuid().ToString();

    AddToMap(mapId, true);
  }

  //*****************************************************************
  // ADD player to a playfab group holding a info about a map
  //*****************************************************************
  static void AddToMap(string mapId, bool newMap = false) {
    Debug.Log("********ADD TO MAP " + mapId + "***********"); // TO BE REMOVED
    if (stopped)return;

    string cities = JsonConvert.SerializeObject(
      new Map(mapId, new MapUser[0]).getCities(), new JsonSerializerSettings {
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
      });
    if (newMap) {
      // Create a new group
      PlayFabGroupsAPI.CreateGroup(new CreateGroupRequest {
        GroupName = mapId
      }, result => {
        PlayFabClientAPI.ExecuteCloudScript(new PlayFab.ClientModels.ExecuteCloudScriptRequest() {
          FunctionName = "addToGroup", FunctionParameter = new string[] {
            result.Group.Id, API.entityId, "0", controller.getUser().getUsername(), API.playFabId, cities
          }
        }, r => {
          if (r.Error != null) {
            Debug.Log(r.Error.StackTrace);
            Stop();
          }
          int Position = Int32.Parse(
            r.FunctionResult.ToString().Split(':')[1].Trim(new char[] {
              ' ',
              '}'
            })
          );
          if (Position == -1 || Position > MapPositions.get().Length) {
            Debug.Log("Position unavailable, creating new map");
            CreateNewMap();
          } else {
            Debug.Log("Adding player at position: " + Position);
            UpdateTitleData(mapId, Position, newMap);
          };
        }, e => {
          Debug.Log(e);
          Stop();
        });
      }, error => {
        Debug.Log(error);
        Stop();
      });
      // Create new cities data
      API.CreateNewCitiesOnServer();
    } else {
      // Add to existing group
      PlayFabGroupsAPI.GetGroup(new GetGroupRequest {
        GroupName = mapId
      }, result => {
        PlayFabClientAPI.ExecuteCloudScript(new PlayFab.ClientModels.ExecuteCloudScriptRequest() {
          FunctionName = "addToGroup", FunctionParameter = new string[] {
            result.Group.Id,
              API.entityId,
              "0",
              controller.getUser().getUsername(),
              API.playFabId,
              "none"
          }
        }, r => {
          if (r.Error != null) {
            Debug.Log(r.Error.StackTrace);
            Stop();
          }
          int Position = Int32.Parse(
            r.FunctionResult.ToString().Split(':')[1].Trim(new char[] {
              ' ',
              '}'
            })
          );
          if (Position == -1 || Position > MapPositions.get().Length) {
            Debug.Log("Position unavailable, creating new map");
            CreateNewMap();
          } else {
            Debug.Log("Adding player at position: " + Position);
            UpdateTitleData(mapId, Position, newMap);
          };
        }, e => {
          Debug.Log(e);
          Stop();
        });
      }, error => {
        Debug.Log(error);
        Stop();
      });
    }
  }

  //*****************************************************************
  // SET the title-wide data indicating the next available map and position on that map
  //*****************************************************************
  static void UpdateTitleData(string mapId, int position, bool newMap) {
    Debug.Log("********UPDATE TITLE DATA: " + mapId + ", at " + position + "***********"); // TO BE REMOVED
    if (stopped)return;

    PlayFabServerAPI.GetTitleData(
      new GetTitleDataRequest {
        Keys = serverData
      },
      result => {
        PlayFabServerAPI.SetTitleData(new SetTitleDataRequest {
            Key = serverData[0],
              Value = mapId
          },
          set_result_one => {
            PlayFabServerAPI.SetTitleData(new SetTitleDataRequest {
                Key = serverData[1], Value = (position + 1).ToString()
              },
              set_result_two => {
                Debug.Log("Title data updated");

              }, error_two => {
                Debug.Log("2: " + error_two);
              });
          }, error_one => {
            Debug.Log("1: " + error_one);
          });
        UpdateGameObjects(mapId, position);
      },
      error => {
        Debug.Log(error);
        Stop();
      }
    );
  }

  //*****************************************************************
  // SET the map and position data onto the local user and village objects
  //*****************************************************************
  static void UpdateGameObjects(string mapId, int position) {
    controller.getUser().setMapId(mapId);
    controller.getUser().getVillage().setPosition(position);
    // Create the initial ship for the user
    if (controller.getUser().getVillage().getShip(0) == null) {
      Ship ship = new Ship("My Ship", MapPositions.get(position), 0);
      ship.increaseLevel();
      ship.setBuilt(true);
      controller.getUser().getVillage().setShip(ship, 0);
      ships.populateShip(ship);
    }
    API.GetMapData(mapId);
    // Set map button and tutorial button clickable
    controller.getUI().mapButton.GetComponent<Button>().interactable = true;
    controller.getUI().tutorialMapButton.GetComponent<Button>().interactable = true;
  }

  public static void Stop(bool val = true) {
    stopped = val;
  }
}