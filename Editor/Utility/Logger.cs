using UnityEngine;
using System.Collections;
using UDGV.CacheSystem;

namespace UDGV.Utility
{
    public static class Logger
    {
        public static void Log(string message)
        {
            if (DependencyCacheManager.Settings.Developer.IsVerbose)
            {
                Debug.Log(message);
            }
        }
    }
}
