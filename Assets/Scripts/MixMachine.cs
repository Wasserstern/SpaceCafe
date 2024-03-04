using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MixMachine : MonoBehaviour
{
    public List<Recipe> allRecipes;
    [Serializable]
    public class Recipe{
        public string name;
        public List<Item.ItemType> ingredients;
        public GameObject itemPrefab;
        public Recipe(){
            ingredients = new List<Item.ItemType>();
        }
    }
    [SerializeField]
    List<Item> currentItems;
    int maxRecipeLength;
    public float inputForce;
    public float outputForce;
    public float ejectTime;
    public Transform input;
    public Transform inputForceDirector;
    public Transform output;

    private void Start(){
        currentItems = new List<Item>();
        int max = 0;
        foreach(Recipe recipe in allRecipes){
            if(recipe.ingredients.Count > max){
                max = recipe.ingredients.Count;
            }
        }
        maxRecipeLength = max;
    }

    private void Update(){
       if(Input.GetKeyDown(KeyCode.M)){
            if(currentItems.Count > 0){
                Debug.Log("Should try to Mix");
                MixItems();
            }
        }
    }

    public void AddItem(Item item){
        item.transform.position = input.position;
        item.canBeHold = false;
        currentItems.Add(item);
        Vector2 forceDirection = ((Vector2)input.position - (Vector2)item.transform.position).normalized;
        item.rgbd.AddForce(forceDirection * inputForce, ForceMode2D.Impulse);
        
    }

    public IEnumerator EjectAll(){
        foreach(Item item in currentItems){
            item.canBeHold = true;
            item.transform.position = output.position;
            Vector2 forceDirection = ((Vector2)output.position - (Vector2)item.transform.position).normalized;
            item.rgbd.AddForce(forceDirection * outputForce, ForceMode2D.Impulse);
            yield return new WaitForSeconds(ejectTime);
        }
        currentItems.Clear();
    }
  
    public void MixItems(){
        for(int i = currentItems.Count; i > 0; i--){
            if(i > maxRecipeLength){
                i = maxRecipeLength;
            }
            int recipeIndex = 0;
            foreach(Recipe recipe in allRecipes){
                Debug.Log(recipe.ingredients);
                if(recipe.ingredients.Count == i){ // If recipe is targetLength
                    List<int> toRemove = new List<int>();
                        List<Item.ItemType> recipeCopy = new List<Item.ItemType>(recipe.ingredients);
                        for(int j = currentItems.Count -1; j >= 0; j--){
                            Item item = currentItems[j];
                            if(recipeCopy.Remove(item.type)){
                                toRemove.Add(j);
                                if(recipeCopy.Count == 0){
                                    Debug.Log("Recipe matched!");
                                    // Recipe matched!
                                    foreach(int index in toRemove){
                                        Item destroyItem = currentItems[index];
                                        currentItems.RemoveAt(index);
                                        Destroy(destroyItem.gameObject);
                                        
                                    }
                                    GameObject newMixItem = GameObject.Instantiate(recipe.itemPrefab);
                                    newMixItem.transform.position = output.position;
                                    break;
                                }
                            }
                        }
                }
                recipeIndex++;
            }
            if(i > currentItems.Count){
                i = currentItems.Count;
            }
            if(currentItems.Count == 0){
                break;
            }
        }
    }

}
