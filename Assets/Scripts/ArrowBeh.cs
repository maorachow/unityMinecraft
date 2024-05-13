using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
public class ArrowBeh : MonoBehaviour
{
    //public static GameObject arrowPrefab;
    public Rigidbody arrowRigidbody;
    public EntityBeh entity;
    public Transform arrowTrans;
    public float lifeTime = 0f;
    public Transform sourceTrans;
    public bool isPosInited = false;
     
     void Start()
    {
        entity= GetComponent<EntityBeh>();
        arrowRigidbody = GetComponent<Rigidbody>();  
        arrowTrans=transform.GetChild(0);
       
    }
    public void OnEnable()
    {
        entity = GetComponent<EntityBeh>();
        arrowRigidbody = GetComponent<Rigidbody>();
        arrowRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        isPosInited = true;
        entity.isInUnloadedChunks = false;
    }
    public void OnDisable()
    {
        lifeTime = 0f;
        arrowRigidbody.velocity= Vector3.zero;
        isPosInited = false;
        entity.isInUnloadedChunks = false;
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
            arrowTrans.rotation = Quaternion.LookRotation(velocityLerped);
        }
       
         lifeTime += Time.deltaTime;
        if(lifeTime > 20f) {
            VoxelWorld.currentWorld.arrowEntityPool.Release(this.gameObject);
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
    public void OnTriggerEnter(Collider other )
    {
        Collider c = other;
        if(sourceTrans == null) {
            return;
        }
        if (arrowRigidbody.velocity.magnitude < 5f)
        {
            return;
        }
        if(c.transform==sourceTrans)
        {
            return;
        }
        if (c.gameObject.tag != "Entity" && c.gameObject.tag != "Player")
        {
            return;
        }
        if (c.GetComponent(typeof(ILivingEntity)) != null)
        {
            ILivingEntity livingEntity = (ILivingEntity)c.GetComponent(typeof(ILivingEntity));
            livingEntity.ApplyDamageAndKnockback(3f + Random.Range(-2f, 2f), (sourceTrans.position - c.transform.position).normalized * Random.Range(-5f, -10f));
        }
        if (c.GetComponent<PlayerMove>() != null)
        {
            c.GetComponent<PlayerMove>().ApplyDamageAndKnockback(3f + Random.Range(-2f, 2f), (sourceTrans.position - c.transform.position).normalized * Random.Range(-5f, -10f));
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
        if (c.gameObject.layer!=0&&c.gameObject.layer!=8)
        {
            VoxelWorld.currentWorld.arrowEntityPool.Release(this.gameObject);
        }
       
    }
}
