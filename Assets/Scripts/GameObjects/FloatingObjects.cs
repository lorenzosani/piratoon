using System;
using System.Collections;
using System.Globalization;
using UnityEngine;

public class FloatingObjects : MonoBehaviour {

  GameObject[] floatingGameObjects;
  ControllerScript controller;

  Vector3[] possiblePositions = new Vector3[23] {
    new Vector3(-3.23f, -6.02f, 0),
    new Vector3(-3.37f, -8.14f, 0),
    new Vector3(0.37f, -5.35f, 0),
    new Vector3(0.66f, -7.45f, 0),
    new Vector3(-4.19f, -9.34f, 0),
    new Vector3(-0.81f, -5.3f, 0),
    new Vector3(3.21f, -7.48f, 0),
    new Vector3(-2.08f, -5.29f, 0),
    new Vector3(3.1f, -6.2f, 0),
    new Vector3(-1.29f, -7.09f, 0),
    new Vector3(-0.47f, -7.84f, 0),
    new Vector3(1.56f, -5.66f, 0),
    new Vector3(0.74f, -6.11f, 0),
    new Vector3(-2.65f, -9.1f, 0),
    new Vector3(-0.75f, -8.75f, 0),
    new Vector3(-4.25f, -7f, 0),
    new Vector3(-1.86f, -8.14f, 0),
    new Vector3(-0.53f, -6.12f, 0),
    new Vector3(1.98f, -7.09f, 0),
    new Vector3(-3.05f, -6.89f, 0),
    new Vector3(-1.37f, -9.67f, 0),
    new Vector3(2.31f, -8.06f, 0),
    new Vector3(-1.96f, -6.11f, 0)
  };

  void Start() {
    floatingGameObjects = new GameObject[transform.childCount];
    for (int i = 0; i < transform.childCount; i++) {
      floatingGameObjects[i] = transform.GetChild(i).gameObject;
    }
    controller = GameObject.Find("GameController").GetComponent<ControllerScript>();
    InvokeRepeating("checkIfSpawn", 10.0f, 30.0f);
  }

  // Check if there any floating objects ready to be spawned
  public void checkIfSpawn() {
    FloatingObject[] objects = controller.getUser().getVillage().getFloatingObjects();
    //Debug.Log("CHECKING IF SPAWNABLE AT TIME:" + DateTime.Now);
    for (int i = 0; i < objects.Length; i++) {
      //Debug.Log(objects[i].getId() + " : " + objects[i].getTime());
      if (DateTime.Compare(objects[i].getTime(), DateTime.Now) < 0) {
        spawn(objects[i]);
      }
    }
  }

  // Spawn a floating object on the scene
  public void spawn(FloatingObject obj) {
    GameObject floatingObj = floatingGameObjects[obj.getId()];
    floatingObj.transform.position = obj.getPosition();
    floatingObj.SetActive(true);
  }

  // This runs when the user collects a floating object
  public void collect(int id) {
    // Check how many resources the object is worth
    int[] resources;
    if (id < 4) {
      resources = new int[3] {
        10,
        0,
        0
      };
    } else if (id < 6) {
      resources = new int[3] {
        20,
        20,
        0
      };
    } else {
      resources = new int[3] {
        30,
        30,
        5
      };
    }
    // Give the collected resources to the user
    int[] spaceLeft = controller.getUser().getStorageSpaceLeft();
    for (int i = 0; i < 3; i++) {
      if (resources[i] > 0) {
        if (spaceLeft[i] == 0) {
          controller.getUI().showPopupMessage(Language.Field["STORAGE_SPACE"]);
        } else {
          controller.getUser().increaseResource(i, resources[i]);
        }
      }
    }
    // Hide the collected object
    GameObject floatingObj = floatingGameObjects[id];
    floatingObj.SetActive(false);
    // Generate a new object to replace the collected one in the queue
    int maxMinutes = (int)((TimeSpan.FromHours(16) - TimeSpan.FromHours(0)).TotalMinutes);
    System.Random rnd = new System.Random();
    TimeSpan time = TimeSpan.FromHours(0).Add(TimeSpan.FromMinutes(rnd.Next(maxMinutes)));
    FloatingObject newObject = new FloatingObject(id, DateTime.Now + time, possiblePositions[rnd.Next(possiblePositions.Length)]);
    // Add the new object to the queue
    controller.getUser().getVillage().replaceFloatingObject(id, newObject);
  }
}