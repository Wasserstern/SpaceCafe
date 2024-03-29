using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Item : MonoBehaviour
{
    AllManager allmng;

    public enum ItemType {apple, coffeeBeans, coffee, sugar,applePie, wateringCan, mintman}
    
    public Rigidbody2D rgbd;
    SpriteRenderer spriteRenderer;
    public Collider2D col;
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
    public bool canBeHold = true;
    protected virtual void Start()
    {
        allmng = GameObject.Find("AllManager").GetComponent<AllManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        rgbd = GetComponent<Rigidbody2D>();
        transform.localScale = new Vector3(size, size, size);
        rgbd.mass = weight;
    }

    protected virtual void Update()
    {
        if(currentGrowTime >= growTime){
            canBeHarvested = true;
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
            float growthFactor = currentGrowTime / growTime;
            transform.localScale = new Vector3(growthFactor * size, growthFactor * size, growthFactor * size);
            currentGrowTime += Time.deltaTime;
        }

    }
    public void UseItemEffect(){
        switch(type){
            case ItemType.apple:{
                GameObject.Find("Player").GetComponent<Player>().health += 1f;
                break;
            }
            case ItemType.wateringCan:{
                break;
            }
        }
    }
    void OnTriggerEnter2D(Collider2D other){
        if(other.gameObject.layer == LayerMask.NameToLayer("StoragePickup") && !onTree){
            other.gameObject.GetComponentInParent<CannonTerminal>().AddItem(this);
        }
        else if(other.gameObject.layer == LayerMask.NameToLayer("MixMachine")){
            other.GetComponentInParent<MixMachine>().AddItem(this);
        }
    }

    public void HarvestFruit(){
        onTree = false;
        GetComponent<Joint2D>().enabled = false;
    }
}
