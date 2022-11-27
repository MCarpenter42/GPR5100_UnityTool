using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.UIElements;
using TMPro;

using NeoCambion;
using NeoCambion.Collections;
using NeoCambion.Encryption;
using NeoCambion.Interpolation;
using NeoCambion.Maths;
using NeoCambion.Unity;
using UnityEditor.Compilation;

[System.Serializable]
public class StringWithPercent
{
    public string Str;
    public float FloatValue;
    public float Percent
    {
        get
        {
            return FloatValue * 100.0f;
        }
        set
        {
            if (value < 0.0f)
            {
                FloatValue = 0.0f;
            }
            else if (value > 100.0f)
            {
                FloatValue = 1.0f;
            }
            else
            {
                FloatValue = value / 100.0f;
            }
        }
    }

    public StringWithPercent()
    { }
    
    public StringWithPercent(string str, float percent)
    {
        Str = str;
        Percent = percent;
    }

    /*public ObjectWithPercent DrawAsProperty()
    {
        EditorGUILayout.BeginHorizontal();
        {
            Obj = EditorGUILayout.ObjectField(new GUIContent(), Obj, typeof(GameObject), true, GUILayout.MinWidth(100.0f)) as GameObject;
            GUILayout.Label(":", GUILayout.MinWidth(8.0f), GUILayout.MaxWidth(8.0f));
            Percent = EditorGUILayout.DelayedFloatField(Percent, GUILayout.MinWidth(45.0f), GUILayout.MaxWidth(45.0f));
            GUILayout.Label("%", GUILayout.MinWidth(15.0f), GUILayout.MaxWidth(15.0f));
        }
        EditorGUILayout.EndHorizontal();

        return this;
    }

    public ObjectWithPercent DrawAsProperty(string propertyName)
    {
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.PrefixLabel(propertyName);
            Obj = EditorGUILayout.ObjectField(new GUIContent(), Obj, typeof(GameObject), true, GUILayout.MinWidth(100.0f)) as GameObject;
            GUILayout.Label(":", GUILayout.MinWidth(8.0f), GUILayout.MaxWidth(8.0f));
            Percent = EditorGUILayout.DelayedFloatField(Percent, GUILayout.MinWidth(45.0f), GUILayout.MaxWidth(45.0f));
            GUILayout.Label("%", GUILayout.MinWidth(15.0f), GUILayout.MaxWidth(15.0f));
        }
        EditorGUILayout.EndHorizontal();

        return this;
    }*/
}

[System.Serializable]
public class ObjectWithPercent
{
    public GameObject Obj;
    public float FloatValue { get; private set; }
    public float Percent
    {
        get
        {
            return FloatValue * 100.0f;
        }
        set
        {
            if (value < 0.0f)
            {
                FloatValue = 0.0f;
            }
            else if (value > 100.0f)
            {
                FloatValue = 1.0f;
            }
            else
            {
                FloatValue = value / 100.0f;
            }
        }
    }

    public ObjectWithPercent()
    { }
    
    public ObjectWithPercent(GameObject obj, float percent)
    {
        Obj = obj;
        Percent = percent;
    }

    public ObjectWithPercent DrawAsProperty()
    {
        EditorGUILayout.BeginHorizontal();
        {
            Obj = EditorGUILayout.ObjectField(new GUIContent(), Obj, typeof(GameObject), true, GUILayout.MinWidth(100.0f)) as GameObject;
            GUILayout.Label(":", GUILayout.MinWidth(8.0f), GUILayout.MaxWidth(8.0f));
            Percent = EditorGUILayout.DelayedFloatField(Percent, GUILayout.MinWidth(45.0f), GUILayout.MaxWidth(45.0f));
            GUILayout.Label("%", GUILayout.MinWidth(15.0f), GUILayout.MaxWidth(15.0f));
        }
        EditorGUILayout.EndHorizontal();

        return this;
    }

    public ObjectWithPercent DrawAsProperty(string propertyName)
    {
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.PrefixLabel(propertyName);
            Obj = EditorGUILayout.ObjectField(new GUIContent(), Obj, typeof(GameObject), true, GUILayout.MinWidth(100.0f)) as GameObject;
            GUILayout.Label(":", GUILayout.MinWidth(8.0f), GUILayout.MaxWidth(8.0f));
            Percent = EditorGUILayout.DelayedFloatField(Percent, GUILayout.MinWidth(45.0f), GUILayout.MaxWidth(45.0f));
            GUILayout.Label("%", GUILayout.MinWidth(15.0f), GUILayout.MaxWidth(15.0f));
        }
        EditorGUILayout.EndHorizontal();

        return this;
    }
}

public class EditorList<T> : ScriptableObject
{
    public List<T> list = new List<T>();
    public bool shown;
    public int selectedItem;

