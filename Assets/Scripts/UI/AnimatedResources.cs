using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AnimatedResources : MonoBehaviour {
  public Transform UITarget;

  Transform target;
  ParticleSystem particleSystem;

  void Start() {
    particleSystem = GetComponent<ParticleSystem>();
    target = new GameObject().transform;
  }

  void LateUpdate() {
    target.position = Camera.main.ScreenToWorldPoint(UITarget.position);
    transform.LookAt(target);
    particleSystem.startLifetime = (Vector3.Distance(target.position, transform.position) / particleSystem.startSpeed);
  }

  public void Animate(int resourcesToCollect) {
    ParticleSystem.EmissionModule em = particleSystem.emission;
    em.rateOverTime = (float)resourcesToCollect;
    if (!particleSystem.isPlaying) {
      particleSystem.Play();
    }
  }
}