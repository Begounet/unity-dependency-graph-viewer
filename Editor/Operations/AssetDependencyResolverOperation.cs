using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class AssetDependencyResolverOperation : DependencyViewerOperation
{
    public int numTotalAssets { get; set; }
    public int numProcessedAssets { get; set; }
    public DependencyViewerNode node { get; set; }

    public override string GetStatus()
    {
        return string.Format("[{0:00.0}%][{1}] Asset dependency resolving for node... ({2:000} / {3:000})", 
            ((float) numProcessedAssets / numTotalAssets) * 100, 
            node.Name, 
            numProcessedAssets, numTotalAssets);
    }
}
