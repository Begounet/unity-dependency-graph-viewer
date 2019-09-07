using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class AssetDependencyResolverOperation : DependencyViewerOperation
{
    public int numTotalAssets { get; set; }
    public int numProcessedAssets { get; set; }
    public int numProcessedProperties { get; set; }
    public DependencyViewerNode node { get; set; }

    public UnityEngine.Object AssetBeingProcessed { get; set; }

    public override string GetStatus()
    {
        return string.Format("[{0:00.0}%][{1:0000}/{2:0000}][{3} -> {4}] Asset dependency resolving...", 
            ((float) numProcessedAssets / numTotalAssets) * 100,
            numProcessedAssets, numTotalAssets,
            node.Name, AssetBeingProcessed.name);
    }
}
