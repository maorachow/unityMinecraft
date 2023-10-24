using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieBeh : MonoBehaviour,ILivingEntity
{   
    public int curBlockOnFootID;
    public int prevBlockOnFootID;
    public AudioSource AS;
    public static AudioClip zombieIdleClip;
    public static Transform targetPosition;
    public Transform currentTrans;
    public static GameObject diedZombiePrefab;
    public bool isZombieDied=false;
    private CharacterController cc;
    public Animator am;
    public Transform headTransform;
    public float entityHealth{get;set;}
    public Vector3 entityVec;
    public Vector3 entityMotionVec;
    public float moveSpeed{get{return 5f;}set{moveSpeed=5f;}}
    public float gravity=-9.8f;
    public float entityY=0f;
    public float jumpHeight=2f;
    public Vector3 entityFacingPos;
    public bool isPosInited=false;
    public float attackCD=1.2f;
    public bool isJumping=false;
    public float entitySpeed;
    public EntityBeh entity;
    public static bool isZombiePrefabLoaded=false;

     public void ApplyDamageAndKnockback(float damageAmount,Vector3 knockback){
       AudioSource.PlayClipAtPoint(AS.clip,transform.position,1f);
        transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.red;
         transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.red;
          transform.GetChild(0).GetChild(2).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.red;
           transform.GetChild(0).GetChild(3).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.red;
           transform.GetChild(0).GetChild(4).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.red;
           transform.GetChild(0).GetChild(5).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.red;
        entityHealth-=damageAmount;
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
        entityHealth=20f;
       entity=GetComponent<EntityBeh>();
        AS=GetComponent<AudioSource>();
        if(isZombiePrefabLoaded==false){
            zombieIdleClip=Resources.Load<AudioClip>("Audios/Zombie_say1");
         diedZombiePrefab=Resources.Load<GameObject>("Prefabs/diedzombie");
         isZombiePrefabLoaded=true;
       targetPosition=GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        }
            currentTrans=transform;
        
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
            entityHealth=20f;
            isPosInited=false;
    }
    
    public void DieWithKnockback(Vector3 knockback){
    AudioSource.PlayClipAtPoint(AS.clip,transform.position,1f);
        isZombieDied=true;
            Transform curTrans=transform;
            Transform diedZombieTrans=Instantiate(diedZombiePrefab,curTrans.position,curTrans.rotation).GetComponent<Transform>();
       

            cc.enabled=false;
            diedZombieTrans.GetChild(0).GetChild(0).GetComponent<Rigidbody>().position=curTrans.GetChild(0).GetChild(0).position;
            diedZombieTrans.GetChild(0).GetChild(1).GetChild(0).GetComponent<Rigidbody>().position=curTrans.GetChild(0).GetChild(1).GetChild(0).position;
            diedZombieTrans.GetChild(0).GetChild(2).GetChild(0).GetComponent<Rigidbody>().position=curTrans.GetChild(0).GetChild(2).GetChild(0).position;
            diedZombieTrans.GetChild(0).GetChild(3).GetChild(0).GetComponent<Rigidbody>().position=curTrans.GetChild(0).GetChild(3).GetChild(0).position;
            diedZombieTrans.GetChild(0).GetChild(4).GetChild(0).GetComponent<Rigidbody>().position=curTrans.GetChild(0).GetChild(4).GetChild(0).position;
            diedZombieTrans.GetChild(0).GetChild(5).GetChild(0).GetComponent<Rigidbody>().position=curTrans.GetChild(0).GetChild(5).GetChild(0).position;

            diedZombieTrans.GetChild(0).GetChild(0).rotation=curTrans.GetChild(0).GetChild(0).rotation;
            diedZombieTrans.GetChild(0).GetChild(1).GetChild(0).GetComponent<Rigidbody>().rotation=curTrans.GetChild(0).GetChild(1).GetChild(0).rotation;
            diedZombieTrans.GetChild(0).GetChild(2).GetChild(0).GetComponent<Rigidbody>().rotation=curTrans.GetChild(0).GetChild(2).GetChild(0).rotation;
            diedZombieTrans.GetChild(0).GetChild(3).GetChild(0).GetComponent<Rigidbody>().rotation=curTrans.GetChild(0).GetChild(3).GetChild(0).rotation;
            diedZombieTrans.GetChild(0).GetChild(4).GetChild(0).GetComponent<Rigidbody>().rotation=curTrans.GetChild(0).GetChild(4).GetChild(0).rotation;
            diedZombieTrans.GetChild(0).GetChild(5).GetChild(0).GetComponent<Rigidbody>().rotation=curTrans.GetChild(0).GetChild(5).GetChild(0).rotation;

         

            diedZombieTrans.GetChild(0).GetChild(0).GetComponent<Rigidbody>().velocity=knockback;
            diedZombieTrans.GetChild(0).GetChild(1).GetChild(0).GetComponent<Rigidbody>().velocity=knockback;
            diedZombieTrans.GetChild(0).GetChild(2).GetChild(0).GetComponent<Rigidbody>().velocity=knockback;
            diedZombieTrans.GetChild(0).GetChild(3).GetChild(0).GetComponent<Rigidbody>().velocity=knockback;
            diedZombieTrans.GetChild(0).GetChild(4).GetChild(0).GetComponent<Rigidbody>().velocity=knockback;
            diedZombieTrans.GetChild(0).GetChild(5).GetChild(0).GetComponent<Rigidbody>().velocity=knockback;
            cc.enabled=true;
            Destroy(diedZombieTrans.gameObject,30f);
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


    public void FixedUpdate(){
        curBlockOnFootID=WorldHelper.instance.GetBlock(currentTrans.position,entity.currentChunk);
         if(curBlockOnFootID==0||(101<=curBlockOnFootID&&curBlockOnFootID<=200)){
        gravity=-9.8f;
        }
          EntityGroundSinkPrevent(cc,curBlockOnFootID,Time.deltaTime);
          if(prevBlockOnFootID!=curBlockOnFootID){
            if(curBlockOnFootID==100){
             gravity=-1f;
             AudioSource.PlayClipAtPoint(PlayerMove.playerSinkClip,transform.position,1f);
             WaterSplashParticleBeh.instance.EmitParticleAtPosition(transform.position);
            }else{
               gravity=-9.8f;
            }
             
        }
        prevBlockOnFootID=curBlockOnFootID;
        targetPos = targetPosition.position;
        if(Random.Range(0f,100f)>99f){
             AudioSource.PlayClipAtPoint(zombieIdleClip,currentTrans.position,1f);
        }
    }
    

        Vector3 targetPos;
          public void EntityGroundSinkPrevent(CharacterController cc,int blockID,float dt){
         if(blockID>0f&&blockID<100f){
            cc.Move(new Vector3(0f,dt*5f,0f));
            gravity=0f;
         }  else{
            gravity=-9.8f;
         }
    }
        public void MoveToTarget(CharacterController cc,Vector3 pos,float dt){
             currentTrans.rotation=Quaternion.Slerp(currentTrans.rotation,Quaternion.Euler(new Vector3(0f,headTransform.eulerAngles.y,0f)),5f*dt);
                ChangeHeadPos(pos);
                  entityVec.x=0.6f;
                if(entitySpeed<=0.01f){
                Jump();
                }
        if(cc.enabled==true){
            if(entityMotionVec.magnitude>0.7f){
                cc.Move(entityMotionVec*dt); 
            }else{
                 cc.Move((currentTrans.forward*entityVec.x+currentTrans.right*entityVec.z)*moveSpeed*dt+entityMotionVec*dt);
            }
        
        entitySpeed=Speed();
   //     Debug.Log(Speed());
        am.SetFloat("speed",  entitySpeed);

        }else return;
        }
        public void ApplyGravity(CharacterController cc,float gravity,float dt){
            if(cc.enabled==true){
              cc.Move((new Vector3(0f,entityVec.y,0f))*moveSpeed*dt);
        if(cc.isGrounded!=true){
            if(!GetComponent<EntityBeh>().isInUnloadedChunks){
             entityY+=gravity*dt;   
            }
            
        }else{
            entityY=0f;
        }
        if(cc.isGrounded==true&&isJumping==true){
            entityY=jumpHeight;
            isJumping=false;
        }
        entityVec.y=entityY;    
        }else return;
        }
    public void Update () {
       float dt=Time.deltaTime;
        if(entityHealth<=0f&&isZombieDied==false){
            DieWithKnockback(entityMotionVec);
        }
        entityMotionVec=Vector3.Lerp(entityMotionVec,Vector3.zero, 3f * dt);

        if(!isPosInited){
            return;
        }
     
        if(currentTrans.position.y<-40f){
            ObjectPools.zombieEntityPool.Release(gameObject);
        }
          if(attackCD>0f){
         attackCD-=dt;
            }
       
       
      
        if(Vector3.Magnitude(currentTrans.position - targetPos)<1.4f){
             entityVec.x=0f;
              if(entityMotionVec.magnitude>0.7f){
                cc.Move(entityMotionVec*dt); 
            }else{
                 cc.Move((currentTrans.forward*entityVec.x+currentTrans.right*entityVec.z)*moveSpeed*dt+entityMotionVec*dt);
            }
            entitySpeed=Mathf.Lerp(entitySpeed,Speed(),5f*dt);
   //     Debug.Log(Speed());
            am.SetFloat("speed",entitySpeed);
            Attack();
          
        }else{
            MoveToTarget(cc,targetPos,dt);
        }
        ApplyGravity(cc,gravity,dt);

        
        
         
           
       // Quaternion targetRotation = Quaternion.LookRotation(targetDir);
       // entityFacingPos=targetRotation.eulerAngles;

        
       // Vector3 velocity = dir * speed * 6;
      //  controller.SimpleMove(velocity);

    }
}
