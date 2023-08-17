using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockOutlineBeh : MonoBehaviour
{
   public bool isCollidingWithPlayer;

    void OnTriggerEnter(Collider other){
        if(other.gameObject.tag=="Player"){
            isCollidingWithPlayer=true;

        }
    }
    void OnTriggerExit(Collider other){
        isCollidingWithPlayer=false;
    }
}
