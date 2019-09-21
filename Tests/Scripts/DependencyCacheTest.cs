using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UDGV;
using UDGV.CacheSystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace UDGV.Tests
{
    public class DependencyCacheTest
    {
        [SetUp]
        public void Setup()
        {
            DependencyCacheManager.IsRunning = false;
        }

        [TearDown]
        public void Teardown()
        {
            DependencyCacheManager.IsRunning = true;
        }


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

        [Test]
        public void Check_Mat0_Only_HasDependencyOn_Tex0()
        {
            Material mat0 = TestUtility.GetMaterial(0);
            Texture tex0 = TestUtility.GetTexture(0);

            Check_ObjectA_Only_HasDirectDependencyOn_ObjectB(mat0, tex0);
        }

        [Test]
        public void Check_Prefab0_Only_HasDependencyOn_Mat0()
        {
            GameObject prefab0 = TestUtility.GetPrefab(0);
            Material mat0 = TestUtility.GetMaterial(0);

            Check_ObjectA_Only_HasDirectDependencyOn_ObjectB(prefab0, mat0);
        }

        [Test]
        public void Check_Scene0_Only_HasDependencyOn_Prefab0()
        {
            GameObject prefab0 = TestUtility.GetPrefab(0);
            SceneAsset scene0 = TestUtility.GetScene(0);

            Check_ObjectA_Only_HasDirectDependencyOn_ObjectB(scene0, prefab0);
        }

        [Test]
        public void Check_Rebuild_Mat0_DependencyOn_Tex0()
        {
            Material mat0 = TestUtility.GetMaterial(0);
            Texture tex0 = TestUtility.GetTexture(0);

            DependencyCache cache = Check_ObjectA_HasDirectDependencyOn_ObjectB(mat0, tex0);
            string mat0Guid = TestUtility.GetObjectGUID(mat0);
            string tex0Guid = TestUtility.GetObjectGUID(tex0);

            cache.Clear(TestUtility.GetMaterialPath(0) + ".mat");
            Assert.IsFalse(cache.HasDependencyOn(mat0Guid, tex0Guid)); // Dependency should be broken now

            cache.RebuildDependencies(mat0);
            Assert.IsTrue(cache.HasDependencyOn(mat0Guid, tex0Guid)); // Dependency should be fixed again
        }

        [Test]
        public void Serialization_Asset()
        {
            DependencyCache cache = TestUtility.CreateDependencyCache();

            Material mat0 = TestUtility.GetMaterial(0);
            cache.RebuildDependencies(mat0);

            string mat0Guid = TestUtility.GetObjectGUID(mat0);
            Assert.IsTrue(cache.IsAssetInCache(mat0Guid));

            cache.Save();
            cache.Clear();
            Assert.IsTrue(!cache.IsAssetInCache(mat0Guid), "The cache should be empty!");

            cache.Load();
            Assert.IsTrue(cache.IsAssetInCache(mat0Guid));
        }

        [Test]
        public void Serialization_AssetDependencies()
        {
            DependencyCache cache = TestUtility.CreateDependencyCache();

            Material mat0 = TestUtility.GetMaterial(0);
            Texture tex0 = TestUtility.GetTexture(0);
            cache.RebuildDependencies(mat0);

            string mat0Guid = TestUtility.GetObjectGUID(mat0);
            string tex0Guid = TestUtility.GetObjectGUID(tex0);

            Assert.IsTrue(cache.IsAssetInCache(mat0Guid));
            Assert.IsTrue(cache.IsAssetInCache(tex0Guid));
            Assert.IsTrue(cache.HasDirectDependencyOn(mat0Guid, tex0Guid));

            cache.Save();
            cache.Clear();
            Assert.IsTrue(!cache.IsAssetInCache(mat0Guid), "The cache should be empty!");

            cache.Load();
            Assert.IsTrue(cache.IsAssetInCache(mat0Guid));
            Assert.IsTrue(cache.HasDirectDependencyOn(mat0Guid, tex0Guid));
        }


        private DependencyCache Check_ObjectA_HasDirectDependencyOn_ObjectB(UnityEngine.Object objectA, UnityEngine.Object objectB, DependencyCache cache = null)
        {
            if (cache == null)
            {
                cache = TestUtility.CreateDependencyCache();
            }

            cache = TestUtility.CreateDependencyCache();
            cache.Build();

            string objectAGuid = TestUtility.GetObjectGUID(objectA);
            string objectBGuid = TestUtility.GetObjectGUID(objectB);

            Assert.IsTrue(cache.HasDirectDependencyOn(objectAGuid, objectBGuid));
            Assert.IsFalse(cache.HasDependencyOn(objectBGuid, objectAGuid));

            return cache;
        }

        private DependencyCache Check_ObjectA_Only_HasDirectDependencyOn_ObjectB(UnityEngine.Object objectA, UnityEngine.Object objectB, DependencyCache cache = null)
        {
            if (cache == null)
            {
                cache = TestUtility.CreateDependencyCache();
            }

            cache.RebuildDependencies(objectA);

            string objectAGuid = TestUtility.GetObjectGUID(objectA);
            string objectBGuid = TestUtility.GetObjectGUID(objectB);

            Assert.IsTrue(cache.HasDirectDependencyOn(objectAGuid, objectBGuid));
            Assert.IsFalse(cache.HasDependencyOn(objectBGuid, objectAGuid));

            return cache;
        }
    }
}
