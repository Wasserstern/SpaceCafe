using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Terminal : MonoBehaviour
{
    public AllManager allmng;
    // Variables for all terminals
    public InputActionMap terminalActionMap;
    public Transform playerAnchor;
    Transform playerTransform;
    void Awake(){
        allmng = GameObject.Find("AllManager").GetComponent<AllManager>();
    }
    
    protected virtual void Start()
    {
        playerTransform = GameObject.Find("Player").transform;
        terminalActionMap.FindAction("Deactivate").performed += Deactivate;
    }

    protected virtual void Update()
    {
        if(terminalActionMap != null && terminalActionMap.enabled){
            playerTransform.position = new Vector3(playerAnchor.transform.position.x, playerAnchor.transform.position.y, playerTransform.position.z);
        }   
    }


    // All Terminals
    protected void Deactivate(InputAction.CallbackContext context){
        Debug.Log("Deactivate");
        playerTransform.gameObject.GetComponent<Player>().ToggleTerminal(context);
    }
}
