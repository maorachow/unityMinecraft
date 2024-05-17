using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
public class EndermanBeh : MonoBehaviour, ILivingEntity
{
    public int curBlockOnFootID;
    public int prevBlockOnFootID;
    public AudioSource AS;
    public static AudioClip endermanIdleClip;
    public static Transform playerPosition;
    public Transform currentTrans;
    public static GameObject diedEndermanPrefab;
    public bool isEndermanDied = false;
    private CharacterController cc;
    public Animator am;
    public Transform headTransform;
    public float entityHealth { get; set; }
    public Vector3 entityVec;
    public Vector3 entityMotionVec;
    public float moveSpeed { get { return 5f; } set { moveSpeed = 5f; } }
    public float gravity = -9.8f;
    public float entityY = 0f;
    public float jumpHeight = 2f;
    public Vector3 entityFacingPos;
    public bool isPosInited = false;
    public float attackCD = 1.2f;
    public bool isJumping = false;
    public float entitySpeed;
    public EntityBeh entity;
    public bool isIdling { get; set; }
    public float timeUsedToReachTarget;
    public bool hasReachedTarget;
    public static bool isEndermanPrefabLoaded = false;
    public bool isTargetingPlayer=false;

    public void ApplyDamageAndKnockback(float damageAmount, Vector3 knockback)
    {
        AudioSource.PlayClipAtPoint(AS.clip, transform.position, 1f);
        transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material.color = Color.red;
        transform.GetChild(0).GetChild(1).GetComponent<MeshRenderer>().material.color = Color.red;
        transform.GetChild(1).GetComponent<MeshRenderer>().material.color = Color.red;
        transform.GetChild(2).GetChild(0).GetComponent<MeshRenderer>().material.color = Color.red;
        transform.GetChild(3).GetChild(0).GetComponent<MeshRenderer>().material.color = Color.red;
        transform.GetChild(4).GetChild(0).GetComponent<MeshRenderer>().material.color = Color.red;
        transform.GetChild(5).GetChild(0).GetComponent<MeshRenderer>().material.color = Color.red;
         
        entityHealth -= damageAmount;
        entityMotionVec = knockback;
        Invoke("InvokeRevertColor", 0.2f);
    }
    void InvokeRevertColor()
    {
        transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material.color = Color.white;
        transform.GetChild(0).GetChild(1).GetComponent<MeshRenderer>().material.color = Color.white;
        transform.GetChild(1).GetComponent<MeshRenderer>().material.color = Color.white;
        transform.GetChild(2).GetChild(0).GetComponent<MeshRenderer>().material.color = Color.white;
        transform.GetChild(3).GetChild(0).GetComponent<MeshRenderer>().material.color = Color.white;
        transform.GetChild(4).GetChild(0).GetComponent<MeshRenderer>().material.color = Color.white;
        transform.GetChild(5).GetChild(0).GetComponent<MeshRenderer>().material.color = Color.white;
    }

    public void Start()
    {
        entityHealth = 30f;
        entity = GetComponent<EntityBeh>();
        AS = GetComponent<AudioSource>();
        if (isEndermanPrefabLoaded == false)
        {
            endermanIdleClip = Resources.Load<AudioClip>("Audios/Enderman_idle1");
            diedEndermanPrefab = Resources.Load<GameObject>("Prefabs/diedenderman");
            isEndermanPrefabLoaded = true;
            playerPosition = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        }
        currentTrans = transform;

        headTransform = transform.GetChild(0);
        entityFacingPos = transform.rotation.eulerAngles;

        cc = GetComponent<CharacterController>();
        am = GetComponent<Animator>();

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
        isEndermanDied = false;
        entityHealth = 30f;
        isTargetingPlayer = false;
        isPosInited = false;
    }

