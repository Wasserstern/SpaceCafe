using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WateringCan : Item
{

    public GameObject waterPrefab;
    public float maxWater;
    public float currentWater;
    public float waterIntervalInSeconds;
    void Start()
    {
    }

 

    private void OnCollisionEnter2D(Collision2D other){
        /*
        if(other.gameObject.layer == LayerMask.NameToLayer("Water") && currentWater < maxWater){
            Destroy(other.gameObject);
            currentWater++;
        }
        */
    }
    public IEnumerator Water(Tree tree){
        
        for(int i = 0; i < 10; i++){
            if(currentWater > 0){
                currentWater--;
                float startTime = Time.time;
                float elapsedTime = 0f;
                GameObject water = GameObject.Instantiate(waterPrefab);
                water.transform.position = transform.position;
                water.GetComponent<Water>().tree = tree;
                

                while(Time.time - startTime < waterIntervalInSeconds){
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D other){
        if(other.gameObject.layer == LayerMask.NameToLayer("WaterFill")){
            currentWater = maxWater;
        }
    }
    
}
