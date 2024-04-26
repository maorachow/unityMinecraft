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
    public Rigidbody rigidbody;
    public EntityBeh entity;
    public Transform arrowTrans;
    public float lifeTime = 0f;
    public Transform sourceTrans;
    public bool isPosInited = false;
     
     void Start()
    {
        entity= GetComponent<EntityBeh>();
        rigidbody = GetComponent<Rigidbody>();  
        arrowTrans=transform.GetChild(0);
       
    }
    public void OnEnable()
    {
        entity = GetComponent<EntityBeh>();
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.constraints = RigidbodyConstraints.None;
        isPosInited = true;
        entity.isInUnloadedChunks = false;
    }
    public void OnDisable()
    {
        lifeTime = 0f;
        rigidbody.velocity= Vector3.zero;
        isPosInited = false;
        entity.isInUnloadedChunks = false;
        rigidbody.constraints = RigidbodyConstraints.None;
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
            ObjectPools.arrowEntityPool.Release(this.gameObject);
        }
    }
   
    private void FixedUpdate()
    {
        velocityPrev1 = rigidbody.velocity;
      
        deltaTimeFromPrevFixedUpdate = 0f;
        if (entity.isInUnloadedChunks)
        {
            rigidbody.constraints = RigidbodyConstraints.FreezeAll;
;
        }
        else
        {
            rigidbody.constraints = RigidbodyConstraints.None;
        }
       
    }
    public void OnTriggerEnter(Collider other )
    {
        Collider c = other;
        if(sourceTrans == null) {
            return;
        }
        if (rigidbody.velocity.magnitude < 5f)
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
        if (c.GetComponent<PlayerMove>() != null)
        {
            c.GetComponent<PlayerMove>().ApplyDamageAndKnockback(3f + Random.Range(-2f, 2f), (sourceTrans.position - c.transform.position).normalized * Random.Range(-5f, -10f));
        }
        if (c.GetComponent<CreeperBeh>() != null)
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
        if(c.gameObject.layer!=0&&c.gameObject.layer!=8)
        {
        ObjectPools.arrowEntityPool.Release(this.gameObject);
        }
       
    }
}
