using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace UDGV.CacheSystem
{
    public class DependencyData
    {
        public string objectGuid;
        public long localId;
        public string sceneOwnerGuid;

        // Debug
        public string PrettyName => AssetDatabase.GUIDToAssetPath(objectGuid);
        public string[] PrettyDependencies => ArrayOfGuidToPaths(Dependencies);
        public string[] PrettyReferences => ArrayOfGuidToPaths(References);

        public HashSet<string> Dependencies { get; private set; } = new HashSet<string>();
        public HashSet<string> References { get; private set; } = new HashSet<string>();


        internal static void Connect(DependencyData reference, DependencyData dependency)
        {
            reference.Dependencies.Add(dependency.objectGuid);
            dependency.References.Add(reference.objectGuid);
        }

        internal static void Disconnect(DependencyData reference, DependencyData dependency)
        {
            reference.Dependencies.Remove(dependency.objectGuid);
            dependency.References.Remove(reference.objectGuid);
        }

        internal void DisconnectAllDependencies(DependencyCacheDataHandler dataHandler)
        {
            DependencyData dependencyData = null;

            // Use a copy because the References property will be changed during the iteration
            HashSet<string> dependenciesCopy = new HashSet<string>(Dependencies);
            var it = dependenciesCopy.GetEnumerator();
            while (it.MoveNext())
            {
                if (dataHandler.TryGetValue(it.Current, out dependencyData))
                {
                    Disconnect(this, dependencyData);
                }
            }
        }

        internal void DisconnectFromAllReferences(DependencyCacheDataHandler dataHandler)
        {
            DependencyData dependencyData = null;

            // Use a copy because the References property will be changed during the iteration
            HashSet<string> referencesCopy = new HashSet<string>(References);
            var it = referencesCopy.GetEnumerator();
            while (it.MoveNext())
            {
                if (dataHandler.TryGetValue(it.Current, out dependencyData))
                {
                    Disconnect(dependencyData, this);
                }
            }
        }

        public override string ToString()
        {
            return $"{PrettyName} ({objectGuid})";
        }

        public bool IsReferencedBy(string otherObjectGuid)
        {
            return References.Contains(otherObjectGuid);
        }

        public bool HasDependencyOn(string otherObjectGuid)
        {
            return Dependencies.Contains(otherObjectGuid);
        }

        /// <summary>
        /// Find if otherObjectGuid is a dependency of the current object.
        /// </summary>
        /// <param name="dataHandler">Data handled by the cache</param>
        /// <param name="otherObjectGuid">Other object Guid</param>
        /// <param name="depth">How deep the search should go. -1 for infinite</param>
        internal bool HasDependencyOn(DependencyCacheDataHandler dataHandler, string otherObjectGuid, int depth)
        {
            // Find among dependencies directly
            foreach (var dependency in Dependencies)
            {
                if (dependency == otherObjectGuid)
                {
                    return true;
                }
            }

            if (depth != 0)
            {
                --depth;

                // For each dependency, find among its dependencies too
                foreach (var dependency in Dependencies)
                {
                    if (dataHandler.TryGetValue(dependency, out DependencyData dependencyData))
                    {
                        bool hasDependency = dependencyData.HasDependencyOn(dataHandler, otherObjectGuid, depth);
                        if (hasDependency)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }


        private string[] ArrayOfGuidToPaths(HashSet<string> guidSet)
        {
            string[] paths = new string[guidSet.Count];
            int i = 0;
            foreach (string guid in guidSet)
            {
                paths[i++] = AssetDatabase.GUIDToAssetPath(guid);
            }
            return paths;
        }
    }
}