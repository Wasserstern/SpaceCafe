using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class CannonTerminal : Terminal{



    public GameObject cannonAnchor;
    public GameObject cannon;
    float xSteer;
    public float steerSpeed;
    public float shootForce;
    public float uiSize;
    public float uiSizeItems;

    public List<List<Item>> storage;

    public Transform physicalStorage;
    public Transform storageEntrance;
    public float storageVisibilityTime;
    public float storageVisibilityDelay;
    float currentVisibilityTime;
    int storageIndex;
    protected override void Start(){
        terminalActionMap = allmng.actions.CannonTerminal;
        terminalActionMap.FindAction("Shoot").performed += Shoot;
        terminalActionMap.FindAction("StorageLeft").performed += StorageLeft;
        terminalActionMap.FindAction("StorageRight").performed += StorageRight;
        base.Start();
        storage = new List<List<Item>>();
        for(int i = 0; i < Enum.GetNames(typeof(Item.ItemType)).Length; i++){
            storage.Add(new List<Item>());
        }
        
    }

    protected override void Update(){
        base.Update();
        if(terminalActionMap.enabled){
            xSteer = terminalActionMap.FindAction("Steer").ReadValue<float>();
            cannonAnchor.transform.Rotate(new Vector3(0f, 0f, -xSteer * steerSpeed * Time.deltaTime));
        }
        if(currentVisibilityTime < storageVisibilityDelay){
            SpriteRenderer[] childRenderer = physicalStorage.GetComponentsInChildren<SpriteRenderer>();
            foreach(SpriteRenderer renderer in childRenderer){
                renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, 1f);
            }
        }
        else{
            SpriteRenderer[] childRenderer = physicalStorage.GetComponentsInChildren<SpriteRenderer>();
            float currentAlpha = Mathf.Lerp(1f, 0f, (currentVisibilityTime - storageVisibilityDelay) / storageVisibilityTime);
            foreach(SpriteRenderer renderer in childRenderer){
                renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, currentAlpha);
            }
        }
        currentVisibilityTime += Time.deltaTime;
        physicalStorage.localScale = new Vector3(uiSize * allmng.mainCamera.orthographicSize, uiSize * allmng.mainCamera.orthographicSize);
    }

    void Shoot(InputAction.CallbackContext context){
        if(storage[storageIndex].Count > 0){
            int removeIndex = UnityEngine.Random.Range(0, storage[storageIndex].Count);
            Item shootItem = storage[storageIndex][removeIndex];
            shootItem.gameObject.layer = LayerMask.NameToLayer("Item");
            storage[storageIndex].RemoveAt(removeIndex);
            shootItem.transform.SetParent(null);
            shootItem.transform.localScale = new Vector3(shootItem.size, shootItem.size, shootItem.size);
            Color color = shootItem.GetComponent<SpriteRenderer>().color;
            color = new Color(color.r, color.g, color.b, 1f);
            shootItem.GetComponent<SpriteRenderer>().color = color;
            shootItem.transform.position = cannon.transform.position;
            Vector2 shootDirection = -((Vector2)cannonAnchor.transform.position - (Vector2)cannon.transform.position).normalized;
            shootItem.rgbd.gravityScale = 0f;
            shootItem.rgbd.AddForce(shootDirection * shootForce, ForceMode2D.Impulse);
            shootItem.Activate();
            currentVisibilityTime = 0f;
        }
    }

    public void AddItem(Item item){
        item.transform.localScale = new Vector3(physicalStorage.localScale.x * uiSizeItems, physicalStorage.localScale.y * uiSizeItems, physicalStorage.localScale.z * uiSizeItems);
        item.gameObject.layer = LayerMask.NameToLayer("Storage");
        item.rgbd.simulated = true;
        item.rgbd.velocity = new Vector2(0f, 0f);
        storage[(int)item.type].Add(item);
        item.transform.SetParent(physicalStorage);
        item.transform.position = storageEntrance.position;
        currentVisibilityTime = 0f;
    }

    void StorageRight(InputAction.CallbackContext context){
        bool itemsFound = false;
        int nextIndex = storageIndex;
        int counter = 0;
        while(!itemsFound && counter < storage.Count){
            if(nextIndex +1 < storage.Count){
                nextIndex++;
            }
            else{
                nextIndex = 0;
            }
            if(storage[nextIndex].Count != 0){
                itemsFound = true;
            }
            counter++;
        }
        storageIndex = nextIndex;
    }
    void StorageLeft(InputAction.CallbackContext context){
        bool itemsFound = false;
        int nextIndex = storageIndex;
        int counter = 0;
        while(!itemsFound && counter < storage.Count){
            if(nextIndex -1 != 0){
                nextIndex--;
            }
            else{
                nextIndex = storage.Count -1;
            }
            if(storage[nextIndex].Count != 0){
                itemsFound = true;
            }
            counter++;
        }
        storageIndex = nextIndex;
    }
    private void OnTriggerEnter2D(Collider2D other){
        if(other.gameObject.layer == LayerMask.NameToLayer("Storage")){
            Item lostItem = other.gameObject.GetComponent<Item>();
            int lostItemIndex = storage[(int)other.gameObject.GetComponent<Item>().type].FindIndex(item => item.GetInstanceID() == lostItem.GetInstanceID());
            storage[(int)other.gameObject.GetComponent<Item>().type].RemoveAt(lostItemIndex);
            Destroy(other.gameObject);
        }
    }

}