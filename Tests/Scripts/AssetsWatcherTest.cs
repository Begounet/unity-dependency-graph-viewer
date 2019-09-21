using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UDGV.CacheSystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace UDGV.Tests
{
    public class AssetsWatcherTest
    {
        AssetsWatcher assetsWatcher;
        List<UnityEngine.Object> assetsToDeleteAtTeardown = new List<UnityEngine.Object>();

        [SetUp]
        public void Setup()
        {
            DependencyCacheManager.IsRunning = false;

            assetsWatcher = new AssetsWatcher();
        }

        [TearDown]
        public void Teardown()
        {
            for (int i = 0; i < assetsToDeleteAtTeardown.Count; ++i)
            {
                string path = AssetDatabase.GetAssetPath(assetsToDeleteAtTeardown[i]);
                AssetDatabase.DeleteAsset(path);
            }

            DependencyCacheManager.IsRunning = true;
        }

        [Test]
        public void Test_AssetModification()
        {
            bool isModificationDetected = false;
            string createdAssetFilePath = string.Empty;

            bool withExtension = true;
            string materialFilePath = TestUtility.GetMaterialPath(9999, withExtension);

            assetsWatcher.OnAssetChanged += (filename) =>
            {
                isModificationDetected = true;
                createdAssetFilePath = filename;
            };
            assetsWatcher.Start();

            Material srcMaterial = TestUtility.GetMaterial(0);
            Material newMaterial = new Material(srcMaterial);

            AssetDatabase.CreateAsset(newMaterial, materialFilePath);
            assetsToDeleteAtTeardown.Add(newMaterial);

            assetsWatcher.ForceUpdate();

            Assert.IsTrue(isModificationDetected);
            Assert.AreEqual(materialFilePath, createdAssetFilePath, $"Created asset path '{createdAssetFilePath}' should be '{materialFilePath}'");
        }

        [Test]
        public void Test_AssetDeleted()
        {
            bool isModificationDetected = false;
            string deletedAssetFilePath = string.Empty;

            bool withExtension = true;
            string materialFilePath = TestUtility.GetMaterialPath(9999, withExtension);

            assetsWatcher.OnAssetDeleted += (filename) =>
            {
                isModificationDetected = true;
                deletedAssetFilePath = filename;
            };
            assetsWatcher.Start();

            Material srcMaterial = TestUtility.GetMaterial(0);
            Material newMaterial = new Material(srcMaterial);

            AssetDatabase.CreateAsset(newMaterial, materialFilePath);
            AssetDatabase.DeleteAsset(materialFilePath);

            assetsWatcher.ForceUpdate();

            Assert.IsTrue(isModificationDetected);
            Assert.AreEqual(materialFilePath, deletedAssetFilePath, $"Deleted asset path '{deletedAssetFilePath}' should be '{materialFilePath}'");
        }
    }
}