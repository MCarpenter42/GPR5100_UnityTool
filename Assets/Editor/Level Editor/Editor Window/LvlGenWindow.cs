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
using NeoCambion.Interpolation;
using NeoCambion.Maths;
using NeoCambion.Unity;
using UnityEditor.Compilation;

public class LvlGenWindow : EditorWindow
{
    #region [ OBJECTS ]

    MonoScript gameManagerClass;
    Canvas uiPrefab;

    GameObject floor;
    LevelGeneration.FloorType floorType;

    GameObject gameManager;
    WorldSpace worldSpace;
    GameObject ui;

    List<GameObject> sceneCameras = new List<GameObject>();
    List<GameObject> previewCameras = new List<GameObject>();
    GameObject container_prevCams;
    int activePreviewCam = -1;

    #endregion

    #region [ PROPERTIES ]

    bool showCoreObjects = true;
    bool showStructureObjects = true;
    bool showLevelSettings = true;
    bool showCollectibleSettings = true;
    bool showCameraSelector = true;

    Vector3Int levelSize = new Vector3Int(5, 1, 5);
    float levelUnitScale = 1.0f;
    float noiseScale = 1.0f;
    float noiseThreshold = 0.5f;

    private float clcGenRateValue = 0.2f;
    public float clcGenRatePercent
    {
        get
        {
            return clcGenRateValue * 100.0f;
        }
        set
        {
            if (value < 0.0f)
            {
                clcGenRateValue = 0.0f;
            }
            else if (value > 100.0f)
            {
                clcGenRateValue = 1.0f;
            }
            else
            {
                clcGenRateValue = value / 100.0f;
            }
        }
    }

    List<ObjectWithPercent> collectibleTypeList = new List<ObjectWithPercent>();
    bool showCollectibleList = false;
    int collectibleTypeListSelected = -1;

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    #region [ BUILT-IN UNITY FUNCTIONS ]

