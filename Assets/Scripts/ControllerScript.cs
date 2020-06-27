using UnityEngine;
using System;
using System.Collections.Generic;

public class ControllerScript : MonoBehaviour
{
  private User user;

  void Start()
  {
    // TODO: Add unique IDs generator
    user = new User(0, 0, new Village(Vector3.zero));
  }

  public User getUser()
  {
    return user;
  }

  public void produceWood(){
    user.increaseResource(0, 1);
  } 
}