using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockOutlineBeh : MonoBehaviour
{
    public bool isCollidingWithPlayer;
    public bool isCollidingWithEntity;
    void OnTriggerStay(Collider other){
        if(other.gameObject.tag=="Player"){
            isCollidingWithPlayer=true;

        }else{
            isCollidingWithPlayer=false;
        }
        if(other.gameObject.tag=="Entity"){
            isCollidingWithEntity=true;
        }else{
            isCollidingWithEntity=false;
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
