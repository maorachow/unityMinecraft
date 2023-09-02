using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileButtonHideBeh : MonoBehaviour
{
  public static RuntimePlatform platform{get{return EntityBeh.platform;}set{platform=EntityBeh.platform;}}
    void Start()
    {
        if(platform==RuntimePlatform.Android||platform==RuntimePlatform.IPhonePlayer){
            
        }else{
            gameObject.SetActive(false);
        }
    }

 
}
