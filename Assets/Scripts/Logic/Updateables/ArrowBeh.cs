using UnityEngine;

public class ArrowBeh : MonoBehaviour
{
    //public static GameObject arrowPrefab;
    public Rigidbody arrowRigidbody;
    public EntityBeh entity;
    public Transform arrowTrans;
    public float lifeTime = 0f;
    public Transform sourceTrans;
    public bool isPosInited = false;
    public float arrowDamage;
    public float arrowDamageRandomRange;
     
     void Start()
    {
        entity= GetComponent<EntityBeh>();
        arrowRigidbody = GetComponent<Rigidbody>();  
        arrowTrans=transform.GetChild(0);
        

    }

    public void CheckIsOwnedByPlayer()
    {
        BoxCollider arrowCollider = GetComponent<BoxCollider>();
        if (sourceTrans.GetComponent<PlayerMove>() != null)
        {
            arrowCollider.gameObject.layer = LayerMask.NameToLayer("PlayerOwnedProjectile");
          
        }
    }
    public void OnEnable()
    {
        entity = GetComponent<EntityBeh>();
        arrowRigidbody = GetComponent<Rigidbody>();
        arrowRigidbody.velocity = new Vector3(0, 0, 0);
        gameObject.layer = LayerMask.NameToLayer("Projectile");
        arrowRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        isPosInited = true;
        entity.isInUnloadedChunks = false;
        arrowDamageRandomRange = 2f;
        arrowDamage = 3f;


    }

    public void SetArrowDamage(float damage,float damageRandomRange)
    {
        this.arrowDamage= damage;
        this.arrowDamageRandomRange= damageRandomRange;
    }
    public void OnDisable()
    {
        lifeTime = 0f;
        arrowRigidbody.velocity= Vector3.zero;
        isPosInited = false;
        entity.isInUnloadedChunks = false;
        sourceTrans = null;
        arrowRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
    }
    // Update is called once per frame
    public float deltaTimeFromPrevFixedUpdate = 0f;

    public Vector3 velocityPrev1;
    public Vector3 velocityLerped;
     
    Vector3 velocityPreDelta;
    void Update()
    {
        deltaTimeFromPrevFixedUpdate += Time.deltaTime;
        velocityLerped =Vector3.Lerp(velocityLerped, velocityPrev1, Time.deltaTime*10f);

        if (velocityLerped.magnitude > 0.01f)
        {
       //     arrowTrans.rotation = Quaternion.LookRotation(velocityLerped);
        }
       
         lifeTime += Time.deltaTime;
        if(lifeTime > 5f&& velocityPrev1.magnitude<0.1f) {
            entity.ReleaseEntity();
            return;
        }
    }
   
    private void FixedUpdate()
    {
        velocityPrev1 = arrowRigidbody.velocity;
      
        deltaTimeFromPrevFixedUpdate = 0f;
        if (entity.isInUnloadedChunks)
        {
            arrowRigidbody.constraints = RigidbodyConstraints.FreezeAll;
;
        }
        else
        {
            arrowRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        }
       
    }
    public void OnTriggerStay(Collider other )
    {
        Collider c = other;
        if(sourceTrans == null) {
            Debug.Log("damage failed:null sourcetrans");
            return;
        }
       
        if(c.transform==sourceTrans)
        {
            Debug.Log("damage failed:sourcetrans");
            return;
        }
        if (c.gameObject.tag != "Entity" && c.gameObject.tag != "Player")
        {

            return;
        }

        if (c is CharacterController)
        {
            float arrowVelocityMagnitude = arrowRigidbody.velocity.magnitude;
            
            if (c.GetComponent(typeof(ILivingEntity)) != null && arrowVelocityMagnitude > 0.1f)
            {

            //    Debug.Log("damage success:entity");
                ILivingEntity livingEntity = (ILivingEntity)c.GetComponent(typeof(ILivingEntity));
                livingEntity.ApplyDamageAndKnockback((arrowDamage + Random.Range(-arrowDamageRandomRange / 2.0f, arrowDamageRandomRange / 2.0f))* Mathf.Min(arrowVelocityMagnitude,1f), (sourceTrans.position - c.transform.position).normalized * Random.Range(-5f, -10f));

                if(c.GetComponent(typeof(IAttackableEntityTarget))!=null)
                {
                    IAttackableEntityTarget attackableTarget = (IAttackableEntityTarget)c.GetComponent(typeof(IAttackableEntityTarget));
                    IAttackableEntityTarget attackSource = (IAttackableEntityTarget)sourceTrans.GetComponent(typeof(IAttackableEntityTarget));
                    if (attackableTarget != null && attackSource != null)
                    {
                        if (attackSource.entityTransformRef.gameObject.activeInHierarchy == true)
                        {
                            attackableTarget.TryAddPriamryAttackerEntity(attackSource);
                        }
                      
                        
                       
                    }
                }
                entity.ReleaseEntity();
                return;
            }

            if (c.GetComponent<PlayerMove>() != null&& arrowVelocityMagnitude > 0.1f)
            {
         //       Debug.Log("damage success:player");
                c.GetComponent<PlayerMove>().ApplyDamageAndKnockback((arrowDamage + Random.Range(-arrowDamageRandomRange / 2.0f, arrowDamageRandomRange / 2.0f)) *Mathf.Min(arrowVelocityMagnitude, 1f), (sourceTrans.position - c.transform.position).normalized * Random.Range(-5f, -10f));
                entity.ReleaseEntity();
                return;
            }

            if (arrowVelocityMagnitude < 0.2f)
            {
            //    Debug.Log("damage failed:not enough velocity");
            }
        //    Debug.Log("character controller damage failed: arrow velocity:"+arrowVelocityMagnitude);
        }
        
  /*      if (c.GetComponent<CreeperBeh>() != null)
        {
            c.GetComponent<CreeperBeh>().ApplyDamageAndKnockback(3f + Random.Range(-2f, 2f), (sourceTrans.position - c.transform.position).normalized * Random.Range(-5f, -10f));
        }
        if (c.GetComponent<ZombieBeh>() != null)
        {
            c.GetComponent<ZombieBeh>().ApplyDamageAndKnockback(3f + Random.Range(-2f, 2f), (sourceTrans.position - c.transform.position).normalized * Random.Range(-5f, -10f));
        }
        if (c.GetComponent<SkeletonBeh>() != null)
        {
            c.GetComponent<SkeletonBeh>().ApplyDamageAndKnockback(3f + Random.Range(-2f, 2f), (sourceTrans.position - c.transform.position).normalized * Random.Range(-5f, -10f));
        }
        if (c.GetComponent<EndermanBeh>() != null)
        {
            c.GetComponent<EndermanBeh>().ApplyDamageAndKnockback(3f + Random.Range(-2f, 2f), (sourceTrans.position - c.transform.position).normalized * Random.Range(-5f, -10f));
        }*/
      /*  if (c.gameObject.layer!=0&&c.gameObject.layer!=8)
        {
          
        }*/
       
    }
}
