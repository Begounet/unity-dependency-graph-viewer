using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UDGV;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class DependencyCacheTest
    {
        [UnityTest]
        public IEnumerator BuildCache()
        {
            DependencyViewerSettings settings = DependencyViewerSettings.Create();
            settings.Load();
             
            Debug.Log("Start building");

            DependencyCache newCache = new DependencyCache(settings);
            foreach (var operation in newCache.Build())
            {
                yield return null;
                Debug.Log("Build running : " + operation.GetStatus());
            }

            Debug.Log("Build completed");

            newCache.DumpCache();
        }
    }
}
