using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UDGV.Tests
{
    internal static class TestUtility
    {
        private const string UnityDependencyGraphViewerDirectoryName = "unity-dependency-graph-viewer";
        private const string TestDirectoryName = "Tests";
        private const string TestAssetDirectoryName = "Assets";
        private const string TestMaterialsDirectoryName = "Materials";
        private const string TestTexturesDirectoryName = "Textures";
        private const string TestPrefabsDirectoryName = "Prefabs";
        private const string TestScenesDirectoryName = "Scenes";

        private const string TestMaterialPrefix = "Mat";
        private const string TestTexturePrefix = "Texture";
        private const string TestPrefabPrefix = "Prefab";
        private const string TestScenePrefix = "Scene";

        public static string TestDirectoryPath
        {
            get
            {
                string[] directories = AssetDatabase.FindAssets(UnityDependencyGraphViewerDirectoryName);
                Assert.IsNotEmpty(directories, $"Cannot find root plugin directory {UnityDependencyGraphViewerDirectoryName}");
                return (CombinePath(AssetDatabase.GUIDToAssetPath(directories[0]), TestDirectoryName));
            }
        }

        public static string TestAssetDirectoryPath     => CombinePath(TestDirectoryPath, TestAssetDirectoryName);

        public static string TestMaterialsDirectoryPath => CombinePath(TestAssetDirectoryPath, TestMaterialsDirectoryName);
        public static string TestTexturesDirectoryPath  => CombinePath(TestAssetDirectoryPath, TestTexturesDirectoryName);
        public static string TestPrefabDirectoryPath    => CombinePath(TestAssetDirectoryPath, TestPrefabsDirectoryName);
        public static string TestSceneDirectoryPath     => CombinePath(TestAssetDirectoryPath, TestScenesDirectoryName);


        public static string GetMaterialPath(int id)
        {
            return CombinePath(TestMaterialsDirectoryPath, TestMaterialPrefix + id);
        }

        public static string GetTexturePath(int id)
        {
            return CombinePath(TestTexturesDirectoryPath, TestTexturePrefix + id);
        }

        public static string GetPrefabPath(int id)
        {
            return CombinePath(TestPrefabDirectoryPath, TestPrefabPrefix + id);
        }

        public static string GetScenePath(int id)
        {
            return CombinePath(TestSceneDirectoryPath, TestScenePrefix + id);
        }


        public static Material GetMaterial(int id)
        {
            string path = GetAssetPath(TestMaterialsDirectoryPath, TestMaterialPrefix, id);
            return AssetDatabase.LoadAssetAtPath<Material>(path);
        }

        public static Texture GetTexture(int id)
        {
            string path = GetAssetPath(TestTexturesDirectoryPath, TestTexturePrefix, id);
            return AssetDatabase.LoadAssetAtPath<Texture>(path);
        }

        public static GameObject GetPrefab(int id)
        {
            string path = GetAssetPath(TestPrefabDirectoryPath, TestPrefabPrefix, id);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        public static SceneAsset GetScene(int id)
        {
            string path = GetAssetPath(TestSceneDirectoryPath, TestScenePrefix, id);
            return AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
        }

        public static bool IsTextureReferencedByMaterial(Texture texture, Material material)
        {
            string[] texturePropertyNames = material.GetTexturePropertyNames();
            for (int i = 0; i < texturePropertyNames.Length; ++i)
            {
                if (material.GetTexture(texturePropertyNames[i]) == texture)
                {
                    return true;
                }
            }
            return false;
        }

        public static DependencyCache CreateDependencyCache(DependencyViewerSettings settings = null)
        {
            if (settings == null)
            {
                settings = DependencyViewerSettings.Create();
                settings.Load();
            }
            return new DependencyCache(settings);
        }

        public static string GetObjectGUID(UnityEngine.Object obj)
        {
            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out string guid, out long localId))
            {
                return guid;
            }
            return string.Empty;
        }

        private static string CombinePath(string path1, string path2)
        {
            return path1 + "/" + path2;
        }

        private static string GetAssetPath(string directoryPath, string prefix, int id)
        {
            string[] assets = AssetDatabase.FindAssets(prefix + id, new string[] { directoryPath });
            Assert.IsNotEmpty(assets, $"Cannot find asset '{prefix}{id}' at path {directoryPath}'");
            return AssetDatabase.GUIDToAssetPath(assets[0]);
        }
        
        private static void Check_ObjectA_HasDirectDependencyOn_ObjectB(UnityEngine.Object objectA, UnityEngine.Object objectB)
        {
            GameObject prefab0 = TestUtility.GetPrefab(0);
            SceneAsset scene0 = TestUtility.GetScene(0);

            DependencyCache cache = TestUtility.CreateDependencyCache();
            cache.Build();

            string objectAGuid = TestUtility.GetObjectGUID(objectA);
            string objectBGuid = TestUtility.GetObjectGUID(objectB);

            Assert.IsTrue(cache.HasDirectDependencyOn(objectBGuid, objectAGuid));
            Assert.IsFalse(cache.HasDependencyOn(objectAGuid, objectBGuid));
        }
    }
}