    public void DieWithKnockback(Vector3 knockback)
    {
        AudioSource.PlayClipAtPoint(AS.clip, transform.position, 1f);
        isEndermanDied = true;
        Transform curTrans = transform;
        Transform diedEndermanTrans = Instantiate(diedEndermanPrefab, curTrans.position, curTrans.rotation).GetComponent<Transform>();


        cc.enabled = false;
        diedEndermanTrans.GetChild(0).GetChild(0).GetChild(0).GetComponent<Rigidbody>().position= transform.GetChild(0).GetChild(0).GetChild(0).position;
        diedEndermanTrans.GetChild(0).GetChild(1).GetComponent<Rigidbody>().position= transform.GetChild(0).GetChild(1).position;
        diedEndermanTrans.GetChild(1).GetComponent<Rigidbody>().position= transform.GetChild(1).position;
        diedEndermanTrans.GetChild(2).GetChild(0).GetComponent<Rigidbody>().position= transform.GetChild(2).GetChild(0).position;
        diedEndermanTrans.GetChild(3).GetChild(0).GetComponent<Rigidbody>().position= transform.GetChild(3).GetChild(0).position;
        diedEndermanTrans.GetChild(4).GetChild(0).GetComponent<Rigidbody>().position= transform.GetChild(4).GetChild(0).position;
        diedEndermanTrans.GetChild(5).GetChild(0).GetComponent<Rigidbody>().position= transform.GetChild(5).GetChild(0).position;



        diedEndermanTrans.GetChild(0).GetChild(0).GetChild(0).GetComponent<Rigidbody>().rotation = transform.GetChild(0).GetChild(0).GetChild(0).rotation;
        diedEndermanTrans.GetChild(0).GetChild(1).GetComponent<Rigidbody>().rotation = transform.GetChild(0).GetChild(1).rotation;
        diedEndermanTrans.GetChild(1).GetComponent<Rigidbody>().rotation = transform.GetChild(1).rotation;
        diedEndermanTrans.GetChild(2).GetChild(0).GetComponent<Rigidbody>().rotation = transform.GetChild(2).GetChild(0).rotation;
        diedEndermanTrans.GetChild(3).GetChild(0).GetComponent<Rigidbody>().rotation = transform.GetChild(3).GetChild(0).rotation;
        diedEndermanTrans.GetChild(4).GetChild(0).GetComponent<Rigidbody>().rotation = transform.GetChild(4).GetChild(0).rotation;
        diedEndermanTrans.GetChild(5).GetChild(0).GetComponent<Rigidbody>().rotation = transform.GetChild(5).GetChild(0).rotation;



        diedEndermanTrans.GetChild(0).GetChild(0).GetChild(0).GetComponent<Rigidbody>().velocity = knockback;
        diedEndermanTrans.GetChild(0).GetChild(1).GetComponent<Rigidbody>().velocity = knockback;
        diedEndermanTrans.GetChild(1).GetComponent<Rigidbody>().velocity = knockback;
        diedEndermanTrans.GetChild(2).GetChild(0).GetComponent<Rigidbody>().velocity = knockback;
        diedEndermanTrans.GetChild(3).GetChild(0).GetComponent<Rigidbody>().velocity = knockback;
        diedEndermanTrans.GetChild(4).GetChild(0).GetComponent<Rigidbody>().velocity = knockback;
        diedEndermanTrans.GetChild(5).GetChild(0).GetComponent<Rigidbody>().velocity = knockback;
        cc.enabled = true;
        Destroy(diedEndermanTrans.gameObject, 30f);
        ItemEntityBeh.SpawnNewItem(transform.position.x, transform.position.y, transform.position.z, 13, new Vector3(Random.Range(-3f, 3f), Random.Range(-3f, 3f), Random.Range(-3f, 3f)));
        VoxelWorld.currentWorld.endermanEntityPool.Release(gameObject);

    }




    public void Attack()
    {
        if (attackCD <= 0f)
        {
            playerPosition.gameObject.GetComponent<PlayerMove>().ApplyDamageAndKnockback(1f, transform.forward * 10f + transform.up * 15f);
            am.SetBool("attack", true);
            attackCD = 1.2f;
            Invoke("CancelAttack", 0.2f);
        }

    }
    public void CancelAttack()
    {
        am.SetBool("attack", false);
    }

    public void Jump()
    {
        isJumping = true;
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
        return cc.velocity.magnitude;

    }


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
            AudioSource.PlayClipAtPoint(endermanIdleClip, currentTrans.position, 1f);
        }
         
       
            isIdling = true;
        

        if (isIdling == true&&isTargetingPlayer==false)
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
            targetPos = playerPosition.position;
        }
       
    }


    Vector3 targetPos;
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
    public void MoveToTarget(CharacterController cc, Vector3 pos, float dt)
    {

        currentTrans.rotation = Quaternion.Slerp(currentTrans.rotation, Quaternion.Euler(new Vector3(0f, headTransform.eulerAngles.y, 0f)), 5f * dt);
        ChangeHeadPos(pos);
        entityVec.x = 0.6f;
        if (entitySpeed <= 0.01f)
        {
            Jump();
        }
        if (cc.enabled == true)
        {
            if (entityMotionVec.magnitude > 0.7f)
            {
                cc.Move(entityMotionVec * dt);
            }
            else
            {
                cc.Move((currentTrans.forward * entityVec.x + currentTrans.right * entityVec.z) * moveSpeed * dt + entityMotionVec * dt);
            }

            entitySpeed = Speed();
            //     Debug.Log(Speed());
            am.SetFloat("speed", entitySpeed);

        }
        if ((currentTrans.position - pos).magnitude < 2.4f)
        {
            hasReachedTarget = true;
        }
        else
        {
            hasReachedTarget = false;
        }
    }
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
    public void Update()
    {
        float dt = Time.deltaTime;
        if (entityHealth <= 0f && isEndermanDied == false)
        {
            DieWithKnockback(entityMotionVec);
        }
        entityMotionVec = Vector3.Lerp(entityMotionVec, Vector3.zero, 3f * dt);

        if (!isPosInited)
        {
            return;
        }

        if (currentTrans.position.y < -40f)
        {
            VoxelWorld.currentWorld.endermanEntityPool.Release(gameObject);
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
        if (Vector3.Magnitude(currentTrans.position - playerPosition.position) < 2.4f)
        {
            if(isTargetingPlayer)
            {
            //  hasReachedTarget=true;
            entityVec.x = 0f;

            entitySpeed = Mathf.Lerp(entitySpeed, Speed(), 5f * dt);
            //     Debug.Log(Speed());
            //  am.SetFloat("speed",entitySpeed);
            Attack();
            }
           

        }
        ApplyGravity(cc, gravity, dt);





        // Quaternion targetRotation = Quaternion.LookRotation(targetDir);
        // entityFacingPos=targetRotation.eulerAngles;


        // Vector3 velocity = dir * speed * 6;
        //  controller.SimpleMove(velocity);

    }
}
