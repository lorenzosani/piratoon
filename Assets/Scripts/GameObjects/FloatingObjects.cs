using UnityEngine;
using System;
using System.Collections;
using System.Globalization;

public class FloatingObjects : MonoBehaviour {

  GameObject[] floatingGameObjects;
  ControllerScript controller;

  void Start(){
    floatingGameObjects = new GameObject[transform.childCount];
    for(int i=0; i<transform.childCount;i++){
      floatingGameObjects[i] = transform.GetChild(i).gameObject;
    }
    controller = GameObject.Find("GameController").GetComponent<ControllerScript>();
    InvokeRepeating("checkIfSpawn", 10.0f, 30.0f);
  }

  // Check if there any floating objects ready to be spawned
  public void checkIfSpawn(){
    FloatingObject[] objects = controller.getUser().getVillage().getFloatingObjects();
    //Debug.Log("CHECKING IF SPAWNABLE AT TIME:" + DateTime.Now);
    for (int i=0; i<objects.Length; i++) {
      //Debug.Log(objects[i].getId() + " : " + objects[i].getTime());
      if (DateTime.Compare(objects[i].getTime(), DateTime.Now) < 0) {
        spawn(objects[i]);
      }
    }
  }

  // Spawn a floating object on the scene
  public void spawn(FloatingObject obj){
    GameObject floatingObj = floatingGameObjects[obj.getId()];
    floatingObj.transform.position = obj.getPosition();
    floatingObj.SetActive(true);
  }

  // This runs when the user collects a floating object
  public void collect(int id){
    // Check how many resources the object is worth
    int[] resources;
    if (id < 4) { 
      resources = new int[3] {10, 0, 0};
    } else if (id < 6) {
      resources = new int[3] {30, 0, 0};
    } else {
      resources = new int[3] {40, 0, 5};
    }
    // Give the collected resources to the user
    int[] spaceLeft = controller.getUser().getStorageSpaceLeft();
    for(int i=0; i<3; i++){
      if (resources[i] > 0){
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
    float x = float.Parse(rnd.Next(-3,3) + "." + rnd.Next(0,99), CultureInfo.InvariantCulture.NumberFormat);
    float y = float.Parse(rnd.Next(-7,-5) + "." + rnd.Next(0,99), CultureInfo.InvariantCulture.NumberFormat);
    FloatingObject newObject = new FloatingObject(id, DateTime.Now + time, new Vector3(x, y, 0));
    // Add the new object to the queue
    controller.getUser().getVillage().replaceFloatingObject(id, newObject);
  }
}