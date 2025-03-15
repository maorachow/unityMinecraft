using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Internal;

public class GlobalAudioResourcesManager
{
    public Dictionary<int, AudioClip> blockAudioDictionary;

    public Dictionary<string, AudioClip> entityAudioDictionary;


    public void LoadDefaultBlockAudioResources()
    {
        blockAudioDictionary = new Dictionary<int, AudioClip>();
        blockAudioDictionary.TryAdd(1, Resources.Load<AudioClip>("Audios/Stone_dig2"));
        blockAudioDictionary.TryAdd(2, Resources.Load<AudioClip>("Audios/Grass_dig1"));
        blockAudioDictionary.TryAdd(3, Resources.Load<AudioClip>("Audios/Gravel_dig1"));
        blockAudioDictionary.TryAdd(4, Resources.Load<AudioClip>("Audios/Grass_dig1"));
        blockAudioDictionary.TryAdd(5, Resources.Load<AudioClip>("Audios/Stone_dig2"));
        blockAudioDictionary.TryAdd(6, Resources.Load<AudioClip>("Audios/Wood_dig1"));
        blockAudioDictionary.TryAdd(7, Resources.Load<AudioClip>("Audios/Wood_dig1"));
        blockAudioDictionary.TryAdd(8, Resources.Load<AudioClip>("Audios/Wood_dig1"));
        blockAudioDictionary.TryAdd(9, Resources.Load<AudioClip>("Audios/Grass_dig1"));
        blockAudioDictionary.TryAdd(10, Resources.Load<AudioClip>("Audios/Stone_dig2"));
        blockAudioDictionary.TryAdd(11, Resources.Load<AudioClip>("Audios/Sand_dig1"));
        blockAudioDictionary.TryAdd(12, Resources.Load<AudioClip>("Audios/Stone_dig2"));
        blockAudioDictionary.TryAdd(13, Resources.Load<AudioClip>("Audios/Stone_dig2"));
        blockAudioDictionary.TryAdd(100, Resources.Load<AudioClip>("Audios/Stone_dig2"));
        blockAudioDictionary.TryAdd(101, Resources.Load<AudioClip>("Audios/Grass_dig1"));
        blockAudioDictionary.TryAdd(102, Resources.Load<AudioClip>("Audios/Wood_dig1"));
        blockAudioDictionary.TryAdd(103, Resources.Load<AudioClip>("Audios/Wood_dig1"));
        blockAudioDictionary.TryAdd(104, Resources.Load<AudioClip>("Audios/Wood_dig1"));

        blockAudioDictionary.TryAdd(107, Resources.Load<AudioClip>("Audios/Stone_dig2"));
        blockAudioDictionary.TryAdd(108, Resources.Load<AudioClip>("Audios/Stone_dig2"));
        blockAudioDictionary.TryAdd(109, Resources.Load<AudioClip>("Audios/Stone_dig2"));
        blockAudioDictionary.TryAdd(110, Resources.Load<AudioClip>("Audios/Stone_dig2"));
        blockAudioDictionary.TryAdd(111, Resources.Load<AudioClip>("Audios/Stone_dig2"));
    }

    public AudioClip TryGetBlockAudioClip(int blockID)
    {
        if (blockAudioDictionary == null)
        {
            return null;
        }
        if (blockAudioDictionary.ContainsKey(blockID))
        {
            return blockAudioDictionary[blockID];
        }
        else
        {
            return null;
        }


    }
    public AudioClip TryGetEntityAudioClip(string clipName)
    {
        if (entityAudioDictionary == null)
        {
            return null;
        }

        if (entityAudioDictionary.ContainsKey(clipName))
        {
            return entityAudioDictionary[clipName];
        }
        else
        {
            return null;
        }


       



    }

    public void LoadDefaultEntityAudioResources()
    {
        entityAudioDictionary=new Dictionary<string, AudioClip>();
        entityAudioDictionary.TryAdd("zombieIdleClip", Resources.Load<AudioClip>("Audios/Zombie_say1"));
        entityAudioDictionary.TryAdd("zombieHurtClip", Resources.Load<AudioClip>("Audios/Zombie_hurt1"));


        entityAudioDictionary.TryAdd("skeletonIdleClip", Resources.Load<AudioClip>("Audios/Skeleton_say1"));
        entityAudioDictionary.TryAdd("skeletonShootClip", Resources.Load<AudioClip>("Audios/Bow_shoot"));
        entityAudioDictionary.TryAdd("skeletonHurtClip", Resources.Load<AudioClip>("Audios/Skeleton_hurt3"));


        entityAudioDictionary.TryAdd("creeperIdleClip", Resources.Load<AudioClip>("Audios/Creeper_say2"));
        entityAudioDictionary.TryAdd("creeperHurtClip", Resources.Load<AudioClip>("Audios/Creeper_say2"));
        entityAudioDictionary.TryAdd("entityExplodeClip", Resources.Load<AudioClip>("Audios/Explosion4"));


        entityAudioDictionary.TryAdd("endermanIdleClip", Resources.Load<AudioClip>("Audios/Enderman_idle1"));
        entityAudioDictionary.TryAdd("endermanHurtClip", Resources.Load<AudioClip>("Audios/Enderman_hit1"));

        entityAudioDictionary.TryAdd("entitySinkClip1", Resources.Load<AudioClip>("Audios/Entering_water"));
        entityAudioDictionary.TryAdd("entitySinkClip2", Resources.Load<AudioClip>("Audios/Exiting_water"));
        entityAudioDictionary.TryAdd("itemPopClip", Resources.Load<AudioClip>("Audios/Pop"));
        entityAudioDictionary.TryAdd("enderPortalTriggerClip", Resources.Load<AudioClip>("Audios/Nether_Portal_trigger"));

        entityAudioDictionary.TryAdd("equipDiamondClip", Resources.Load<AudioClip>("Audios/Equip_diamond2"));
        entityAudioDictionary.TryAdd("playerHurtClip", Resources.Load<AudioClip>("Audios/Player_hurt2"));
        entityAudioDictionary.TryAdd("playerEatClip", Resources.Load<AudioClip>("Audios/Drink"));
        entityAudioDictionary.TryAdd("playerSweepAttackClip", Resources.Load<AudioClip>("Audios/Sweep_attack1"));
        entityAudioDictionary.TryAdd("playerShootClip", Resources.Load<AudioClip>("Audios/Bow_shoot"));
    }
    public static void PlayClipAtPointCustomRollOff(AudioClip clip, Vector3 position,float minDist,float maxDist)
    {
        if (clip == null)
        {
            Debug.Log("missing file");
            return;
        }
        GameObject gameObject = new GameObject("One shot audio");
        gameObject.transform.position = position;
        AudioSource audioSource = (AudioSource)gameObject.AddComponent(typeof(AudioSource));
        audioSource.clip = clip;
        audioSource.spatialBlend = 1f;
        audioSource.volume = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.minDistance = minDist;
        audioSource.maxDistance = maxDist;
        audioSource.Play();
        Object.Destroy((Object)gameObject, clip.length * ((double)Time.timeScale < 0.0099999997764825821 ? 0.01f : Time.timeScale));
    }
}
