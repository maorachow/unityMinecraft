using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreeperBeh : MonoBehaviour
{
    public Transform targetPosition;

    public static GameObject diedCreeperPrefab;
    public static bool isCreeperPrefabLoaded=false;
    public bool isCreeperDied=false;
    private CharacterController cc;
    public Animator am;
    public Transform headTransform;
    public float creeperHealth=20f;
      public Vector3 entityVec;
    public float moveSpeed=5f;
    public float gravity=-9.8f;
    public float entityY=0f;
    public float jumpHeight=2f;
    public Vector3 entityFacingPos;
    public float nextWaypointDistance =6f;
    public bool isPosInited=false;
    Vector2 curpos;
    Vector2 lastpos;
    public bool isJumping=false;
    public Vector3 entityMotionVec;
    public float entitySpeed;
    public void Start () {
        creeperHealth=20f;
        isCreeperDied=false;
        if(isCreeperPrefabLoaded==false){
                diedCreeperPrefab=Resources.Load<GameObject>("Prefabs/diedcreeper");
                isCreeperPrefabLoaded=true;
        }
        targetPosition=GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        headTransform=transform.GetChild(1);
        entityFacingPos=transform.rotation.eulerAngles;
    
        cc = GetComponent<CharacterController>();
        am=GetComponent<Animator>();
    
    }

     public void OnDisable(){
        entityMotionVec=Vector3.zero;
        isCreeperDied=false;
        creeperHealth=20f;
            isPosInited=false;
    }

    public void ApplyDamageAndKnockback(float damageAmount,Vector3 knockback){
       
        transform.GetChild(0).GetComponent<MeshRenderer>().material.color=Color.red;
         transform.GetChild(1).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.red;
          transform.GetChild(2).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.red;
           transform.GetChild(3).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.red;
           transform.GetChild(4).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.red;
           transform.GetChild(5).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.red;
        creeperHealth-=damageAmount;
        entityMotionVec=knockback;
        Invoke("InvokeRevertColor",0.2f);
    }
    void InvokeRevertColor(){
              transform.GetChild(0).GetComponent<MeshRenderer>().material.color=Color.white;
         transform.GetChild(1).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.white;
          transform.GetChild(2).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.white;
           transform.GetChild(3).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.white;
           transform.GetChild(4).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.white;
           transform.GetChild(5).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.white;
    }
    void CreeperDie(Vector3 knockback){
        isCreeperDied=true;
          Transform diedCreeperTrans=Instantiate(diedCreeperPrefab,transform.position,transform.rotation).GetComponent<Transform>();
              diedCreeperTrans.GetChild(0).position=transform.GetChild(0).position;
         diedCreeperTrans.GetChild(1).GetChild(0).position=transform.GetChild(1).GetChild(0).position;
          diedCreeperTrans.GetChild(2).GetChild(0).position=transform.GetChild(2).GetChild(0).position;
           diedCreeperTrans.GetChild(3).GetChild(0).position=transform.GetChild(3).GetChild(0).position;
           diedCreeperTrans.GetChild(4).GetChild(0).position=transform.GetChild(4).GetChild(0).position;
           diedCreeperTrans.GetChild(5).GetChild(0).position=transform.GetChild(5).GetChild(0).position;
               diedCreeperTrans.GetChild(0).rotation=transform.GetChild(0).rotation;
         diedCreeperTrans.GetChild(1).GetChild(0).rotation=transform.GetChild(1).GetChild(0).rotation;
          diedCreeperTrans.GetChild(2).GetChild(0).rotation=transform.GetChild(2).GetChild(0).rotation;
           diedCreeperTrans.GetChild(3).GetChild(0).rotation=transform.GetChild(3).GetChild(0).rotation;
           diedCreeperTrans.GetChild(4).GetChild(0).rotation=transform.GetChild(4).GetChild(0).rotation;
           diedCreeperTrans.GetChild(5).GetChild(0).rotation=transform.GetChild(5).GetChild(0).rotation;


           diedCreeperTrans.GetChild(0).GetComponent<Rigidbody>().velocity=knockback;
            diedCreeperTrans.GetChild(1).GetChild(0).GetComponent<Rigidbody>().velocity=knockback;
            diedCreeperTrans.GetChild(2).GetChild(0).GetComponent<Rigidbody>().velocity=knockback;
           diedCreeperTrans.GetChild(3).GetChild(0).GetComponent<Rigidbody>().velocity=knockback;
           diedCreeperTrans.GetChild(4).GetChild(0).GetComponent<Rigidbody>().velocity=knockback;
           diedCreeperTrans.GetChild(5).GetChild(0).GetComponent<Rigidbody>().velocity=knockback;
           Destroy(diedCreeperTrans.gameObject,10f);
           ObjectPools.creeperEntityPool.Release(gameObject);
    }

    public void InitPos(){
        Invoke("InvokeInitPos",0.1f);
    }
    public void InvokeInitPos(){
        isPosInited=true;
    }
    public void Jump(){
        isJumping=true;
    }
    public void ChangeHeadPos(Vector3 pos){
     //   headTransform.rotation=q;
     headTransform.LookAt(pos);
    }
 
    float Speed()
	{
    /*    if(PlayerMove.isPaused==true){
            return entitySpeed;
        }
		curpos =new Vector2(gameObject.transform.position.x,gameObject.transform.position.z);//当前点
		float _speed = (Vector3.Magnitude(curpos - lastpos) / Time.deltaTime/4f);//与上一个点做计算除去当前帧花的时间。
		lastpos = curpos;//把当前点保存下一次用*/
        if(GameUIBeh.isPaused==true){
            return entitySpeed;
        }else{
        return cc.velocity.magnitude;    
        }
		
	}

    public void Update () {
        entityMotionVec=Vector3.Lerp(entityMotionVec,Vector3.zero,Time.deltaTime*3f);
        if(creeperHealth<=0f&&isCreeperDied==false){
            CreeperDie(entityMotionVec);
        }
        if(!isPosInited){
            return;
        }
     
        if(entitySpeed<0.1f){
            Jump();
        }
   //      seeker.StartPath(transform.position, targetPosition.position, OnPathComplete);
        if(transform.position.y<-40f){
           ObjectPools.creeperEntityPool.Release(gameObject);
        }
            
      
         Vector3 targetDir;
    
         targetDir = targetPosition.position;
        
        
       
        if(cc.isGrounded!=true){
            if(!GetComponent<EntityBeh>().isInUnloadedChunks){
             entityY+=gravity*Time.deltaTime;   
            }
            
        }else{
            entityY=0f;
        }
        if(cc.isGrounded==true&&isJumping==true){
            entityY=jumpHeight;
            isJumping=false;
        }
        entityVec.y=entityY;
       // Quaternion targetRotation = Quaternion.LookRotation(targetDir);
       // entityFacingPos=targetRotation.eulerAngles;
        
        transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.Euler(new Vector3(0f,headTransform.eulerAngles.y,0f)),5f*Time.deltaTime);
        ChangeHeadPos(targetDir);
        entityVec.x=1f;
        if(cc.enabled==true){
        
         if(entityMotionVec.magnitude>0.7f){
                cc.Move(entityMotionVec*Time.deltaTime); 
            }else{
                 cc.Move((transform.forward*entityVec.x+transform.right*entityVec.z)*moveSpeed*Time.deltaTime+entityMotionVec*Time.deltaTime);
            }
            entitySpeed=Speed();
        am.SetFloat("speed",entitySpeed);
        cc.Move(new Vector3(0f,entityVec.y,0f)*5f*Time.deltaTime);    
        }
        
       // Vector3 velocity = dir * speed * 6;
      //  controller.SimpleMove(velocity);

    }
}
