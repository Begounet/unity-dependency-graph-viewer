using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace UDGV
{
    public class DependencyData
    {
        public string objectGuid;
        public long localId;
        public string sceneOwnerGuid;

        public string PrettyName => AssetDatabase.GUIDToAssetPath(objectGuid);

        public HashSet<string> Dependencies { get; private set; } = new HashSet<string>();
        public HashSet<string> References { get; private set; } = new HashSet<string>();

        public static void Connect(DependencyData reference, DependencyData dependency)
        {
            reference.Dependencies.Add(dependency.objectGuid);
            dependency.References.Add(reference.objectGuid);
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
    }
}