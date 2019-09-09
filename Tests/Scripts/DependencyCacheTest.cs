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

            AssertUtility.IsObjectDirectlyReferencingOtherObject(tex0, mat0);

            DependencyCache cache = TestUtility.CreateDependencyCache();
            cache.Build();

            string mat0Guid = TestUtility.GetObjectGUID(mat0);
            string tex0Guid = TestUtility.GetObjectGUID(tex0);

            Assert.IsTrue(cache.HasDirectDependencyOn(mat0Guid, tex0Guid));
            Assert.IsFalse(cache.HasDependencyOn(tex0Guid, mat0Guid));
        }

        [Test]
        public void Check_Prefab0_HasDependencyOn_Mat0()
        {
            GameObject prefab0 = TestUtility.GetPrefab(0);
            Material mat0 = TestUtility.GetMaterial(0);

            DependencyCache cache = TestUtility.CreateDependencyCache();
            cache.Build();

            string mat0Guid = TestUtility.GetObjectGUID(mat0);
            string prefab0Guid = TestUtility.GetObjectGUID(prefab0);

            Assert.IsTrue(cache.HasDirectDependencyOn(prefab0Guid, mat0Guid));
            Assert.IsFalse(cache.HasDependencyOn(mat0Guid, prefab0Guid));
        }

        [Test]
        public void Check_Scene0_HasDependencyOn_Prefab0()
        {
            GameObject prefab0 = TestUtility.GetPrefab(0);
            SceneAsset scene0 = TestUtility.GetScene(0);

            DependencyCache cache = TestUtility.CreateDependencyCache();
            cache.Build();

            string prefab0Guid = TestUtility.GetObjectGUID(prefab0);
            string scene0Guid = TestUtility.GetObjectGUID(scene0);

            Assert.IsTrue(cache.HasDirectDependencyOn(scene0Guid, prefab0Guid));
            Assert.IsFalse(cache.HasDependencyOn(prefab0Guid, scene0Guid));
        }
    }
}
