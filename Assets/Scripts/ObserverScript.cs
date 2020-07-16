using UnityEngine;
using System;
using System.Collections;

public class ObserverScript : MonoBehaviour
{
  public void update(){
    Authenticate.SetUserData();
  }
}