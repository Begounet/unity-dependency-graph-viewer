using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UDGV;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace UDGV.Tests
{
    public class DependencyCacheTest
    {
        [UnityTest]
        public IEnumerator BuildCache()
        {
            Debug.Log("Start building");

            DependencyCache cache = TestUtility.CreateDependencyCache();
            foreach (var operation in cache.BuildAsync())
            {
                yield return null;
                Debug.Log("Build running : " + operation.GetStatus());
            }

            Debug.Log("Build completed");

            cache.DumpCache();
        }

        [Test]
        public void Check_Mat0_HasDependencyOn_Tex0()
        {
            Material mat0 = TestUtility.GetMaterial(0);
            Texture tex0 = TestUtility.GetTexture(0);

            Check_ObjectA_HasDirectDependencyOn_ObjectB(mat0, tex0);
        }

        [Test]
        public void Check_Prefab0_HasDependencyOn_Mat0()
        {
            GameObject prefab0 = TestUtility.GetPrefab(0);
            Material mat0 = TestUtility.GetMaterial(0);

            Check_ObjectA_HasDirectDependencyOn_ObjectB(prefab0, mat0);
        }

        [Test]
        public void Check_Scene0_HasDependencyOn_Prefab0()
        {
            GameObject prefab0 = TestUtility.GetPrefab(0);
            SceneAsset scene0 = TestUtility.GetScene(0);

            Check_ObjectA_HasDirectDependencyOn_ObjectB(scene0, prefab0);
        }


        private void Check_ObjectA_HasDirectDependencyOn_ObjectB(UnityEngine.Object objectA, UnityEngine.Object objectB)
        {
            DependencyCache cache = TestUtility.CreateDependencyCache();
            cache.Build();

            string objectAGuid = TestUtility.GetObjectGUID(objectA);
            string objectBGuid = TestUtility.GetObjectGUID(objectB);

            Assert.IsTrue(cache.HasDirectDependencyOn(objectAGuid, objectBGuid));
            Assert.IsFalse(cache.HasDependencyOn(objectBGuid, objectAGuid));
        }
    }
}
