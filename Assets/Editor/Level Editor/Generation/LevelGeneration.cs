using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEditor.Compilation;
using UnityEngine.SceneManagement;
using UnityEditor;
using TMPro;

using NeoCambion;
using NeoCambion.Collections;
using NeoCambion.Encryption;
using NeoCambion.Heightmaps;
using NeoCambion.Interpolation;
using NeoCambion.Maths;
using NeoCambion.Unity;

public class LevelGeneration : Core
{
	#region [ ENUMERATION TYPES ]

	public enum FloorType { Cuboid, Plane };

    #endregion

    #region [ OBJECTS ]

    public LevelGenData genData;
    public LevelData lvlData;

    #endregion

    #region [ PROPERTIES ]

    public bool dataReceived = false;

    HeightMap floorNoise;
    HeightMap elevation;
    HeightMap collectibleSpawning;

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */
	
    public void Generate(GameObject floorPrefab, FloorType floorType, WorldSpace worldSpace, Vector3Int levelSize, float levelUnitScale, float noiseScale, float noiseThreshold, float clcGenRate, List<ObjectWithPercent> collectibleTypeList)
    {
        ReceiveGenerationData(floorPrefab, floorType, worldSpace, levelSize, levelUnitScale, noiseScale, noiseThreshold, clcGenRate, collectibleTypeList);
        Generate();
    }
    
    public void Generate()
    {
        lvlData = new LevelData();
        GenerateFloor();
        GenerateCollectibles();
    }

    public void ReceiveGenerationData(GameObject floorPrefab, FloorType floorType, WorldSpace worldSpace, Vector3Int levelSize, float levelUnitScale, float noiseScale, float noiseThreshold, float clcGenRate, List<ObjectWithPercent> collectibleTypeList)
    {
        genData = new LevelGenData(floorPrefab, floorType, worldSpace, levelSize, levelUnitScale, noiseScale, noiseThreshold, clcGenRate, collectibleTypeList);
        dataReceived = true;
    }
    
    public void GenerateFloor()
    {
        if (dataReceived)
        {
            float offsetScale = 10.0f * genData.noiseScale;
            Vector2 offset = new Vector2(Random.Range(-offsetScale, offsetScale), Random.Range(-offsetScale, offsetScale));
            floorNoise = new HeightMap(genData.levelSize.x, genData.levelSize.z, genData.noiseScale, offset);
            float[,] elevation = new float[floorNoise.width, floorNoise.depth];
            for (int x = 0; x < floorNoise.width; x++)
            {
                for (int y = 0; y < floorNoise.depth; y++)
                {
                    float f;
                    float a = genData.noiseThreshold;
                    float b = 1.0f - genData.noiseThreshold;
                    if (floorNoise.Map[x, y] > a)
                    {
                        f = (floorNoise.Map[x, y] - a) / b;
                    }
                    else
                    {
                        f = 0.0f;
                    }
                    f *= (float)genData.levelSize.y;
                    elevation[x, y] = f;
                }
            }
            this.elevation = new HeightMap(elevation, true);
        }

        genData.worldSpace.Reset();

        GameObject mainFloor = Instantiate(genData.floorPrefab, genData.worldSpace.contTranform_Floor);
        Vector3 scale = mainFloor.transform.localScale;
        scale.x = (float)genData.levelSize.x * genData.levelUnitScale;
        scale.z = (float)genData.levelSize.z * genData.levelUnitScale;
        mainFloor.transform.localScale = scale;
        if (genData.floorType == FloorType.Cuboid)
        {
            float halfHeight = mainFloor.transform.localScale.y / 2.0f;
            mainFloor.transform.position = mainFloor.transform.position.AddToAxis(Axis.Y, -halfHeight);
        }

        for (int x = 0; x < genData.levelSize.x; x++)
        {
            for (int z = 0; z < genData.levelSize.z; z++)
            {
                float pointElevation = elevation.Map[x, z];
                if (pointElevation > 0.0f)
                {
                    GameObject raisedFloor = new GameObject();
                    raisedFloor.transform.SetParent(genData.worldSpace.contTranform_Floor);
                    Vector3 pos = new Vector3(-(mainFloor.transform.localScale.x / 2.0f) + (0.5f * genData.levelUnitScale), pointElevation, -(mainFloor.transform.localScale.z / 2.0f) + (0.5f * genData.levelUnitScale));
                    pos += new Vector3((float)x * genData.levelUnitScale, 0.0f, (float)z * genData.levelUnitScale);
                    raisedFloor.transform.localPosition = pos;
                    if (genData.floorType == FloorType.Cuboid)
                    {
                        GameObject floorObj = Instantiate(genData.floorPrefab, raisedFloor.transform);
                        floorObj.transform.localScale = new Vector3(genData.levelUnitScale, pointElevation, genData.levelUnitScale);
                        floorObj.transform.localPosition = new Vector3(0.0f, -(pointElevation / 2.0f), 0.0f);
                    }
                    else if (genData.floorType == FloorType.Plane)
                    {
                        GameObject floorObj = Instantiate(genData.floorPrefab, raisedFloor.transform);
                        floorObj.transform.localScale = new Vector3(genData.levelUnitScale, 1.0f, genData.levelUnitScale);
                        floorObj.transform.localPosition = Vector3.zero;

                        int n = (int)(pointElevation - (pointElevation % genData.levelUnitScale)) + 1;
                        if (z > genData.levelSize.z || elevation.Map[x, z + 1] < pointElevation)
                        {
                            GameObject wallPsZ = Instantiate(genData.floorPrefab, raisedFloor.transform);
                            wallPsZ.transform.localScale = new Vector3(genData.levelUnitScale, 1.0f, genData.levelUnitScale * (float)n);
                            wallPsZ.transform.localPosition = new Vector3(0.0f, -genData.levelUnitScale * (float)n, genData.levelUnitScale / 2.0f);
                            wallPsZ.transform.eulerAngles = new Vector3(90.0f, 0.0f, 0.0f);
                        }
                        if (x > genData.levelSize.x || elevation.Map[x + 1, z] < pointElevation)
                        {
                            GameObject wallPsX = Instantiate(genData.floorPrefab, raisedFloor.transform);
                            wallPsX.transform.localScale = new Vector3(genData.levelUnitScale * (float)n, 1.0f, genData.levelUnitScale);
                            wallPsX.transform.localPosition = new Vector3(genData.levelUnitScale / 2.0f, -genData.levelUnitScale * (float)n, 0.0f);
                            wallPsX.transform.eulerAngles = new Vector3(0.0f, 0.0f, -90.0f);
                        }
                        if (z < 1 || elevation.Map[x, z - 1] < pointElevation)
                        {
                            GameObject wallNgZ = Instantiate(genData.floorPrefab, raisedFloor.transform);
                            wallNgZ.transform.localScale = new Vector3(genData.levelUnitScale, 1.0f, genData.levelUnitScale * (float)n);
                            wallNgZ.transform.localPosition = new Vector3(0.0f, -genData.levelUnitScale * (float)n, -(genData.levelUnitScale / 2.0f));
                            wallNgZ.transform.eulerAngles = new Vector3(-90.0f, 0.0f, 0.0f);
                        }
                        if (x < 1 || elevation.Map[x - 1, z] < pointElevation)
                        {
                            GameObject wallNgX = Instantiate(genData.floorPrefab, raisedFloor.transform);
                            wallNgX.transform.localScale = new Vector3(genData.levelUnitScale * (float)n, 1.0f, genData.levelUnitScale);
                            wallNgX.transform.localPosition = new Vector3(-(genData.levelUnitScale / 2.0f), -genData.levelUnitScale * (float)n, 0.0f);
                            wallNgX.transform.eulerAngles = new Vector3(0.0f, 0.0f, 90.0f);
                        }
                    }
                }
            }
        }
    }

