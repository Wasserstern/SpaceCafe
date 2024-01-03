using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderItemUI : MonoBehaviour
{
    public AllManager allmng;
    public float uiSize;
    public int index = 0;

    void Awake(){
    }
    void Start()
    {
        allmng = GameObject.Find("AllManager").GetComponent<AllManager>();
    }

    void Update()
    {
        transform.localScale = new Vector2(uiSize * allmng.mainCamera.orthographicSize, uiSize * allmng.mainCamera.orthographicSize);
        transform.localPosition = new Vector2(uiSize * allmng.mainCamera.orthographicSize * index, transform.localPosition.y);
    }
}
