using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CafeOrder : MonoBehaviour
{
    AllManager allmng;
    public GameObject itemUIPrefab;
    public float timeBonus;
    int itemCount;
    float orderTime;
    float elapsedTime;

    public List<Item.ItemType> requestedItems;

    bool isActive;
    public float orderWorth;

    public Transform orderUITransform;

    void Start()
    {
        allmng = GameObject.Find("AllManager").GetComponent<AllManager>();
        requestedItems = new List<Item.ItemType>();
        itemCount = Random.Range(allmng.minItemsPerOrder, allmng.maxItemsPerOrder +1);
        for(int i = 0; i < itemCount; i++){
            Item.ItemType possibleItem = Item.ItemType.coffee;
            int dataSetIndex = 0;
            for(int j = 0; j < allmng.itemDataset.Length; j++ ){
                if(allmng.itemDataset[j].itemActive){
                    possibleItem = allmng.itemDataset[j].itemType;
                    dataSetIndex = j;
                    float roll = Random.Range(0f, 1f);
                    if(roll < allmng.itemDataset[j].itemChance){
                        // Item selection succesfull. 
                        possibleItem = allmng.itemDataset[j].itemType;
                        break;
                    }
                }
                
            }
            orderWorth += allmng.itemDataset[dataSetIndex].worth;
            requestedItems.Add(possibleItem);
        }
        orderTime = Random.Range(allmng.minOrderTime, allmng.maxOrderTime);
    }

    void Update()
    {

        if(isActive){
            if(elapsedTime <= orderTime){
                // ORDER TIME OVER
                elapsedTime = 0f;
            }
            elapsedTime += Time.deltaTime;
            float xOffset = 0f;
            for(int i = 0; i > orderUITransform.childCount; i++){
                xOffset += orderUITransform.GetChild(i).localScale.x;
            }
            orderUITransform.localPosition = new Vector2(-xOffset / 2, 1f);
        }

    }
    public void Activate(){
        isActive = true;
        orderUITransform.localPosition = new Vector2(-requestedItems.Count / 2, 1f);
        for(int i = 0; i < requestedItems.Count; i++){
            GameObject itemUI = GameObject.Instantiate(itemUIPrefab);
            itemUI.transform.SetParent(orderUITransform);
            itemUI.transform.localPosition = new Vector2(i, 0f);
            itemUI.GetComponent<OrderItemUI>().index = i;
            itemUI.GetComponent<SpriteRenderer>().sprite = allmng.itemDataset[(int)requestedItems[i]].sprite;
        }
    }
    public bool TryServeItem(Item.ItemType servedType){
        if(requestedItems.Remove(servedType)){
            // TODO: CHANGE SPRITE DYNAMICALLY
            bool removedUI = false;
            for(int i = 0; i < orderUITransform.childCount; i++){
                if(!removedUI && orderUITransform.GetChild(i).GetComponent<SpriteRenderer>().sprite == allmng.itemDataset[(int)servedType].sprite){
                    Destroy(orderUITransform.GetChild(i).gameObject);
                    removedUI = true;
                }
                else if(removedUI){
                    orderUITransform.GetChild(i).GetComponent<OrderItemUI>().index--;
                }
            }
            if(requestedItems.Count == 0){
                orderWorth += (1 - (elapsedTime / orderTime)) * timeBonus;
            }
            return true;
        }
        else{
            return false;
        }
    }

    

}
