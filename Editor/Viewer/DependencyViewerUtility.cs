using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

internal static class DependencyViewerUtility
{
    public static List<Scene> GetCurrentOpenedScenes()
    {
        List<Scene> scenes = new List<Scene>();
        for (int sceneIdx = 0; sceneIdx < SceneManager.sceneCount; ++sceneIdx)
        {
            Scene scene = SceneManager.GetSceneAt(sceneIdx);
            scenes.Add(scene);
        }
        return scenes;
    }

    public static void ForeachGameObjectInScenes(List<Scene> scenes, bool visitChildren, Action<GameObject> callback)
    {
        for (int i = 0; i < scenes.Count; ++i)
        {
            ForeachGameObjectInScene(scenes[i], visitChildren, callback);
        }
    }

    public static void ForeachGameObjectInScene(Scene scene, bool visitChildren, Action<GameObject> callback)
    {
        GameObject[] gameObjects = scene.GetRootGameObjects();
        for (int gameObjectIdx = 0; gameObjectIdx < gameObjects.Length; ++gameObjectIdx)
        {
            GameObject rootGameObject = gameObjects[gameObjectIdx];
            callback(rootGameObject);
            if (visitChildren)
            {
                UDGV.GameObjectUtility.ForeachChildrenGameObject(rootGameObject, callback);
            }
        }
    }


}
