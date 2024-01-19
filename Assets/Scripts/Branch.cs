using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Branch : MonoBehaviour
{
    public float treeGrowTime = 0f;
    public int branchDepth;
    public float branchSizeStep;

    float currentGrowTime;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(treeGrowTime != 0f && currentGrowTime <= treeGrowTime){
            float growthFactor = currentGrowTime / treeGrowTime;
            growthFactor *= 1f - branchSizeStep * branchDepth;
            transform.localScale = new Vector3(growthFactor, growthFactor, growthFactor);
            currentGrowTime += Time.deltaTime;
        }
    }
}
