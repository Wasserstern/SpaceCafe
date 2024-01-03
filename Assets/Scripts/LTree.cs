using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LTree : MonoBehaviour
{
    AllManager allmng;
    public Item.ItemType fruitType;
    public GameObject branchPrefab;
    public string production;
    public int iterations;
    public float angleStep;
    public float branchGrowTime;
    public float fruitChance;
    
    string currentString;
    Stack<(Vector2 position, float angle, int depth)> turtleStates;
    Queue<(Vector2 position, float angle, int depth)> branchElementData;
    List<Vector2> fruitPositions;
    
    // Position, angle
    (Vector2 position, float angle, int depth) turtle;
    bool coroutineRunning = false;

    void Start()
    {
        allmng = GameObject.Find("AllManager").GetComponent<AllManager>();
        turtle = ((Vector2)transform.position, 0f, 1);
        turtleStates = new Stack<(Vector2 position, float angle, int depth)>();
        branchElementData = new Queue<(Vector2 position, float angle, int depth)>();
        fruitPositions = new List<Vector2>();
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
                    newBranch.transform.position = turtle.position;
                    newBranch.transform.rotation = Quaternion.Euler(0f, 0f, turtle.angle);
                    newBranch.transform.parent = transform;
                    //newBranch.transform.localScale /= turtle.depth;
                    turtle.position = newBranch.transform.GetChild(0).position;
                    branchElementData.Enqueue((turtle.position, turtle.angle, turtle.depth));
                    break;
                }
                case '[':{
                    turtleStates.Push(turtle);
                    turtle.depth++;
                    break;
                }
                case ']':{
                    fruitPositions.Add(turtle.position);
                    turtle = turtleStates.Pop();
                    turtle.depth--;
                    break;
                }
                case '+':{
                    turtle.depth++;
                    turtle.angle += angleStep;
                    break;
                }
                case '-':{
                    turtle.depth++;
                    turtle.angle -= angleStep;
                    break;
                }
            }
        }
        foreach(Vector2 position in fruitPositions){
            if(UnityEngine.Random.Range(0f, 1f) < fruitChance){
                GameObject newFruit = GameObject.Instantiate(allmng.itemDataset[(int)fruitType].itemPrefab);
                newFruit.transform.parent = transform;
                newFruit.transform.position = position;
                newFruit.GetComponent<Rigidbody2D>().simulated = false;
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
    }
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
}
