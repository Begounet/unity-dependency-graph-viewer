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

            AssertUtility.IsTextureReferencedByMaterial(tex0, mat0);

            DependencyCache cache = TestUtility.CreateDependencyCache();
            cache.Build();

            string mat0Guid = TestUtility.GetObjectGUID(mat0);
            string tex0Guid = TestUtility.GetObjectGUID(tex0);

            Assert.IsTrue(cache.TryGetDependencyDataForAsset(mat0Guid, out DependencyData mat0dd));
            Assert.IsTrue(cache.TryGetDependencyDataForAsset(tex0Guid, out DependencyData tex0dd));

            Assert.IsTrue(mat0dd.HasDependencyOn(tex0Guid));
            Assert.IsTrue(tex0dd.IsReferencedBy(mat0Guid));

            Assert.IsFalse(mat0dd.IsReferencedBy(tex0Guid));
            Assert.IsFalse(tex0dd.HasDependencyOn(mat0Guid));
        }
    }
}
