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

public static class Mapmaking {
  static bool stopped = false;

  static string fetchedMapId;
  static int fetchedPosition;
  static List<string> serverData = new List<string>() {
    "currentMap",
    "availablePosition"
  };

  static ControllerScript controller;
  public static void RegisterScripts(ControllerScript c) {
    controller = c;
  }

  //*****************************************************************
  // GET the most recent map id and available position
  //*****************************************************************
  public static void Start() {
    Debug.Log("********START ADD TO MAP***********"); // TO BE REMOVED

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
        Debug.Log("Fetched position: " + fetchedPosition);
        if (fetchedPosition > 33) {
          Debug.Log("Position is bigger than 33");
          CreateNewMap();
        } else {
          AddToMap(fetchedMapId, fetchedPosition);
        }
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
    int position = 0;

    AddToMap(mapId, position, true);
  }

  //*****************************************************************
  // ADD player to a playfab group holding a info about a map
  //*****************************************************************
  static void AddToMap(string mapId, int position, bool newMap = false) {
    Debug.Log("********ADD TO MAP " + mapId + "***********"); // TO BE REMOVED
    if (stopped)return;

    if (newMap) {
      // Create a new group
      PlayFabGroupsAPI.CreateGroup(new CreateGroupRequest {
        GroupName = mapId
      }, result => {
        PlayFabClientAPI.ExecuteCloudScript(new PlayFab.ClientModels.ExecuteCloudScriptRequest() {
          FunctionName = "addToGroup", FunctionParameter = new string[] {
            result.Group.Id,
              API.entityId,
              position.ToString(),
              controller.getUser().getUsername(),
              API.playFabId
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
          Debug.Log("Fetched position: " + Position);
          if (Position > 33) {
            CreateNewMap();
          } else {
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
    } else {
      // Add to existing group
      PlayFabGroupsAPI.GetGroup(new GetGroupRequest {
        GroupName = mapId
      }, result => {
        PlayFabClientAPI.ExecuteCloudScript(new PlayFab.ClientModels.ExecuteCloudScriptRequest() {
          FunctionName = "addToGroup", FunctionParameter = new string[] {
            result.Group.Id,
              API.entityId,
              position.ToString(),
              controller.getUser().getUsername(),
              API.playFabId
          }
        }, r => {
          if (r.Error != null) {
            Debug.Log(r.Error.StackTrace);
            Stop();
          }
          Debug.Log(r.FunctionResult.ToString());
          int Position = Int32.Parse(
            r.FunctionResult.ToString().Split(':')[1].Trim(new char[] {
              ' ',
              '}'
            })
          );
          if (Position > 33) {
            CreateNewMap();
          } else {
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
  }

  public static void Stop(bool val = true) {
    stopped = val;
  }
}