    public void GenerateCollectibles()
    {
        if (dataReceived)
        {
            float offsetScale = 10.0f * genData.noiseScale;
            Vector2 offset = new Vector2(Random.Range(-offsetScale, offsetScale), Random.Range(-offsetScale, offsetScale));
            collectibleSpawning = new HeightMap(genData.levelSize.x, genData.levelSize.z, genData.noiseScale, offset, HeightMapType.Random);
            for (int x = 0; x < collectibleSpawning.width; x++)
            {
                for (int z = 0; z < collectibleSpawning.depth; z++)
                {
                    if (InterpDelta.CosSlowDown(collectibleSpawning.Map[x, z]) < genData.clcGenRate)
                    {
                        float y = elevation.Map[x, z] + 0.5f;
                        Vector3 pos = new Vector3(-((float)genData.levelSize.x / 2.0f) + (0.5f * genData.levelUnitScale), y, -((float)genData.levelSize.z / 2.0f) + (0.5f * genData.levelUnitScale));
                        pos += new Vector3((float)x * genData.levelUnitScale, 0.0f, (float)z * genData.levelUnitScale);

                        GameObject chosen = PickCollectible();
                        if (chosen != null)
                        {
                            lvlData.collectibleSpawns.Add(pos, chosen);
                            GameObject collectible = Instantiate(chosen, genData.worldSpace.contTranform_Collectibles);
                            collectible.transform.localPosition = pos;
                        }
                    }
                }
            }
        }
    }

    public GameObject PickCollectible()
    {
        float r = Random.Range(0.0f, 1.0f);
        float totalThreshold = 0.0f;
        for (int i = 0; i < genData.collectibleTypeList.Count; i++)
        {
            totalThreshold += genData.collectibleTypeList[i].FloatValue;
            if (r < totalThreshold)
            {
                return genData.collectibleTypeList[i].Obj as GameObject;
            }
        }
        return null;
    }
}

[System.Serializable]
public class LevelGenData
{
    public GameObject floorPrefab;
	public LevelGeneration.FloorType floorType;
    public WorldSpace worldSpace;
    public Vector3Int levelSize;
    public float levelUnitScale;
    public float noiseScale;
    public float noiseThreshold;
    public float clcGenRate;
    public List<ObjectWithPercent> collectibleTypeList;

    public LevelGenData(GameObject floorPrefab, LevelGeneration.FloorType floorType, WorldSpace worldSpace, Vector3Int levelSize, float levelUnitScale, float noiseScale, float noiseThreshold, float clcGenRate, List<ObjectWithPercent> collectibleTypeList)
    {
        this.floorPrefab = floorPrefab;
        this.floorType = floorType;
        this.worldSpace = worldSpace;
        this.levelSize = levelSize;
        this.levelUnitScale = levelUnitScale;
        this.noiseScale = noiseScale;
        this.noiseThreshold = noiseThreshold;
        this.clcGenRate = clcGenRate;
        this.collectibleTypeList = collectibleTypeList;
    }
}

public class LevelData
{
    public Dictionary<Vector3, GameObject> collectibleSpawns = new Dictionary<Vector3, GameObject>();
}
