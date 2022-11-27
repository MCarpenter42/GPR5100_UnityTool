using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
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

public class FileHandling : Core
{
    public static string CacheFolder { get { return Application.dataPath + "/Editor/SaveData/"; } }
    public static string CacheFile { get { return "_CachedGenData.json"; } }

    public static void SaveGenerationCache(LevelGenData data)
    {
#if UNITY_EDITOR
        if (!AssetDatabase.IsValidFolder("Assets/Editor/SaveData"))
        {
            AssetDatabase.CreateFolder("Assets/Editor", "SaveData");
        }
#endif
        string jsonString = JsonUtility.ToJson(data);
        string path = CacheFolder + CacheFile;
        File.WriteAllText(path, jsonString);
    }

    public static LevelGenData LoadGenerationCache()
    {
        LevelGenData data = null;

        string path = CacheFolder + CacheFile;
        if (File.Exists(path))
        {
            string jsonString = File.ReadAllText(path);
            data = JsonUtility.FromJson<LevelGenData>(jsonString);
        }

        return data;
    }

    public static void SaveGenerationPreset(LevelGenData data)
    {
        Debug.Log(CacheFolder + CacheFile);

#if UNITY_EDITOR
        if (!AssetDatabase.IsValidFolder("Assets/Editor/SaveData"))
        {
            AssetDatabase.CreateFolder("Assets/Editor", "SaveData");
        }
#endif
        string jsonString = JsonUtility.ToJson(data);
        string path = EditorUtility.SaveFilePanel("Save level generation data", CacheFolder, "", "json");
        if (!path.IsEmptyOrNullOrWhiteSpace())
        {
            if (path != (CacheFolder + CacheFile))
            {
                File.WriteAllText(path, jsonString);
            }
            else
            {
                Debug.LogError("Unable to overwrite the data cache!");
            }
        }
    }

    public static LevelGenData LoadGenerationPreset()
    {
        LevelGenData data = null;

        string path = EditorUtility.OpenFilePanel("Load level generation data", Application.dataPath, "json");

        if (!path.IsEmptyOrNullOrWhiteSpace())
        {
            string jsonString = System.IO.File.ReadAllText(path);

            data = JsonUtility.FromJson<LevelGenData>(jsonString);
        }
        else
        {
            Debug.LogError("Invalid filepath!");
        }

        return data;
    }


}
