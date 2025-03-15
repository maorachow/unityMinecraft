using UnityEngine;

public partial class PlayerMove
{
    

    public void DropItem(int itemID,int itemCount)
    {

        AS.PlayOneShot(GlobalGameResourcesManager.instance.audioResourcesManager.TryGetEntityAudioClip("itemPopClip"));
      
       
        for (int i = 0; i < itemCount; i++)
        {
            VoxelWorld.currentWorld.itemEntityManager.SpawnNewItem(headPos.position.x, headPos.position.y, headPos.position.z, itemID,
                (headPos.forward * 6));
        }
        AttackAnimate();
        Invoke("cancelAttackInvoke", 0.16f);
    }

    public void TryInteractWithItemEntities()
    {
        if (isDied == true)
        {
            return;
        }
        Collider[] itemColliders = Physics.OverlapBox(transform.position+ new Vector3(0f, 0.95f, 0f), new Vector3(1f, 3f, 1f),
            Quaternion.identity, LayerMask.GetMask("ItemEntity"));
       
        foreach (var item in itemColliders)
        {
            if (item.GetComponent<ItemEntityBeh>() != null)
            {
                item.GetComponent<ItemEntityBeh>().TryEatItem(this);
            }
        }
    }
}
