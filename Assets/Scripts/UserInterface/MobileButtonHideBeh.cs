using UnityEngine;

public class MobileButtonHideBeh : MonoBehaviour
{
    public static bool isHidingButton=true;
  public static RuntimePlatform platform{get{return WorldManager.platform;}}
    void Start()
    {
   //     isHidingButton=false;
        if((platform==RuntimePlatform.Android||platform==RuntimePlatform.IPhonePlayer)||isHidingButton==false){
            
        }else{
            gameObject.SetActive(false);
        }
    }

 
}
