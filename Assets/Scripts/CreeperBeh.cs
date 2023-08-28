using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreeperBeh : MonoBehaviour
{
    public Transform targetPosition;


    private CharacterController cc;
    public Animator am;
    public Transform headTransform;

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

    public float entitySpeed;
    public void Start () {
        targetPosition=GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        headTransform=transform.GetChild(1);
        entityFacingPos=transform.rotation.eulerAngles;
    
        cc = GetComponent<CharacterController>();
        am=GetComponent<Animator>();
    
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
        if(PlayerMove.isPaused==true){
            return entitySpeed;
        }
		curpos =new Vector2(gameObject.transform.position.x,gameObject.transform.position.z);//当前点
		float _speed = (Vector3.Magnitude(curpos - lastpos) / Time.deltaTime/4f);//与上一个点做计算除去当前帧花的时间。
		lastpos = curpos;//把当前点保存下一次用
		return _speed;
	}

    public void Update () {
        if(!isPosInited){
            return;
        }
        entitySpeed=Speed();
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
        am.SetFloat("speed",entitySpeed);
        transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.Euler(new Vector3(0f,headTransform.eulerAngles.y,0f)),5f*Time.deltaTime);
        ChangeHeadPos(targetDir);
        entityVec.x=1f;
        if(cc.enabled==true){
        cc.Move((transform.forward*entityVec.x+transform.right*entityVec.z)*moveSpeed*Time.deltaTime+new Vector3(0f,entityVec.y,0f)*5f*Time.deltaTime);    
        }
        
       // Vector3 velocity = dir * speed * 6;
      //  controller.SimpleMove(velocity);

    }
}
