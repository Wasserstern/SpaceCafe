using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Water : MonoBehaviour
{
    // Start is called before the first frame update
    public float towardsTreeSpeed;
    public float absorbTime;
    public Tree tree;

    Rigidbody2D rgbd;
    void Start()
    {
        rgbd = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if(tree != null){
            rgbd.simulated = false;
            transform.position = Vector2.MoveTowards(transform.position, tree.absorbPoint.position, towardsTreeSpeed * Time.deltaTime);
            if(transform.position == tree.absorbPoint.position){
                StartCoroutine(GetAbsorbed(tree));
                tree = null;
            }
        }
        
    }

    IEnumerator GetAbsorbed(Tree tree){
        float startTime = Time.time;
        float elapsedTime = 0f;
        Vector3 startScale = transform.localScale;
        while(Time.time - startTime < absorbTime){
            float t = elapsedTime / absorbTime;
            transform.localScale = Vector3.Lerp(startScale, new Vector3(0f, 0f, 0f), t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        tree.currentWater++;
        Destroy(this.gameObject);
    }
}
