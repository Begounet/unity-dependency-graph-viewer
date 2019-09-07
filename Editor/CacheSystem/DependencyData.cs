using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace UDGV
{
    internal class DependencyData
    {
        public string objectGUID;
        public long localId;
        public string sceneOwnerGUID;

        public string PrettyName => AssetDatabase.GUIDToAssetPath(objectGUID);

        public HashSet<string> Dependencies { get; private set; } = new HashSet<string>();
        public HashSet<string> References { get; private set; } = new HashSet<string>();

        public static void Connect(DependencyData reference, DependencyData dependency)
        {
            reference.Dependencies.Add(dependency.objectGUID);
            dependency.References.Add(reference.objectGUID);
        }

        public override string ToString()
        {
            return $"{PrettyName} ({objectGUID})";
        }
    }
}