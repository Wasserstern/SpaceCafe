using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Item : MonoBehaviour
{

    public enum ItemType {apple, coffeeBeans, coffee, sugar,applePie}
    
    public Rigidbody2D rgbd;
    Collider2D col;
    public float weight;
    public float size;
    public float health;
    public ItemType type;
    void Start()
    {
        col = GetComponent<Collider2D>();
        rgbd = GetComponent<Rigidbody2D>();
        transform.localScale = new Vector3(size, size, size);
        rgbd.mass = weight;
    }

    void Update()
    {
        if(health <= 0f){
            Destroy(this.gameObject);
        }
    }
    void OnTriggerEnter2D(Collider2D other){
        if(other.gameObject.layer == LayerMask.NameToLayer("StoragePickup")){
            other.gameObject.GetComponentInParent<CannonTerminal>().AddItem(this);
        }
    }

    public void Activate(){

    }

}
