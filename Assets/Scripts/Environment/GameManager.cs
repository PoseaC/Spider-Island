using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public int maximumHoles = 10;
    List<BulletHole> holes = new List<BulletHole>();

    [Header("Foliage")]
    [HideInInspector] public Transform player;
    public float chunkSize = 100;
    public float playerViewDistance = 500;
    [HideInInspector] public List<GameObject> foliage = new List<GameObject>();

    public List<AudioSource> sources = new List<AudioSource>();
    Dictionary<Vector3, List<GameObject>> chunks = new Dictionary<Vector3, List<GameObject>>();
    Vector3 activeChunk;
    List<Vector3> newChunks;
    List<Vector3> oldChunks;

    bool isSwitching = false;
    private void Awake()
    {
        activeChunk = Vector3.zero;
        newChunks = new List<Vector3>();
        oldChunks = new List<Vector3>();
        player = FindObjectOfType<PlayerMovement>().transform;
    }
    public void CheckBullets(BulletHole bullet)
    {
        holes.Add(bullet);
        if (holes.Count >= maximumHoles)
        {
            if (holes[0] != null)
            {
                Destroy(holes[0].gameObject);
                holes.RemoveAt(0);
            }
        }
    }
    public void SetSFXVolume()
    {
        foreach(AudioSource source in sources)
        {
            source.volume = PlayerPrefs.GetFloat("SFX", 50) / 100;
        }
    }
    private void Update()
    {
        Vector3 lastActiveChunk = activeChunk;

        foreach(Vector3 chunk in chunks.Keys)
        {
            if(Vector3.Distance(chunk, player.position) < playerViewDistance)
            {
                newChunks.Add(chunk);
                activeChunk = chunk;
            }
            else
            {
                oldChunks.Add(chunk);
            }
        }

        if (lastActiveChunk != activeChunk && !isSwitching)
            StartCoroutine(SwitchChunks(newChunks, oldChunks));

        newChunks.Clear();
        oldChunks.Clear();
    }
    IEnumerator SwitchChunks(List<Vector3> newChunks, List<Vector3> oldChunks)
    {
        isSwitching = true;
        foreach (Vector3 chunk in newChunks)
        {
            foreach (GameObject obj in chunks[chunk])
            {
                obj.SetActive(true);
            }
        }

        foreach (Vector3 chunk in oldChunks)
        {
            foreach (GameObject obj in chunks[chunk])
            {
                obj.SetActive(false);
            }
        }
        isSwitching = false;
        yield return null;
    }
    public void SeparateChunks()
    {
        Vector3 currentChunk = Vector3.zero;
        chunks.Add(currentChunk, new List<GameObject>());
        foreach(GameObject obj in foliage)
        {
            if(Vector3.Distance(obj.transform.position, currentChunk) < chunkSize)
            {
                chunks[currentChunk].Add(obj);
            }
            else
            {
                currentChunk = obj.transform.position;
                chunks.Add(currentChunk, new List<GameObject>());
            }
            obj.SetActive(false);
        }
    }
}
