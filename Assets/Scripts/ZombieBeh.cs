using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieBeh : MonoBehaviour
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
    Vector2 curpos;
    Vector2 lastpos;
    public float attackCD=1.2f;
    public bool isJumping=false;
    public float entitySpeed;
    public void Start () {
        targetPosition=GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        headTransform=transform.GetChild(0).GetChild(1);
        entityFacingPos=transform.rotation.eulerAngles;
    
        cc = GetComponent<CharacterController>();
        am=GetComponent<Animator>();
    
    }
       public void Attack(){
        if(attackCD<=0f){
               am.SetBool("attack",true);
        attackCD=1.2f;
        Invoke("CancelAttack",0.36f);  
        }
   
    }
    public void CancelAttack(){
        am.SetBool("attack",false);
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
		curpos = new Vector2(gameObject.transform.position.x,gameObject.transform.position.z);
		float _speed = (Vector3.Magnitude(curpos - lastpos) / Time.deltaTime/4f);
		lastpos = curpos;
        Debug.Log(_speed);
		return _speed;
	}




    public void Update () {
        entitySpeed=Speed();
        if(transform.position.y<-40f){
            Destroy(gameObject);
        }
          if(attackCD>0f){
         attackCD-=Time.deltaTime;
            }
 
     
       

    
         Vector3 targetDir;
    
         targetDir = targetPosition.position;
                am.SetFloat("speed",entitySpeed);
        transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.Euler(new Vector3(0f,headTransform.eulerAngles.y,0f)),5f*Time.deltaTime);
        ChangeHeadPos(targetDir);
        entityVec.x=1f;
      
        if(Vector3.Magnitude(transform.position - targetDir)<1f){
            Attack();
          
        }else{
             if(entitySpeed<=0.18f){
                Jump();
            }
        if(cc.enabled==true){
        cc.Move((transform.forward*entityVec.x+transform.right*entityVec.z)*moveSpeed*Time.deltaTime);    
        }
        }
        cc.Move((new Vector3(0f,entityVec.y,0f))*moveSpeed*Time.deltaTime);
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

        
       // Vector3 velocity = dir * speed * 6;
      //  controller.SimpleMove(velocity);

    }
}
