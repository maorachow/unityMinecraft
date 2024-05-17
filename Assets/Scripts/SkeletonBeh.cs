using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class SkeletonBeh : MonoBehaviour,ILivingEntity
{

    public static Transform playerPosition;
    public static AudioClip skeletonIdleClip;
    public static AudioClip skeletonShootClip;
    public static GameObject diedSkeletonPrefab;
    public AudioSource AS;
    public Vector3 entityVec;
    public Vector3 entityMotionVec;
    public Animator am;
    public Transform headTransform;
    public EntityBeh entity;
    public static bool isSkeletonPrefabLoaded=false;
    public CharacterController cc;
    public bool isSkeletonDied = false;
    public Transform currentTrans;
    public bool isPosInited = false;
    public float attackCD;
    public float gravity;
    public float entityY;
    public float jumpHeight = 2f;
    public bool isJumping = false;
    public float timeUsedToReachTarget = 0f;
    public bool isIdling;
    public bool hasReachedTarget = false;
    public float entityHealth { get ; set ; }
    public float moveSpeed { get { return 5f; } set { moveSpeed = 5f; } }

    public void ApplyGravity(CharacterController cc, float gravity, float dt)
    {
        //   Debug.Log("gra");
        if (cc.enabled == true)
        {
            cc.Move((new Vector3(0f, entityVec.y, 0f)) * moveSpeed * dt);
            if (cc.isGrounded != true)
            {

                if (entity.isInUnloadedChunks == false)
                {
                    entityY += gravity * dt;
                }

            }
            else
            {
                entityY = 0f;
            }
            if (cc.isGrounded == true && isJumping == true)
            {
                entityY = jumpHeight;
                isJumping = false;
            }
            entityVec.y = entityY;
        }
    }

    public void DieWithKnockback(Vector3 knockback)
    {
        AudioSource.PlayClipAtPoint(AS.clip, transform.position, 1f);
        isSkeletonDied = true;
        Transform curTrans = transform;
        Transform diedSkeletonTrans = Instantiate(diedSkeletonPrefab, curTrans.position, curTrans.rotation).GetComponent<Transform>();


        cc.enabled = false;
        diedSkeletonTrans.GetChild(0).GetChild(0).GetComponent<Rigidbody>().position = curTrans.GetChild(0).GetChild(0).position;
        diedSkeletonTrans.GetChild(0).GetChild(1).GetChild(0).GetComponent<Rigidbody>().position = curTrans.GetChild(0).GetChild(1).GetChild(0).position;
        diedSkeletonTrans.GetChild(0).GetChild(2).GetChild(0).GetComponent<Rigidbody>().position = curTrans.GetChild(0).GetChild(2).GetChild(0).position;
        diedSkeletonTrans.GetChild(0).GetChild(3).GetChild(0).GetComponent<Rigidbody>().position = curTrans.GetChild(0).GetChild(3).GetChild(0).position;
        diedSkeletonTrans.GetChild(0).GetChild(4).GetChild(0).GetComponent<Rigidbody>().position = curTrans.GetChild(0).GetChild(4).GetChild(0).position;
        diedSkeletonTrans.GetChild(0).GetChild(5).GetChild(0).GetComponent<Rigidbody>().position = curTrans.GetChild(0).GetChild(5).GetChild(0).position;

        diedSkeletonTrans.GetChild(0).GetChild(0).rotation = curTrans.GetChild(0).GetChild(0).rotation;
        diedSkeletonTrans.GetChild(0).GetChild(1).GetChild(0).GetComponent<Rigidbody>().rotation = curTrans.GetChild(0).GetChild(1).GetChild(0).rotation;
        diedSkeletonTrans.GetChild(0).GetChild(2).GetChild(0).GetComponent<Rigidbody>().rotation = curTrans.GetChild(0).GetChild(2).GetChild(0).rotation;
        diedSkeletonTrans.GetChild(0).GetChild(3).GetChild(0).GetComponent<Rigidbody>().rotation = curTrans.GetChild(0).GetChild(3).GetChild(0).rotation;
        diedSkeletonTrans.GetChild(0).GetChild(4).GetChild(0).GetComponent<Rigidbody>().rotation = curTrans.GetChild(0).GetChild(4).GetChild(0).rotation;
        diedSkeletonTrans.GetChild(0).GetChild(5).GetChild(0).GetComponent<Rigidbody>().rotation = curTrans.GetChild(0).GetChild(5).GetChild(0).rotation;



        diedSkeletonTrans.GetChild(0).GetChild(0).GetComponent<Rigidbody>().velocity = knockback;
        diedSkeletonTrans.GetChild(0).GetChild(1).GetChild(0).GetComponent<Rigidbody>().velocity = knockback;
        diedSkeletonTrans.GetChild(0).GetChild(2).GetChild(0).GetComponent<Rigidbody>().velocity = knockback;
        diedSkeletonTrans.GetChild(0).GetChild(3).GetChild(0).GetComponent<Rigidbody>().velocity = knockback;
        diedSkeletonTrans.GetChild(0).GetChild(4).GetChild(0).GetComponent<Rigidbody>().velocity = knockback;
        diedSkeletonTrans.GetChild(0).GetChild(5).GetChild(0).GetComponent<Rigidbody>().velocity = knockback;
        cc.enabled = true;
       
        Destroy(diedSkeletonTrans.gameObject, 30f);
     
         VoxelWorld.currentWorld.skeletonEntityPool.Release(gameObject);

    }

    public void EntityGroundSinkPrevent(CharacterController cc, int blockID, float dt)
    {
        if (cc.enabled == false)
        {
            return;
        }
        if (blockID > 0f && blockID < 100f)
        {
            cc.Move(new Vector3(0f, dt * 5f, 0f));
            gravity = 0f;
        }
        else
        {
            gravity = -9.8f;
        }
    }


    public void ApplyDamageAndKnockback(float damageAmount, Vector3 knockback)
    {
        AudioSource.PlayClipAtPoint(AS.clip, transform.position, 1f);
        transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material.color = Color.red;
        transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<MeshRenderer>().material.color = Color.red;
        transform.GetChild(0).GetChild(2).GetChild(0).GetComponent<MeshRenderer>().material.color = Color.red;
        transform.GetChild(0).GetChild(3).GetChild(0).GetComponent<MeshRenderer>().material.color = Color.red;
        transform.GetChild(0).GetChild(4).GetChild(0).GetComponent<MeshRenderer>().material.color = Color.red;
        transform.GetChild(0).GetChild(5).GetChild(0).GetComponent<MeshRenderer>().material.color = Color.red;
        entityHealth -= damageAmount;
        entityMotionVec = knockback;
        Invoke("InvokeRevertColor", 0.2f);
    }
    void InvokeRevertColor()
    {
        transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material.color = Color.white;
        transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<MeshRenderer>().material.color = Color.white;
        transform.GetChild(0).GetChild(2).GetChild(0).GetComponent<MeshRenderer>().material.color = Color.white;
        transform.GetChild(0).GetChild(3).GetChild(0).GetComponent<MeshRenderer>().material.color = Color.white;
        transform.GetChild(0).GetChild(4).GetChild(0).GetComponent<MeshRenderer>().material.color = Color.white;
        transform.GetChild(0).GetChild(5).GetChild(0).GetComponent<MeshRenderer>().material.color = Color.white;
    }

    public async void Attack()
    {
        if (attackCD <= 0f)
        {
            //      playerPosition.gameObject.GetComponent<PlayerMove>().ApplyDamageAndKnockback(1f, transform.forward * 10f + transform.up * 15f);
           
            am.SetBool("attack", true);
            attackCD =2f;
            await UniTask.Delay(200);
            if (headTransform == null)
            {
                return;
            }
            Vector3 arrowPos = headTransform.position + headTransform.forward*1.3f;
            EntityBeh arrow=EntityBeh.SpawnNewEntity(arrowPos.x, arrowPos.y, arrowPos.z, 4);
            arrow.GetComponent<ArrowBeh>().sourceTrans = transform;
            arrow.GetComponent<Rigidbody>().rotation=Quaternion.LookRotation(headTransform.forward);
          //  System.Diagnostics.Stopwatch sw=new System.Diagnostics.Stopwatch();
         //   sw.Start();
            
            await UniTask.WaitUntil(()=>arrow.GetComponent<ArrowBeh>().isPosInited==true);
            //   sw.Stop();
            // Debug.Log(sw.Elapsed.TotalMilliseconds);
            arrow.GetComponent<Rigidbody>().constraints=RigidbodyConstraints.None;
            arrow.GetComponent<Rigidbody>().WakeUp();
            arrow.GetComponent<Rigidbody>().AddForce( headTransform.forward*20f,ForceMode.VelocityChange);
            AudioSource.PlayClipAtPoint(skeletonShootClip, transform.position);
            Invoke("CancelAttack", 0.2f);
        }

    }
    public void CancelAttack()
    {
        am.SetBool("attack", false);
    }

    public void InitPos()
    {
        Invoke("InvokeInitPos", 0.1f);
    }
    public void InvokeInitPos()
    {
        isPosInited = true;
    }
    public void OnDisable()
    {
        entityMotionVec = Vector3.zero;
        isSkeletonDied = false;
        entityHealth = 20f;
        isPosInited = false;
    }
    void Start()
    {
        entityHealth = 20f;
        if (isSkeletonPrefabLoaded == false)
        {


            isSkeletonPrefabLoaded = true;
            playerPosition = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
            skeletonIdleClip = Resources.Load<AudioClip>("Audios/Skeleton_say1");
            diedSkeletonPrefab = Resources.Load<GameObject>("Prefabs/diedskeleton");
            skeletonShootClip = Resources.Load<AudioClip>("Audios/Bow_shoot");
        }
        currentTrans = transform;
        cc=GetComponent<CharacterController>(); 
        am=GetComponent<Animator>();
        entity = GetComponent<EntityBeh>();
        AS=GetComponent<AudioSource>();
        headTransform = transform.GetChild(0).GetChild(1);
    }
    public void ChangeHeadPos(Vector3 pos)
    {
        //   headTransform.rotation=q;
        Vector3 direction = (pos - headTransform.position).normalized;
        headTransform.rotation = Quaternion.Slerp(headTransform.rotation, Quaternion.LookRotation(direction), 10f * Time.deltaTime);
    }
    float Speed()
    {
        //      Debug.Log(cc.velocity);
        return new Vector3(cc.velocity.x,0,cc.velocity.y).magnitude;

    }
    public void Jump()
    {
        isJumping = true;
    }
    public float entitySpeed;

   public Vector3 targetPos;

    public void MoveToTarget(CharacterController cc, Vector3 pos, float dt)
    {
        currentTrans.rotation = Quaternion.Slerp(currentTrans.rotation, Quaternion.Euler(new Vector3(0f, headTransform.eulerAngles.y, 0f)), 5f * dt);
        if (cc.enabled == true)
        {
            if (entityMotionVec.magnitude > 0.7f)
            {
                cc.Move(entityMotionVec * dt);
            }
        }
            ChangeHeadPos(pos);
        if (isIdling)
        {
            if ((currentTrans.position - pos).magnitude < 1.4f)
            {
                hasReachedTarget = true;
            }
            else
            {
                hasReachedTarget = false;
            }
        }
        else
        {
            if ((currentTrans.position - pos).magnitude < 12f)
            {
                hasReachedTarget = true;
            }
            else
            {
                hasReachedTarget = false;
            }
        }
        if (hasReachedTarget)
        {
            entitySpeed = Speed();
            //     Debug.Log(Speed());
            am.SetFloat("speed", entitySpeed);
            return;
        }
        
        entityVec.x = 0.6f;
        if (entitySpeed <= 0.01f)
        {
            Jump();
        }
        if (cc.enabled == true)
        {
             
                cc.Move((currentTrans.forward * entityVec.x + currentTrans.right * entityVec.z) * moveSpeed * dt + entityMotionVec * dt);
             

            entitySpeed = Speed();
            //     Debug.Log(Speed());
            am.SetFloat("speed", entitySpeed);

        }
       
       
    }
    public void Update()
    {
        float dt = Time.deltaTime;
        if (!isPosInited)
        {
            return;
        }
        if (entityHealth <= 0f && isSkeletonDied == false)
        {
            DieWithKnockback(entityMotionVec);
        }
        entityMotionVec = Vector3.Lerp(entityMotionVec, Vector3.zero, 3f * dt);

        

        if (currentTrans.position.y < -40f)
        {
             VoxelWorld.currentWorld.skeletonEntityPool.Release(gameObject);
        }
        if (attackCD > 0f)
        {
            attackCD -= dt;
        }
        if (entity.isInUnloadedChunks == true)
        {
            return;
        }

        MoveToTarget(cc, targetPos, dt);
        if (Vector3.Magnitude(currentTrans.position - playerPosition.position) < 12f)
        {
 ;
           Attack();

        }
        ApplyGravity(cc, gravity, dt);





        // Quaternion targetRotation = Quaternion.LookRotation(targetDir);
        // entityFacingPos=targetRotation.eulerAngles;


        // Vector3 velocity = dir * speed * 6;
        //  controller.SimpleMove(velocity);

    }

    public int curBlockOnFootID;
    public int prevBlockOnFootID;
    public void FixedUpdate()
    {
        if (!isPosInited)
        {
            return;
        }
        if (entity.isInUnloadedChunks == true)
        {
            return;
        }
       
      
        curBlockOnFootID = WorldHelper.instance.GetBlock(currentTrans.position, entity.currentChunk);
        if (curBlockOnFootID == 0 || (101 <= curBlockOnFootID && curBlockOnFootID <= 200))
        {
            gravity = -9.8f;
        }
        EntityGroundSinkPrevent(cc, curBlockOnFootID, Time.deltaTime);
        if (prevBlockOnFootID != curBlockOnFootID)
        {
            if (curBlockOnFootID == 100)
            {
                gravity = -1f;
                AudioSource.PlayClipAtPoint(PlayerMove.playerSinkClip, transform.position, 1f);
                ParticleEffectManagerBeh.instance.EmitWaterSplashParticleAtPosition(transform.position);
            }
            else
            {
                gravity = -9.8f;
            }

        }
        prevBlockOnFootID = curBlockOnFootID;
        //  targetPos = playerPosition.position;
        if (Random.Range(0f, 100f) > 99f)
        {
            AudioSource.PlayClipAtPoint(skeletonIdleClip, currentTrans.position, 1f);
        }
         
        if ((playerPosition.position-transform.position).magnitude<32f)
        {
            //  Debug.Log("hit");

             
                isIdling = false;
           
          

        }
        else
        {
            isIdling = true;
        }
        if (isIdling == true)
        {
            if (hasReachedTarget == true)
            {
                timeUsedToReachTarget = 0f;
                Vector2 randomTargetPos = new Vector2(Random.Range(transform.position.x - 8f, transform.position.x + 8f), Random.Range(transform.position.z - 8f, transform.position.z + 8f));
                Vector3 finalTargetPos = new Vector3(randomTargetPos.x, WorldHelper.instance.GetChunkLandingPoint(randomTargetPos.x, randomTargetPos.y) + 1f, randomTargetPos.y);
                targetPos = finalTargetPos;
            }
            else
            {
                timeUsedToReachTarget += Time.deltaTime;
                if (timeUsedToReachTarget >= 5f)
                {
                    hasReachedTarget = true;
                    timeUsedToReachTarget = 0f;
                    Vector2 randomTargetPos = new Vector2(Random.Range(transform.position.x - 8f, transform.position.x + 8f), Random.Range(transform.position.z - 8f, transform.position.z + 8f));
                    Vector3 finalTargetPos = new Vector3(randomTargetPos.x, WorldHelper.instance.GetChunkLandingPoint(randomTargetPos.x, randomTargetPos.y) + 1f, randomTargetPos.y);
                    targetPos = finalTargetPos;
                }
            }

        }
        else
        {
            timeUsedToReachTarget = 0f;
            targetPos =playerPosition.position+new Vector3(0f,0.6f,0f);
        }
       
    }
}
