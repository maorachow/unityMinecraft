using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockOutlineBeh : MonoBehaviour
{
    public bool isCollidingWithPlayer;
    public bool isCollidingWithEntity;
    void OnTriggerEnter(Collider other){
        if(other.gameObject.tag=="Player"){
            isCollidingWithPlayer=true;

        }
        if(other.gameObject.tag=="Entity"){
            isCollidingWithEntity=true;
        }
    }
    void OnDisable(){
        isCollidingWithPlayer=false;
        isCollidingWithEntity=false;
    }
    void OnTriggerExit(Collider other){
        isCollidingWithPlayer=false;
        isCollidingWithEntity=false;
    }
}
