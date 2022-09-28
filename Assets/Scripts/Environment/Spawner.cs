using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class Spawner : MonoBehaviour
{
    [System.Serializable]
    public class Obj
    {
        public GameObject obj;
        public int spawnLimit = 0;
    }

    public float timeBetweenSpawns = 20;
    public float spawningDistance;
    List<Queue<GameObject>> objectPools;
    public Obj[] objects;
    public PlayerMovement player;
    public MapGenerator map;
    public GunBehaviour[] guns;

    float timeSinceLastSpawn = 0;
    void Start()
    {
        player.transform.position = map.playerSpawnPoint;
        foreach(GunBehaviour gun in guns)
        {
            gun.transform.position = map.playerSpawnPoint + Vector3.forward;
        }
        GameManager manager = FindObjectOfType<GameManager>();
        objectPools = new List<Queue<GameObject>>();
        foreach(Obj o in objects)
        {
            Queue<GameObject> oQueue = new Queue<GameObject>();
            for (int i = 0; i < o.spawnLimit; i++)
            {
                GameObject obj = Instantiate(o.obj);
                obj.SetActive(false);
                foreach(AudioSource step in obj.GetComponentsInChildren<AudioSource>())
                {
                    manager.sources.Add(step);
                }
                oQueue.Enqueue(obj);
            }
            objectPools.Add(oQueue);
        }
        manager.SetSFXVolume();
    }
    private void Update()
    {
        timeSinceLastSpawn += Time.deltaTime;
        if(timeSinceLastSpawn > timeBetweenSpawns)
        {
            timeSinceLastSpawn = 0;
            SpawnEnemy();
        }
    }
    void SpawnEnemy()
    {
        GameObject o = objectPools[0].Dequeue();
        if (o.GetComponent<EnemyAI>().spawnAvailable) {
            Vector3 position = player.transform.position + Random.insideUnitSphere * spawningDistance;
            NavMesh.SamplePosition(position, out NavMeshHit hit, spawningDistance, 1);
            o.transform.position = hit.position;
            o.SetActive(true);
        }
        objectPools[0].Enqueue(o);
    }
    public void Drop(Vector3 position)
    {
        int index = Random.Range(1, objectPools.Count);
        GameObject obj = objectPools[index].Dequeue();
        obj.transform.position = position;
        obj.GetComponent<PickupObject>().startPosition = position;
        obj.SetActive(true);
        objectPools[index].Enqueue(obj);
    }
}
