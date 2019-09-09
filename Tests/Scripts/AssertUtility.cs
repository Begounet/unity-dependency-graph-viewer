using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UDGV.Tests
{
    internal static class AssertUtility
    {
        public static void IsObjectDirectlyReferencingOtherObject(Texture texture, Material material)
        {
            Assert.IsTrue(
                TestUtility.IsTextureReferencedByMaterial(texture, material), 
                $"The material '{material}' should refer the texture '{texture}'.");
        }

        public static void IsObjectDirectlyReferencingOtherObject(UnityEngine.Object targetObject, UnityEngine.Object otherObject, DependencyViewerSettings settings)
        {
            Assert.IsTrue(
                DependencyResolverUtility.IsObjectReferencingOtherObject(targetObject, otherObject, settings), 
                $"The object '{targetObject}' is not directly referencing '{otherObject}'.");
        }
    }
}