using System;
using System.Collections.Generic;
using UnityEngine;

public class AllManager : MonoBehaviour
{
    public Actions actions;
    public Camera mainCamera;
    public GameObject customerPrefab;
    
    // Difficulty settings
    public int difficultyLevel;
    public int minItemsPerOrder;
    public int maxItemsPerOrder;

    public float minOrderTime;
    public float maxOrderTime;
    public float maxOrders;
    public float orderSpawnTime;
    float elapsedTime;

    [Serializable]
    public struct ItemData{
        public string name;
        public bool itemActive;
        public float itemChance;
        public float worth;
        public Item.ItemType itemType;
        public GameObject itemPrefab;
        public GameObject treePrefab;
        public Sprite sprite;
        
    }

    public ItemData[] itemDataset;

    public List<CafeOrder> currentOrders;
    
    void Awake(){
        actions = new Actions();
    }
    void Start()
    {
        currentOrders = new List<CafeOrder>();
    }
    void Update()
    {
        if(elapsedTime >= orderSpawnTime){
            if(currentOrders.Count < maxOrders){
                Vector2 cafeOrigin = (Vector2)GameObject.Find("Cafe").transform.position;
                GameObject newCustomer = GameObject.Instantiate(customerPrefab);
                float radians = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
                float x = cafeOrigin.x + 50f * Mathf.Cos(radians);
                float y = cafeOrigin.y + 50f * Mathf.Sin(radians);
                newCustomer.transform.position = new Vector2(x, y);
                currentOrders.Add(newCustomer.GetComponent<Customer>().order);
            }
            elapsedTime = 0f;
        }
        elapsedTime += Time.deltaTime;
    }

    public void ChangeDifficulty(){
        switch(difficultyLevel){
            case 0:{
                break;
            }  
        }
    }

}
