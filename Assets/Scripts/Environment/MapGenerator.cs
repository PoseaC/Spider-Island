using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{   
    //for a better explanation watch Sebastian Lague's series on procedural landmass generation, this is just the Wish version of it

    [Header("Map Settings")]
    [Range(2,256)] public int size = 32; //length and width of the map in number of vertices
    public Gradient topology; //color of the map depending on the altitude
    public float shoreOffset = -30; //how much influence does the edge of the map have over the height of the topology
    public NavMeshSurface navMesh;
    public float waterLevel;

    //map seed
    public float offsetX = 0;
    public float offsetY = 0;
    public Vector3 playerSpawnPoint;

    [Header("Foliage")]
    public GameObject[] trees;
    public GameObject grass;
    public Vector2 treeGrowthRange;
    float deviationFromSpawn = 5f;
    [Range(0, 100)] public int treeDensity = 50;
    [Range(0, 10)] public int grassDensity = 5;

    [Header("Noise Settings")]
    [Range(.0001f, 100)] public float scale = 1; //how dense is the main layer of noise
    [Range(1, 10)] public int numberOfLayers = 1; //how many layers of noise
    [Range(0, 1)] public float persistence = .5f; //how much each subsequent layer impacts the overall noiseMap
    [Range(1, 10)] public float density = 2; //how dense is each subsequent layer of noise
    public float heightMultiplier = 1f; //amplitude of the noise map
    public AnimationCurve scaleInfluence; //how much the heightMultiplier impacts the map depending on the height

    //information to build the mesh
    Mesh mesh;
    int[] triangles;
    public MeshRenderer rend;
    Vector3[] heightMap;

    float maxHeight = float.MinValue; //highest point in the map
    float minHeight = float.MaxValue; //lowest point in the map

    void Awake()
    {
        //set a random seed for the map on start-up
        offsetX = Random.Range(-999, 999);
        offsetY = Random.Range(-999, 999);
        GenerateMap();
        gameObject.AddComponent<MeshCollider>();
        deviationFromSpawn = transform.localScale.x/2;
        GenerateFoliage(heightMap);
    }
    Vector3[] GenerateNoiseMap()
    {
        //create a vector of points for the map
        Vector3[] noiseMap = new Vector3[size * size]; 

        //variables needed to normalize the map
        float minHeight = float.MaxValue;
        float maxHeight = float.MinValue;

        for (int y = 0; y < size; y++)
        {
            //calculate the distance from the edge of the map for each point to affect it's height
            float distanceFromShoreZ = y <= size / 2? Mathf.InverseLerp(shoreOffset, size / 2, y) : Mathf.InverseLerp(size - shoreOffset, size / 2, y);

            for (int x = 0; x < size; x++)
            {
                float distanceFromShoreX = x <= size / 2? Mathf.InverseLerp(shoreOffset, size / 2, x) : Mathf.InverseLerp(size - shoreOffset, size / 2, x);

                float amplitude = 1; //amplitude affects the height of each point
                float frequency = 1; //frequency affects how close the points are together
                float currentHeight = 0;

                for (int i = 0; i < numberOfLayers; i++)
                {
                    //create the coordinates from which to sample the point, unity's perlin noise always returns the same value for the same coordinates which makes seeds really easy
                    float sampleX = (float) (x / scale * frequency) + offsetX;
                    float sampleY = (float) (y / scale * frequency) + offsetY;
                    float noise = Mathf.PerlinNoise(sampleX, sampleY); //sample a point from the perlin noise function
                    currentHeight += (float)noise * amplitude * distanceFromShoreX * distanceFromShoreZ; //and multiply it by these attributes

                    amplitude *= persistence; //persistence is >1 so amplitude decreases
                    frequency *= density; //while frequency increases
                }

                if (currentHeight > maxHeight)
                    maxHeight = currentHeight;
                else if (currentHeight < minHeight)
                    minHeight = currentHeight;

                noiseMap[x * size + y] = new Vector3(x, currentHeight, y);
            }
        }

        for (int j = 0; j < noiseMap.Length; j++)
        {
            //normalize the height 
            noiseMap[j].y = Mathf.InverseLerp(minHeight, maxHeight, noiseMap[j].y);
        }

        return noiseMap;
    }

    Vector3[] GenerateMesh(Vector3[] noiseMap)
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        List<Vector3> possibleSpawns = new List<Vector3>();

        //set mesh variables
        triangles = new int[(size-1) * (size-1) * 6];
        int triangleIndex = 0;
        
        for (int vertexIndex = 0; vertexIndex < noiseMap.Length; vertexIndex++)
        {
            //the noise map is between 0 and 1 so multiplied by 2 and subtracted 1 can give negative values as well
            noiseMap[vertexIndex].y = (noiseMap[vertexIndex].y * 2) - 1; 
            //the amplitude of each point is affected by the noise multiplier and the influence of it depending on the noise curve
            noiseMap[vertexIndex].y *= heightMultiplier * scaleInfluence.Evaluate(noiseMap[vertexIndex].y);

            if (noiseMap[vertexIndex].y * transform.localScale.x > waterLevel)
                possibleSpawns.Add(noiseMap[vertexIndex]);

            //needed to normalize the height for making the texture
            if (noiseMap[vertexIndex].y > maxHeight)
                maxHeight = noiseMap[vertexIndex].y;
            else if (noiseMap[vertexIndex].y < minHeight)
                minHeight = noiseMap[vertexIndex].y;

            //for the bottom and right edge of the matrices there are no more quads to set
            if (noiseMap[vertexIndex].x < size - 1 && noiseMap[vertexIndex].z < size - 1)
            { 
                //set one quad at a time

                //first half of the quad
                triangles[triangleIndex] = vertexIndex;
                triangles[triangleIndex + 1] = vertexIndex + size + 1;
                triangles[triangleIndex + 2] = vertexIndex + size;

                //second half of the quad
                triangles[triangleIndex + 3] = vertexIndex;
                triangles[triangleIndex + 4] = vertexIndex + 1;
                triangles[triangleIndex + 5] = vertexIndex + size + 1;

                triangleIndex += 6;
            }
        }
        playerSpawnPoint = possibleSpawns[Random.Range(0, possibleSpawns.Count)] * transform.localScale.x + Vector3.up;

        //feed the data to the mesh 
        mesh.Clear();
        mesh.vertices = noiseMap;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return noiseMap;
    }

    void GenerateTexture(Vector3[] heightMap)
    {
        //create a color array, texture and uvs
        Color[] colorMap = new Color[size * size];
        Texture2D texture = new Texture2D(size, size);
        Vector2[] uvs = new Vector2[heightMap.Length];

        for (int i = 0; i < heightMap.Length; i++)
        {
            //uvs are normalized so the values need to be divided by the size of the map
            uvs[i] = new Vector2(heightMap[i].z / size, heightMap[i].x / size);
            colorMap[i] = topology.Evaluate(Mathf.InverseLerp(minHeight, maxHeight, heightMap[i].y));
        }

        //feed the texture to the mesh
        mesh.uv = uvs;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Point;
        texture.SetPixels(colorMap);
        texture.Apply();
        rend.sharedMaterial.mainTexture = texture;
    }
    void GenerateFoliage(Vector3[] heightMap)
    {
        //spawn trees in a certain height range with a bit of lateral offset for diversity
        GameManager manager = FindObjectOfType<GameManager>();
        Vector3 mapScale = transform.localScale;
        for(int i = 0; i < heightMap.Length; i++)
        {
            if(heightMap[i].y > treeGrowthRange.x && heightMap[i].y < treeGrowthRange.y)
            {
                int spawnChance = Random.Range(1, 101);
                if(spawnChance < treeDensity)
                {
                    GameObject tree = trees[Random.Range(0, trees.Length)];
                    Vector3 treePosition = heightMap[i];
                    Vector2 spawnDeviation = Random.insideUnitCircle * deviationFromSpawn;

                    treePosition.x = treePosition.x * mapScale.x + spawnDeviation.x;
                    treePosition.y *= mapScale.y;
                    treePosition.z = treePosition.z * mapScale.z + spawnDeviation.y;

                    Physics.Raycast(treePosition + Vector3.up * deviationFromSpawn, Vector3.down, out RaycastHit hit, deviationFromSpawn * 2);

                    GameObject spawnedTree = Instantiate(tree, hit.point, Quaternion.Euler(0, Random.Range(0, 360), 0));
                    manager.foliage.Add(spawnedTree);
                }
                GenerateGrass(heightMap[i], manager);
            }
        }
        navMesh.BuildNavMesh();
        manager.SeparateChunks();
    }
    void GenerateGrass(Vector3 position, GameManager manager)
    {
        Vector3 mapScale = transform.localScale;
        for (int i = 0; i <= grassDensity; i++)
        {
            Vector3 grassPosition = position;
            Vector2 spawnDeviation = Random.insideUnitCircle * deviationFromSpawn;

            grassPosition.x = grassPosition.x * mapScale.x + spawnDeviation.x;
            grassPosition.y *= mapScale.y;
            grassPosition.z = grassPosition.z * mapScale.z + spawnDeviation.y;

            Physics.Raycast(grassPosition + Vector3.up * deviationFromSpawn, Vector3.down, out RaycastHit hit, deviationFromSpawn * 2);

            GameObject spawnedGrass = Instantiate(grass, hit.point, Quaternion.Euler(0, Random.Range(0, 360), 0));
            spawnedGrass.transform.up = hit.normal;
            manager.foliage.Add(spawnedGrass);
        }
    }
    public void GenerateMap()
    {
        Vector3[] noiseMap = GenerateNoiseMap();
        heightMap = GenerateMesh(noiseMap);
        GenerateTexture(heightMap);
    }
}
