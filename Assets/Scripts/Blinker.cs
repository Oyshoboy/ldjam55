using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blinker : MonoBehaviour
{
   public GameObject objectToBlink;
   public float blinkInterval = 1f;

   private void Update()
   {
      HandleBlink();
   }

   private void HandleBlink()
   {
      if (objectToBlink == null) return;
      objectToBlink.SetActive(Mathf.PingPong(Time.time, blinkInterval) < blinkInterval / 2);
   }
}
