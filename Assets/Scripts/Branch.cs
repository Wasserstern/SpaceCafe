using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Branch : MonoBehaviour
{
    public Tree tree;
    public int branchDepth;
    public float branchSizeStep;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
        if(tree != null && tree.currentGrowTime <= tree.treeGrowTime){
            float growthFactor = tree.currentGrowTime / tree.treeGrowTime;
            growthFactor *= 1f - branchSizeStep * branchDepth;
            transform.localScale = new Vector3(growthFactor, growthFactor, growthFactor);
        }
    }
}
