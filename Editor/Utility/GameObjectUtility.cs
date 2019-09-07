using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UDGV
{
    internal static class GameObjectUtility
    {
        public static void ForeachChildrenGameObject(GameObject rootGameObject, Action<GameObject> callback)
        {
            Transform rootTransform = rootGameObject.transform;
            for (int i = 0; i < rootTransform.childCount; ++i)
            {
                Transform childTransform = rootTransform.GetChild(i);
                callback(childTransform.gameObject);
                ForeachChildrenGameObject(childTransform.gameObject, callback);
            }
        }
    }
}