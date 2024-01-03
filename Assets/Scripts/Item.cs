using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Item : MonoBehaviour
{
    AllManager allmng;

    public enum ItemType {apple, coffeeBeans, coffee, sugar,applePie}
    
    public Rigidbody2D rgbd;
    SpriteRenderer spriteRenderer;
    Collider2D col;
    public float weight;
    public float size;
    public float health;
    public float expireTime;
    public float growTime;
    public ItemType type;

    float currentExpireTime;
    float currentGrowTime;

    public bool onTree;
    public bool canBeHarvested;
    void Start()
    {
        allmng = GameObject.Find("AllManager").GetComponent<AllManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        rgbd = GetComponent<Rigidbody2D>();
        transform.localScale = new Vector3(size, size, size);
        rgbd.mass = weight;
    }

    void Update()
    {
        if(!onTree){
            if(health <= 0f){
                Destroy(this.gameObject);
            }
            if(currentExpireTime >= expireTime){
                switch(type){
                    case ItemType.apple:{
                        GameObject newFruitFly = GameObject.Instantiate(allmng.fruitFlyPrefab);
                        newFruitFly.transform.position = transform.position;
                        health = 0f;
                        break;
                    }
                }
            }
            else if(currentExpireTime >= expireTime * 0.8f){
                spriteRenderer.sprite = allmng.itemDataset[(int)type].decaySprite;
            }
            currentExpireTime += Time.deltaTime;
        }
        else{
            if(currentGrowTime >= growTime){
                canBeHarvested = true;
            }
            currentGrowTime += Time.deltaTime;
        }

    }
    void OnTriggerEnter2D(Collider2D other){
        if(other.gameObject.layer == LayerMask.NameToLayer("StoragePickup")){
            other.gameObject.GetComponentInParent<CannonTerminal>().AddItem(this);
        }
    }

    public void Harvest(){
        onTree = false;
        transform.SetParent(null);
        GetComponent<Joint2D>().enabled = false;
    }
}
