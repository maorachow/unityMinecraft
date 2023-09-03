using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieBeh : MonoBehaviour
{
    public Transform targetPosition;
    public static GameObject diedZombiePrefab;
    public bool isZombieDied=false;
    private CharacterController cc;
    public Animator am;
    public Transform headTransform;
    public float zombieHealth=20f;
    public Vector3 entityVec;
    public Vector3 entityMotionVec;
    public float moveSpeed=5f;
    public float gravity=-9.8f;
    public float entityY=0f;
    public float jumpHeight=2f;
    public Vector3 entityFacingPos;
    public bool isPosInited=false;
    public float attackCD=1.2f;
    public bool isJumping=false;
    public float entitySpeed;
    public static bool isZombiePrefabLoaded=false;

     public void ApplyDamageAndKnockback(float damageAmount,Vector3 knockback){
       
        transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.red;
         transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.red;
          transform.GetChild(0).GetChild(2).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.red;
           transform.GetChild(0).GetChild(3).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.red;
           transform.GetChild(0).GetChild(4).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.red;
           transform.GetChild(0).GetChild(5).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.red;
        zombieHealth-=damageAmount;
        entityMotionVec=knockback;
        Invoke("InvokeRevertColor",0.2f);
    }
    void InvokeRevertColor(){
         transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.white;
         transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.white;
          transform.GetChild(0).GetChild(2).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.white;
           transform.GetChild(0).GetChild(3).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.white;
           transform.GetChild(0).GetChild(4).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.white;
           transform.GetChild(0).GetChild(5).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.white;
    }
    public void Start () {
        if(isZombiePrefabLoaded==false){
         diedZombiePrefab=Resources.Load<GameObject>("Prefabs/diedzombie");
         isZombiePrefabLoaded=true;
        }
        
        targetPosition=GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        headTransform=transform.GetChild(0).GetChild(1);
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
    public void OnDisable(){
            entityMotionVec=Vector3.zero;
            isZombieDied=false;
            zombieHealth=20f;
            isPosInited=false;
    }
    
    public void ZombieDie(Vector3 knockback){
    
        isZombieDied=true;
            Transform diedZombieTrans=Instantiate(diedZombiePrefab,transform.position,transform.rotation).GetComponent<Transform>();

            diedZombieTrans.GetChild(0).GetChild(0).position=transform.GetChild(0).GetChild(0).position;
            diedZombieTrans.GetChild(0).GetChild(1).GetChild(0).position=transform.GetChild(0).GetChild(1).GetChild(0).position;
            diedZombieTrans.GetChild(0).GetChild(2).GetChild(0).position=transform.GetChild(0).GetChild(2).GetChild(0).position;
            diedZombieTrans.GetChild(0).GetChild(3).GetChild(0).position=transform.GetChild(0).GetChild(3).GetChild(0).position;
            diedZombieTrans.GetChild(0).GetChild(4).GetChild(0).position=transform.GetChild(0).GetChild(4).GetChild(0).position;
            diedZombieTrans.GetChild(0).GetChild(5).GetChild(0).position=transform.GetChild(0).GetChild(5).GetChild(0).position;
            diedZombieTrans.GetChild(0).GetChild(0).rotation=transform.GetChild(0).GetChild(0).rotation;
            diedZombieTrans.GetChild(0).GetChild(1).GetChild(0).rotation=transform.GetChild(0).GetChild(1).GetChild(0).rotation;
            diedZombieTrans.GetChild(0).GetChild(2).GetChild(0).rotation=transform.GetChild(0).GetChild(2).GetChild(0).rotation;
            diedZombieTrans.GetChild(0).GetChild(3).GetChild(0).rotation=transform.GetChild(0).GetChild(3).GetChild(0).rotation;
            diedZombieTrans.GetChild(0).GetChild(4).GetChild(0).rotation=transform.GetChild(0).GetChild(4).GetChild(0).rotation;
            diedZombieTrans.GetChild(0).GetChild(5).GetChild(0).rotation=transform.GetChild(0).GetChild(5).GetChild(0).rotation;
            diedZombieTrans.GetChild(0).GetChild(0).GetComponent<Rigidbody>().velocity=knockback;
            diedZombieTrans.GetChild(0).GetChild(1).GetChild(0).GetComponent<Rigidbody>().velocity=knockback;
            diedZombieTrans.GetChild(0).GetChild(2).GetChild(0).GetComponent<Rigidbody>().velocity=knockback;
            diedZombieTrans.GetChild(0).GetChild(3).GetChild(0).GetComponent<Rigidbody>().velocity=knockback;
            diedZombieTrans.GetChild(0).GetChild(4).GetChild(0).GetComponent<Rigidbody>().velocity=knockback;
            diedZombieTrans.GetChild(0).GetChild(5).GetChild(0).GetComponent<Rigidbody>().velocity=knockback;
            Destroy(diedZombieTrans.gameObject,10f);
            ObjectPools.zombieEntityPool.Release(gameObject);
    }




       public void Attack(){
        if(attackCD<=0f){
             targetPosition.gameObject.GetComponent<PlayerMove>().ApplyDamageAndKnockback(1f,transform.forward*10f+transform.up*15f);
               am.SetBool("attack",true);
        attackCD=1.2f;
        Invoke("CancelAttack",0.2f);  
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
       //      Debug.Log(cc.velocity);
       	return cc.velocity.magnitude;

	}




    public void Update () {
        if(zombieHealth<=0f&&isZombieDied==false){
            ZombieDie(entityMotionVec);
        }
        entityMotionVec=Vector3.Lerp(entityMotionVec,Vector3.zero, 3f * Time.deltaTime);

        if(!isPosInited){
            return;
        }
     
        if(transform.position.y<-40f){
            ObjectPools.zombieEntityPool.Release(gameObject);
        }
          if(attackCD>0f){
         attackCD-=Time.deltaTime;
            }
 
     
       

    
        Vector3 targetDir;
    
        targetDir = targetPosition.position;
        
        transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.Euler(new Vector3(0f,headTransform.eulerAngles.y,0f)),5f*Time.deltaTime);
        ChangeHeadPos(targetDir);
        entityVec.x=1f;
      
        if(Vector3.Magnitude(transform.position - targetDir)<1.6f){
            entitySpeed=Mathf.Lerp(entitySpeed,Speed(),5f*Time.deltaTime);
   //     Debug.Log(Speed());
            am.SetFloat("speed",entitySpeed);
            Attack();
          
        }else{
             if(entitySpeed<=0.01f){
                Jump();
            }
        if(cc.enabled==true){
            if(entityMotionVec.magnitude>0.7f){
                cc.Move(entityMotionVec*Time.deltaTime); 
            }else{
                 cc.Move((transform.forward*entityVec.x+transform.right*entityVec.z)*moveSpeed*Time.deltaTime+entityMotionVec*Time.deltaTime);
            }
        
        entitySpeed=Speed();
   //     Debug.Log(Speed());
        am.SetFloat("speed",  entitySpeed);

        }else return;
        }
        if(cc.enabled==true){
        cc.Move((new Vector3(0f,entityVec.y,0f))*moveSpeed*Time.deltaTime);
        }else return;
        if(cc.enabled==true){
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
        }
        
         
           
       // Quaternion targetRotation = Quaternion.LookRotation(targetDir);
       // entityFacingPos=targetRotation.eulerAngles;

        
       // Vector3 velocity = dir * speed * 6;
      //  controller.SimpleMove(velocity);

    }
}
