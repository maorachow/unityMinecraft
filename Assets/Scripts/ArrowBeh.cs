using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class ArrowBeh : MonoBehaviour
{
    //public static GameObject arrowPrefab;
    public Rigidbody rigidbody;
    public EntityBeh entity;
    public Transform arrowTrans;
    public float lifeTime = 0f;
    public Transform sourceTrans;
    void Start()
    {
        entity= GetComponent<EntityBeh>();
        rigidbody = GetComponent<Rigidbody>();  
        arrowTrans=transform.GetChild(0);
    }
    public void OnDisable()
    {
        lifeTime = 0f;
        rigidbody.velocity= Vector3.zero;
    }
    // Update is called once per frame
    void Update()
    {
        if (rigidbody.velocity.magnitude > 0.1f)
        {
            arrowTrans.rotation = Quaternion.LookRotation(rigidbody.velocity.normalized);
        }
       
        lifeTime+= Time.deltaTime;
        if(lifeTime > 20f) {
            ObjectPools.arrowEntityPool.Release(this.gameObject);
        }
    }
    private void FixedUpdate()
    {
        if(entity.isInUnloadedChunks)
        {
            rigidbody.constraints = RigidbodyConstraints.FreezeAll;
;
        }
        else
        {
            rigidbody.constraints = RigidbodyConstraints.None;
        }
    }
    public void OnCollisionEnter(Collision collision)
    {
        Collider c = collision.collider;
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
