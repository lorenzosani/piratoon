using UnityEngine;
using System;
using System.Collections;

public class ObserverScript : MonoBehaviour
{
  ControllerScript controller;

  void Start() {
    controller = GetComponent<ControllerScript>();
  }

  public void update(){
    controller.updateRemoteUserData();
  }
}