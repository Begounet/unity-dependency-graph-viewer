using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UDGV.Tests
{
    internal static class AssertUtility
    {
        public static void IsTextureReferencedByMaterial(Texture texture, Material material)
        {
            Assert.IsTrue(TestUtility.IsTextureReferencedByMaterial(texture, material), $"The material '{material}' should refer the texture '{texture}'.");
        }
    }
}