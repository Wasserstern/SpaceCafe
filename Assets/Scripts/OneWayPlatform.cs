using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneWayPlatform : MonoBehaviour
{
    // Start is called before the first frame update
    bool isFlipped;
    Rigidbody2D playerRgbd;
    PlatformEffector2D platEffector;
    void Start()
    {
        playerRgbd = GameObject.Find("Player").GetComponent<Rigidbody2D>();
        platEffector = GetComponent<PlatformEffector2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if(isFlipped){
            platEffector.rotationalOffset = 0f;
            if(playerRgbd.velocity.y > 0f){
                platEffector.rotationalOffset = 180f;
                isFlipped = false;
            }
        }
    }
    public void FallThrough(){
        Debug.Log("I should be flipped now.");
        isFlipped = true;
    }
}
