using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : MonoBehaviour
{
    AllManager allmng;
    public Item.ItemType fruitType;
    public GameObject branchPrefab;
    public string production;
    public int iterations;
    public float angleStep;
    public float treeGrowTime;
    public float fruitChance;
    
    string currentString;
    Stack<(Transform currentTransform, Vector2 branchEnd, float angle, int depth)> turtleStates;
    Queue<(Vector2 position, float angle, int depth)> branchElementData;

    List<Transform> fruitTransforms;
    List<GameObject> fruits;
    
    // Position, angle
    (Transform branchTransform, Vector2 branchEnd, float angle, int depth) turtle;
    bool coroutineRunning = false;

    float currentGrowTime;
    void Start()
    {
        allmng = GameObject.Find("AllManager").GetComponent<AllManager>();
        branchElementData = new Queue<(Vector2 position, float angle, int depth)>();
        turtleStates = new Stack<(Transform currentTransform, Vector2 branchEnd, float angle, int depth)>();
        turtle = (transform, (Vector2)transform.position, 0f, 1);
        fruitTransforms = new List<Transform>();
        fruits = new List<GameObject>();
        currentString = "F";
        for(int i = 0; i < iterations; i++){
            string nextString = "";
            foreach(char c in currentString){
                if(c == 'F'){
                    nextString += production;
                }
                else{
                    nextString += c;
                }
            }
            currentString = nextString;
        }

        foreach(char c in currentString){
            switch(c){
                case 'F':{

                    GameObject newBranch = GameObject.Instantiate(branchPrefab);
                    newBranch.transform.position = turtle.branchEnd;
                    newBranch.transform.rotation = Quaternion.Euler(0f, 0f, turtle.angle);
                    newBranch.transform.SetParent(turtle.branchTransform);
                    //newBranch.transform.localScale /= turtle.depth;
                    turtle = (newBranch.transform, (Vector2)newBranch.transform.GetChild(0).position, turtle.angle, turtle.depth);
                    branchElementData.Enqueue((turtle.branchEnd, turtle.angle, turtle.depth));
                    break;
                }
                case '[':{
                    turtleStates.Push(turtle);
                    turtle.depth++;
                    break;
                }
                case ']':{
                    fruitTransforms.Add(turtle.branchTransform.GetChild(0));
                    turtle = turtleStates.Pop();
                    break;
                }
                case '+':{

                    turtle.angle += angleStep;
                    turtle.depth++;
                    break;
                }
                case '-':{
                    turtle.depth++;
                    turtle.angle -= angleStep;
                    break;
                }
            }
        }
        foreach(Transform fruitTransform in fruitTransforms){
            if(UnityEngine.Random.Range(0f, 1f) < fruitChance){
                GameObject newFruit = GameObject.Instantiate(allmng.itemDataset[(int)fruitType].itemPrefab);
                newFruit.transform.parent = fruitTransform;
                newFruit.transform.position = fruitTransform.position;
                fruits.Add(newFruit);
            }
        }
    }

    public void HarvestTree(){
        for(int i = fruits.Count; i >= 0; i--){
            GameObject fruit = fruits[i];
            Item fruitItem = fruit.GetComponent<Item>();
            if(fruitItem.canBeHarvested){
                fruitItem.Harvest();
                fruits.RemoveAt(i);
            }
        }
    }

    void Update()
    {
        /*
        if(!coroutineRunning && branchElementData.Count > 0){
            coroutineRunning = true;
            StartCoroutine(GrowBranch(branchElementData.Dequeue()));
        }
       */
       if(currentGrowTime >= treeGrowTime){

       }
       currentGrowTime += Time.deltaTime;
    }
    /*
    IEnumerator GrowBranch((Vector2 position, float angle, int depth) branchData){
        float startTime = Time.time;
        float elapsedTime = 0f;
        GameObject newBranch = GameObject.Instantiate(branchPrefab);
        newBranch.transform.position = branchData.position;
        newBranch.transform.localScale = new Vector3(0f, 0f, 0f);
        newBranch.transform.rotation = Quaternion.Euler(0f, 0f, branchData.angle);
        while(Time.time - startTime < branchGrowTime){
            float t = elapsedTime / branchGrowTime;
            newBranch.transform.localScale = Vector3.Lerp(new Vector3(0f, 0f, 0f), new Vector3(1f, 1f, 1f), t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        coroutineRunning = false;
    }
    */
}