    public EditorList(List<T> list, bool shown, int selectedItem)
    {
        this.list = list;
        this.shown = shown;
        if (shown)
        {
            this.selectedItem = selectedItem;
        }
        else
        {
            this.selectedItem = -1;
        }
    }
}

public static class EditorListDrawer
{
    public static EditorList<int> DrawList(this EditorList<int> editorList, string label)
    {
        List<int> list = editorList.list;
        int n = list.Count;
        bool shown = editorList.shown;
        int selectedItem = editorList.selectedItem;

        List<int> listOut = new List<int>();
        bool shownOut;

        EditorGUILayout.BeginVertical();

        EditorGUILayout.BeginHorizontal();
        shownOut = EditorGUILayout.Foldout(shown, " " + label, true, EditorStyles.foldout);
        int nOut = EditorGUILayout.DelayedIntField(n, GUILayout.MinWidth(40.0f), GUILayout.MaxWidth(40.0f));
        EditorGUILayout.EndHorizontal();
        if (nOut < 0)
        {
            nOut = 0;
        }

        for (int i = 0; i < nOut; i++)
        {
            if (i >= list.Count)
            {
                listOut.Add(default);
            }
            else
            {
                listOut.Add(list[i]);
            }
        }

        if (shown)
        {
            EditorGUILayout.Space(5.0f);

            EditorGUILayout.BeginVertical();

            if (listOut.Count > 0)
            {
                for (int i = 0; i < listOut.Count; i++)
                {
                    string itemLabel = "Element " + i;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.BeginVertical(EditorStyles.miniButton, GUILayout.MinWidth(70.0f), GUILayout.MaxWidth(90.0f));
                    if (i == selectedItem)
                    {
                        if (GUILayout.Button(itemLabel, EditorStyles.boldLabel))
                        {
                            selectedItem = i;
                        }
                    }
                    else
                    {
                        if (GUILayout.Button(itemLabel, EditorStyles.label))
                        {
                            selectedItem = i;
                        }
                    }
                    EditorGUILayout.EndVertical();
                    GUILayout.Label("  ", GUILayout.MinWidth(5.0f), GUILayout.MaxWidth(5.0f));
                    listOut[i] = EditorGUILayout.DelayedIntField(listOut[i]);
                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                EditorGUILayout.LabelField("No elements", "");
            }

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(100.0f));

            if (GUILayout.Button("+"))
            {
                listOut.Add(default);
            }

            if (GUILayout.Button("-"))
            {
                if (selectedItem > -1)
                {
                    listOut.RemoveAt(selectedItem);
                }
                else if (listOut.Count > 0)
                {
                    listOut.RemoveAt(listOut.Count - 1);
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }
        else
        {
            selectedItem = -1;
        }

        EditorGUILayout.EndVertical();

        return new EditorList<int>(listOut, shownOut, selectedItem);
    }

    public static EditorList<string> DrawList(this EditorList<string> editorList, string label)
    {
        List<string> list = editorList.list;
        int n = list.Count;
        bool shown = editorList.shown;
        int selectedItem = editorList.selectedItem;

        List<string> listOut = new List<string>();
        bool shownOut;

        EditorGUILayout.BeginVertical();

        EditorGUILayout.BeginHorizontal();
        shownOut = EditorGUILayout.Foldout(shown, " " + label, true, EditorStyles.foldout);
        int nOut = EditorGUILayout.DelayedIntField(n, GUILayout.MinWidth(40.0f), GUILayout.MaxWidth(40.0f));
        EditorGUILayout.EndHorizontal();
        if (nOut < 0)
        {
            nOut = 0;
        }

        for (int i = 0; i < nOut; i++)
        {
            if (i >= list.Count)
            {
                listOut.Add(default);
            }
            else
            {
                listOut.Add(list[i]);
            }
        }

        if (shown)
        {
            EditorGUILayout.Space(5.0f);

            EditorGUILayout.BeginVertical();

            if (listOut.Count > 0)
            {
                for (int i = 0; i < listOut.Count; i++)
                {
                    string itemLabel = "Element " + i;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.BeginVertical(EditorStyles.miniButton, GUILayout.MinWidth(70.0f), GUILayout.MaxWidth(90.0f));
                    if (i == selectedItem)
                    {
                        if (GUILayout.Button(itemLabel, EditorStyles.boldLabel))
                        {
                            selectedItem = i;
                        }
                    }
                    else
                    {
                        if (GUILayout.Button(itemLabel, EditorStyles.label))
                        {
                            selectedItem = i;
                        }
                    }
                    EditorGUILayout.EndVertical();
                    GUILayout.Label("  ", GUILayout.MinWidth(5.0f), GUILayout.MaxWidth(5.0f));
                    listOut[i] = EditorGUILayout.DelayedTextField(listOut[i]);
                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                EditorGUILayout.LabelField("No elements", "");
            }

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(100.0f));

            if (GUILayout.Button("+"))
            {
                listOut.Add(default);
            }

            if (GUILayout.Button("-"))
            {
                if (selectedItem > -1)
                {
                    listOut.RemoveAt(selectedItem);
                }
                else if (listOut.Count > 0)
                {
                    listOut.RemoveAt(listOut.Count - 1);
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }
        else
        {
            selectedItem = -1;
        }

        EditorGUILayout.EndVertical();

        return new EditorList<string>(listOut, shownOut, selectedItem);
    }

    public static EditorList<ObjectWithPercent> DrawList(this EditorList<ObjectWithPercent> editorList, GUIContent label)
    {
        List<ObjectWithPercent> list = editorList.list;
        int n = list.Count;
        bool shown = editorList.shown;
        int selectedItem = editorList.selectedItem;

        List<ObjectWithPercent> listOut = new List<ObjectWithPercent>();
        bool shownOut;

        EditorGUILayout.BeginVertical();

        EditorGUILayout.BeginHorizontal();
        shownOut = EditorGUILayout.Foldout(shown, label, true, EditorStyles.foldout);
        int nOut = EditorGUILayout.DelayedIntField(n, GUILayout.MinWidth(40.0f), GUILayout.MaxWidth(40.0f));
        EditorGUILayout.EndHorizontal();
        if (nOut < 0)
        {
            nOut = 0;
        }

        for (int i = 0; i < nOut; i++)
        {
            if (i >= list.Count)
            {
                listOut.Add(default);
            }
            else
            {
                listOut.Add(list[i]);
            }
        }

        if (shown)
        {
            EditorGUILayout.Space(5.0f);

            EditorGUILayout.BeginVertical();
            {
                if (listOut.Count > 0)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.BeginHorizontal(GUILayout.MinWidth(70.0f), GUILayout.MaxWidth(90.0f));
                            {
                                GUILayout.Label("List Position");
                            }
                            EditorGUILayout.EndHorizontal();
                            GUILayout.Label("  ", GUILayout.MinWidth(5.0f), GUILayout.MaxWidth(5.0f));
                            GUILayout.Label("Object", GUILayout.MinWidth(100.0f));
                            GUILayout.Label("Percentage", GUILayout.MinWidth(68.0f), GUILayout.MaxWidth(68.0f));
                            GUILayout.Label("", GUILayout.MinWidth(0.0f), GUILayout.MaxWidth(0.0f));
                        }
                        EditorGUILayout.EndHorizontal();

                        for (int i = 0; i < listOut.Count; i++)
                        {
                            string itemLabel = "Element " + i;
                            if (listOut[i] == null)
                            {
                                listOut[i] = new ObjectWithPercent();
                            }
                            EditorGUILayout.BeginHorizontal();
                            {
                                EditorGUILayout.BeginVertical(EditorStyles.miniButton, GUILayout.MinWidth(70.0f), GUILayout.MaxWidth(90.0f));
                                {
                                    if (i == selectedItem)
                                    {
                                        if (GUILayout.Button(itemLabel, EditorStyles.boldLabel))
                                        {
                                            selectedItem = i;
                                        }
                                    }
                                    else
                                    {
                                        if (GUILayout.Button(itemLabel, EditorStyles.label))
                                        {
                                            selectedItem = i;
                                        }
                                    }
                                }
                                EditorGUILayout.EndVertical();
                                GUILayout.Label("  ", GUILayout.MinWidth(5.0f), GUILayout.MaxWidth(5.0f));
                                listOut[i] = listOut[i].DrawAsProperty();
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                    EditorGUILayout.EndVertical();
                }
                else
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    {
                        EditorGUILayout.LabelField("No elements", "");
                    }
                    EditorGUILayout.EndVertical();
                }

                float totalPercent = 0.0f;
                foreach (ObjectWithPercent percObj in listOut)
                {
                    totalPercent += percObj.Percent;
                }
                if (totalPercent > 100.0f)
                {
                    float x = totalPercent / 100.0f;
                    for (int i = 0; i < listOut.Count; i++)
                    {
                        listOut[i].Percent /= x;
                    }
                }

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.BeginHorizontal();
                    { }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(100.0f));
                    {
                        if (GUILayout.Button("+"))
                        {
                            listOut.Add(default);
                        }

                        if (GUILayout.Button("-"))
                        {
                            if (selectedItem > -1)
                            {
                                listOut.RemoveAt(selectedItem);
                            }
                            else if (listOut.Count > 0)
                            {
                                listOut.RemoveAt(listOut.Count - 1);
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }
        
        if (!shown || listOut.Count == 0)
        {
            selectedItem = -1;
        }

        EditorGUILayout.EndVertical();

        return new EditorList<ObjectWithPercent>(listOut, shownOut, selectedItem);
    }
}
