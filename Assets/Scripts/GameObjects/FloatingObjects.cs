using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using UnityEngine;

public class FloatingObjects : MonoBehaviour {

  GameObject[] floatingGameObjects;
  ControllerScript controller;

  void Start() {
    floatingGameObjects = new GameObject[transform.childCount];
    for (int i = 0; i < transform.childCount; i++) {
      floatingGameObjects[i] = transform.GetChild(i).gameObject;
    }
    controller = GameObject.Find("GameController").GetComponent<ControllerScript>();
    InvokeRepeating("checkIfSpawn", 10.0f, 30.0f);
  }

  // Check if there any floating objects ready to be spawned
  void checkIfSpawn() {
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
    int[, ] resPerType = new int[4, 3] { { 20, 0, 0 }, { 30, 30, 0 }, { 50, 50, 5 }, { 0, 20, 0 } };
    int[] resources = new int[3] { resPerType[id, 0], resPerType[id, 1], resPerType[id, 2] };
    // Check if the user has enough storage space, if so give him the resources
    int[] spaceLeft = controller.getUser().getStorageSpaceLeft();
    bool giveResources = false;
    for (int i = 0; i < 3; i++) {
      if (resources[i] > 0 && spaceLeft[i] > 0) {
        giveResources = true;
        controller.getUser().increaseResource(i, resources[i]);
      }
    }
    // If not, show a popup message
    if (!giveResources) {
      controller.getUI().showPopupMessage(Language.Field["STORAGE_SPACE"]);
    }
    // Hide the collected object
    GameObject floatingObj = floatingGameObjects[id];
    floatingObj.SetActive(false);
    // Generate a new object to replace the collected one in the queue
    System.Random rnd = new System.Random();
    TimeSpan time = TimeSpan.FromHours(0).Add(TimeSpan.FromMinutes(rnd.Next(30)));
    // Check if other objects have already this position
    int randomPos = rnd.Next(23);
    int[] positionsUsed = controller.getUser().getVillage().getFloatingObjects().Select(o => o.getPositionId()).ToArray();
    do { randomPos = rnd.Next(23); } while (positionsUsed.Contains(randomPos));
    FloatingObject newObject = new FloatingObject(id, DateTime.Now + time, rnd.Next(23));
    // Add the new object to the queue
    controller.getUser().getVillage().replaceFloatingObject(id, newObject);
  }
}