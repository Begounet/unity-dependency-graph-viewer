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
    }
}