    private void OnGUI()
    {
        GUI.enabled = true;
        GUIContent label = new GUIContent();

        EditorGUILayout.BeginVertical(EditorStyles.inspectorFullWidthMargins);
        {
            EditorGUILayout.Space(5.0f);

            label.text = "Core Objects";
            label.tooltip = null;
            showCoreObjects = EditorGUILayout.Foldout(showCoreObjects, label, true, EditorStyles.foldoutHeader);
            if (showCoreObjects)
            {
                EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
                {
                    label.text = "GameManager Script";
                    label.tooltip = "A script containing a GameManager singleton class";
                    gameManagerClass = EditorGUILayout.ObjectField(label, gameManagerClass, typeof(MonoScript), true) as MonoScript;

                    label.text = "Level UI Prefab";
                    label.tooltip = "Canvas prefab for level UI";
                    uiPrefab = EditorGUILayout.ObjectField(label, uiPrefab, typeof(Canvas), true) as Canvas;
                }
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space(10.0f);

            if (gameManagerClass == null)
            {
                GUI.enabled = false;
                label.text = "GameManager prefab required!";
                label.tooltip = "Convert the current scene to use the level generator setup";
            }
            else
            {
                label.text = "Convert Current Scene";
                label.tooltip = "Convert the current scene to use the level generator setup";
            }
            if (GUILayout.Button(label))
            {
                ConvertScene();
            }
            GUI.enabled = true;

            EditorGUILayout.Space(10.0f);

            label.text = "Structure Objects";
            label.tooltip = null;
            showStructureObjects = EditorGUILayout.Foldout(showStructureObjects, label, true, EditorStyles.foldoutHeader);
            if (showStructureObjects)
            {
                EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
                {
                    label.text = "Floor Type";
                    label.tooltip = "The type of object to be used to create the level floor - this will determine how the level editor assembles the floor";
                    floorType = (LevelGeneration.FloorType)EditorGUILayout.EnumPopup(label, floorType);

                    if (floorType == LevelGeneration.FloorType.Cuboid)
                    {
                        label.text = "Floor Prefab (Cuboid)";
                        label.tooltip = "A cube-based GameObject prefab for the level floor";
                    }
                    else
                    {
                        label.text = "Floor Prefab (Plane)";
                        label.tooltip = "A plane-based GameObject prefab for the level floor";
                    }
                    floor = EditorGUILayout.ObjectField(label, floor, typeof(GameObject), true) as GameObject;
                }
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space(10.0f);

            label.text = "Level Structure Settings";
            label.tooltip = null;
            showLevelSettings = EditorGUILayout.Foldout(showLevelSettings, label, true, EditorStyles.foldoutHeader);
            if (showLevelSettings)
            {
                EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.BeginVertical();
                        {
                            label.text = "Level Width (X)";
                            label.tooltip = "Width of the level on the X axis";
                            GUILayout.Label(label);

                            levelSize.x = EditorGUILayout.DelayedIntField(levelSize.x);
                            if (levelSize.x < 1)
                            {
                                levelSize.x = 1;
                            }
                        }
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.BeginVertical();
                        {
                            label.text = "Level Width (Z)";
                            label.tooltip = "Width of the level on the Z axis";
                            GUILayout.Label(label);

                            levelSize.z = EditorGUILayout.DelayedIntField(levelSize.z);
                            if (levelSize.z < 1)
                            {
                                levelSize.z = 1;
                            }
                        }
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    {
                        levelUnitScale = EditorGUILayout.Slider("Level Unit Scale", levelUnitScale, 0.25f, 5.0f);
                        int intLUS = (int)(levelUnitScale * 4.0f);
                        levelUnitScale = (float)intLUS / 4.0f;
                    }
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.BeginHorizontal();
                    {
                        levelSize.y = EditorGUILayout.IntSlider("Max Level Height", levelSize.y, 1, 5);
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    {
                        noiseScale = EditorGUILayout.Slider("Generation Noise Scale", noiseScale, 1.0f, 6.0f);
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    {
                        noiseThreshold = EditorGUILayout.Slider("Generation Noise Threshold", noiseThreshold, 0.0f, 0.6f);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space(10.0f);

            label.text = "Collectibles Settings";
            label.tooltip = null;
            showCollectibleSettings = EditorGUILayout.Foldout(showCollectibleSettings, label, true, EditorStyles.foldoutHeader);
            if (showCollectibleSettings)
            {
                EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
                {
                    GUILayout.Label("Collectible Spawn Point Generation Rate");

                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("", GUILayout.MinWidth(2.0f), GUILayout.MaxWidth(2.0f));
                        clcGenRatePercent = EditorGUILayout.Slider(clcGenRatePercent, 0.0f, 100.0f);
                        GUILayout.Label("%", GUILayout.MinWidth(15.0f), GUILayout.MaxWidth(15.0f));
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space(4.0f);

                    EditorList<ObjectWithPercent> editorList = new EditorList<ObjectWithPercent>(collectibleTypeList, showCollectibleList, collectibleTypeListSelected);
                    editorList = editorList.DrawList(new GUIContent("Collectibles", "A list of potential collectibles to spawn, along with their corresponding spawn chance. If the total spawn chance across all collectibles is 100%, a collectible will always spawn at a given node. Total spawn chance can never exceed 100%."));
                    collectibleTypeList = editorList.list;
                    showCollectibleList = editorList.shown;
                    collectibleTypeListSelected = editorList.selectedItem;
                }
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space(10.0f);

            if (floor == null)
            {
                GUI.enabled = false;
                label.text = "Floor prefab required!";
                label.tooltip = "Generate new level setup using current settings";
            }
            else
            {
                label.text = "Generate Level";
                label.tooltip = "Generate new level setup using current settings";
            }
            if (GUILayout.Button(label))
            {
                Generate();
            }
            GUI.enabled = true;

            label.text = "Preview Cameras";
            label.tooltip = null;
            showCameraSelector = EditorGUILayout.Foldout(showCameraSelector, label, true, EditorStyles.foldoutHeader);
            if (showCameraSelector)
            {
                GetPreviewCameras(FindObjectsOfType<Camera>());
                EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            string activeCam;
                            if (activePreviewCam > -1)
                            {
                                activeCam = (activePreviewCam + 1).ToString();
                            }
                            else
                            {
                                activeCam = "None";
                            }
                            GUILayout.Label("Active Preview Camera: " + activeCam, EditorStyles.boldLabel);
                        }
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.Space(4.0f);

                        if (previewCameras.Count > 0)
                        {
                            for (int i = 0; i < previewCameras.Count; i++)
                            {
                                EditorGUILayout.BeginHorizontal();
                                {
                                    EditorGUILayout.PrefixLabel("Preview Camera " + (i + 1));
                                    string btnLabel;
                                    if (activePreviewCam == i)
                                    {
                                        btnLabel = "Stop Viewing";
                                    }
                                    else
                                    {
                                        btnLabel = "View";
                                    }
                                    if (GUILayout.Button(btnLabel))
                                    {
                                        if (activePreviewCam == i)
                                        {
                                            PickPreviewCamera(-1);
                                        }
                                        else
                                        {
                                            PickPreviewCamera(i);
                                        }
                                    }
                                }
                                EditorGUILayout.EndHorizontal();
                            }
                        }
                        else
                        {
                            GUILayout.Label("(No preview cameras available)");
                        }
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndVertical();
            }
            else
            {
                PickPreviewCamera(-1);
            }
        }
        EditorGUILayout.EndVertical();
    }

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    [MenuItem("Window/Level Generator")]
    public static void ShowWindow()
    {
        EditorWindow window = GetWindow(typeof(LvlGenWindow));
        window.titleContent = new GUIContent("Level Generator");
    }

    public void ConvertScene()
    {
        if (gameManagerClass != null)
        {
            if (FindObjectOfType(gameManagerClass.GetClass()) == null)
            {
                gameManager = new GameObject();
                gameManager.name = "GameManager";
                if (gameManagerClass != null)
                {
                    gameManager.AddComponent(gameManagerClass.GetClass());
                }
                else
                {
                    Debug.LogError("GameManager script could not be found!");
                }
                gameManager.AddComponent<EventSystem>();
                gameManager.AddComponent<StandaloneInputModule>();
            }
            else
            {
                gameManager = FindObjectOfType(gameManagerClass.GetClass()) as GameObject;
            }
        }

        foreach (Transform trn in FindObjectsOfType<Transform>())
        {
            if (worldSpace == null && trn.gameObject.name == "World Space")
            {
                worldSpace = trn.gameObject.GetOrAddComponent<WorldSpace>();
            }
            if (container_prevCams == null && trn.gameObject.name == "Preview Camera Container")
            {
                container_prevCams = trn.gameObject;
            }
        }
        foreach (Canvas cnv in FindObjectsOfType<Canvas>())
        {
            if (ui == null && cnv.gameObject.name == "UI")
            {
                ui = cnv.gameObject;
            }
        }

        if (worldSpace == null)
        {
            GameObject worldSpaceObj = new GameObject("World Space");
            worldSpace = worldSpaceObj.AddComponent<WorldSpace>();
        }

        if (ui == null)
        {
            if (uiPrefab == null)
            {
                GameObject uiObj = new GameObject("UI");
                RectTransform ui_R = uiObj.GetOrAddComponent<RectTransform>();
                Canvas ui_C = uiObj.GetOrAddComponent<Canvas>();
                ui_C.renderMode = RenderMode.ScreenSpaceOverlay;
                CanvasScaler ui_CS = uiObj.GetOrAddComponent<CanvasScaler>();
                ui_CS.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                ui_CS.referenceResolution = new Vector2(1920, 1080);
                GraphicRaycaster ui_GR = uiObj.GetOrAddComponent<GraphicRaycaster>();
                ui = uiObj;
            }
            else
            {
                ui = Instantiate(uiPrefab).gameObject;
            }
        }

        if (container_prevCams == null)
        {
            container_prevCams = new GameObject("Preview Camera Container");
        }

        Camera[] cameras = FindObjectsOfType<Camera>();
        if (cameras.Length == 0)
        {
            cameras = new Camera[1];
            GameObject cameraObj = new GameObject("Main Camera");
            cameras[0] = cameraObj.AddComponent<Camera>();
            cameras[0].gameObject.tag = "MainCamera";
            cameraObj.transform.position = new Vector3(0.0f, 1.0f -10.0f);
        }
        if (cameras.Length == 1)
        {
            if (cameras[0].transform.parent == null)
            {
                cameras[0].transform.SetParent(worldSpace.transform);
            }
        }
        GetPreviewCameras(cameras);

        Light[] lights = FindObjectsOfType<Light>();
        if (lights.Length == 0)
        {
            lights = new Light[1];
            lights[0] = new Light();
            lights[0].type = LightType.Directional;
            lights[0].color = new Color(1.0f, 0.9568627f, 0.8392157f, 1.0f);
        }
        if (lights.Length == 1)
        {
            if (lights[0].transform.parent == null)
            {
                lights[0].transform.SetParent(worldSpace.transform);
            }
        }
    }

    public void GetPreviewCameras(Camera[] cameras)
    {
        previewCameras.Clear();
        Transform[] trns = container_prevCams.transform.GetChildren();
        foreach (Transform trn in trns)
        {
            if (trn.gameObject.HasComponent<Camera>())
            {
                previewCameras.Add(trn.gameObject);
            }
        }
    }

    public void PickPreviewCamera(int index)
    {
        if (index.InBounds(previewCameras))
        {
            foreach (GameObject obj in sceneCameras)
            {
                obj.SetActive(false);
            }
            foreach (GameObject obj in previewCameras)
            {
                obj.SetActive(false);
            }
            previewCameras[index].SetActive(true);
            activePreviewCam = index;
        }
        else
        {
            foreach (GameObject obj in sceneCameras)
            {
                obj.SetActive(false);
            }
            foreach (GameObject obj in previewCameras)
            {
                obj.SetActive(false);
            }
            GameObject.FindGameObjectWithTag("MainCamera").SetActive(true);
            activePreviewCam = -1;
        }
    }

    public void Generate()
    {
        LevelGeneration generator = new LevelGeneration();
        generator.ReceiveGenerationData(floor, floorType, worldSpace, levelSize, levelUnitScale, noiseScale, noiseThreshold, clcGenRateValue, collectibleTypeList);
        generator.Generate();
    }
}

public class CollectibleListItem
{
    public GameObject prefab;
    public float spawnChance;
}
