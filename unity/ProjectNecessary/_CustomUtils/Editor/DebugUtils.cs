﻿using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// 用于生成新工程的必须文件夹以及导入VSCode
/// </summary>
public class DebugUtils : Editor
{

    #region Make Necessary Folders

    [MenuItem("Utils/Make Necessary Folders %#r")]
    public static void MakeFolders()
    {
        List<string> paths = FilterFolderPathList(GetNecessaryFolderPathList());
        for (int i = 0; i < paths.Count; i++)
        {
            Directory.CreateDirectory(paths[i]);
        }

        // Save Scene
        SaveMainScene();

        AssetDatabase.Refresh();
    }

    private static List<string> GetNecessaryFolderPathList()
    {
        List<string> list = new List<string>();
        list.Add(Application.dataPath + Path.DirectorySeparatorChar + "Editor");
        list.Add(Application.dataPath + Path.DirectorySeparatorChar + "Scenes");
        list.Add(Application.dataPath + Path.DirectorySeparatorChar + "Scripts");

        return list;
    }

    private static List<string> FilterFolderPathList(List<string> list)
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (AssetDatabase.IsValidFolder(list[i]))
            {
                list.RemoveAt(i);
            }
        }

        return list;
    }

    #endregion

    #region Import VSCode

    #endregion

    #region Open File In Finder

    [MenuItem("Utils/Open In Finder  %#f")]
    public static void OpenInFinder()
    {
        if (Selection.activeObject == null)
        {
            Process.Start(Application.dataPath);
            return;
        }

        string path = AssetDatabase.GetAssetPath(Selection.activeObject.GetInstanceID());
        if (!string.IsNullOrEmpty(path))
        {
            string filePath = Application.dataPath + Path.DirectorySeparatorChar + path.Replace("Assets/", "");
            if (Directory.Exists(filePath))
                Process.Start(filePath);
            else
                Process.Start(Path.GetDirectoryName(filePath));
        }
    }

    #endregion

    public static void SaveMainScene()
    {
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), Application.dataPath + Path.DirectorySeparatorChar + "Scenes" + Path.DirectorySeparatorChar + "Main.unity");
    }

#if NGUI
    [MenuItem("Utils/Open In Finder  %#d")]
    public static void AdjustDepth()
    {
        if (Selection.activeGameObject == null)
        {
            return;
        }

        UIPanel panel = Selection.activeGameObject.GetComponentInParent<UIPanel>();
        int depth = 0;  // Start from 0
        if (panel != null)
        {
            // Adjust Panel
            UIPanel[] panels = panel.GetComponentsInChildren<UIPanel>();
            IEnumerator enumerator = panels.GetEnumerator();
            while (enumerator.MoveNext())
            {
                UIPanel childPanel = enumerator.Current as UIPanel;
                childPanel.depth = depth++;
                childPanel.sortingOrder = childPanel.depth;
            }

            UIWidget[] widgets = panel.GetComponentsInChildren<UIWidget>();
            enumerator = widgets.GetEnumerator();
            while (enumerator.MoveNext())
            {
                UIWidget widget = enumerator.Current as UIWidget;
                widget.depth = depth++;
            }
        }
    }
#endif

    [MenuItem("Utils/Move GameObject Up Between Siblings &%UP")]
    public static bool MoveUpSibling()
    {
        if (Selection.activeGameObject == null)
        {
            return false;
        }

        int siblingIndex = Selection.activeGameObject.transform.GetSiblingIndex();
        if (siblingIndex > 0)
        {
            Selection.activeGameObject.transform.SetSiblingIndex(siblingIndex - 1);
            return true;
        }

        return false;
    }

    [MenuItem("Utils/Move GameObject Down Between Siblings &%DOWN")]
    public static bool MoveDownSibling()
    {
        if (Selection.activeGameObject == null)
        {
            return false;
        }

        int siblingIndex = Selection.activeGameObject.transform.GetSiblingIndex();
        Selection.activeGameObject.transform.SetSiblingIndex(siblingIndex + 1);

        return false;
    }

    [MenuItem("Utils/Move GameObject Up To Parent Siblings &UP")]
    public static bool MoveUpToParentSibling()
    {
        GameObject obj = Selection.activeGameObject;
        if (obj == null)
        {
            return false;
        }

        Transform transform = obj.transform;
        int siblingIndex = transform.GetSiblingIndex();
        if (siblingIndex > 0)
        {
            if (transform.parent != null)
            {
                Transform parentTransform = transform.parent;
                Transform siblingTransform = parentTransform.GetChild(siblingIndex - 1);
                Transform[] siblingChildren = siblingTransform.GetComponentsInChildren<Transform>();
                if (siblingChildren != null)
                {
                    Transform siblingLastChildTransform = siblingChildren[siblingChildren.Length - 1];
                    transform.parent = siblingLastChildTransform;
                    transform.Reset();
                    EditorGUIUtility.PingObject(Selection.activeGameObject);
                }
            }
            else
            {   // Root GameObject
                GameObject[] rootGameObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
                Transform parentSiblingTransform = rootGameObjects[siblingIndex - 1].transform;
                if (parentSiblingTransform != null) {
                    transform.parent = parentSiblingTransform;
                    transform.Reset();
					EditorGUIUtility.PingObject(Selection.activeGameObject);
                }
            }
        }
        else
        {
            if (transform.parent == null)
                return false;

            int parentSiblingIndex = transform.parent.GetSiblingIndex();
            transform.parent = transform.parent.parent;
            transform.SetSiblingIndex(parentSiblingIndex);
            transform.Reset();

            EditorGUIUtility.PingObject(Selection.activeGameObject);
        }

        return false;
    }



    [MenuItem("Utils/Move GameObject Down To Parent Siblings &DOWN")]
    public static bool MoveDownToParentSibling()
    {
        GameObject obj = Selection.activeGameObject;
        if (obj == null)
        {
            return false;
        }

        Transform transform = obj.transform;
        int siblingIndex = transform.GetSiblingIndex();
        int maxSiblingIndex = 0;
        GameObject[] rootGameObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        if (transform.parent == null)
        {
            maxSiblingIndex = rootGameObjects.Length - 1;
        }
        else
        {
            maxSiblingIndex = transform.parent.childCount - 1;
        }

        if (siblingIndex < maxSiblingIndex)
        {
            if (transform.parent != null)
            {
                Transform parentTransform = transform.parent;
                Transform siblingTransform = parentTransform.GetChild(siblingIndex + 1);
                transform.parent = siblingTransform;
                transform.Reset();
                EditorGUIUtility.PingObject(Selection.activeGameObject);
            }
            else
            {	// Root GameObject
                Transform parentSiblingTransform = rootGameObjects[siblingIndex + 1].transform;
                Transform parentSiblingFirstChildTransform = parentSiblingTransform;
                transform.parent = parentSiblingFirstChildTransform;
                transform.SetAsFirstSibling();
                transform.Reset();
				EditorGUIUtility.PingObject(Selection.activeGameObject);
            }
        }
        else
        {
            if (transform.parent == null)
                return false;

            int parentSiblingIndex = transform.parent.GetSiblingIndex();
            transform.parent = transform.parent.parent;
            transform.SetSiblingIndex(parentSiblingIndex + 1);
            transform.Reset();

            EditorGUIUtility.PingObject(Selection.activeGameObject);
        }

        return false;
    }

}

public static class TransformExtension {
    public static void Reset (this Transform transform) {
        transform.localPosition = Vector3.one;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }
}