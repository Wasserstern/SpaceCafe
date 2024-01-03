using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    AllManager allmng;
    Rigidbody2D rgbd;
    Collider2D col;

    Actions actions;
    InputActionMap currentActionMap;

    // General settings
    public int coins;
    public float maxMoveSpeed;
    public float moveAcceleration;
    public float moveDeceleration;
    public float jumpForce;
    public float throwForce;
    public float groundCheckDistance;
    public float pickupDistance;
    public float itemHeight;
    public float terminalRadius;
    public int maxItems;
    [Range(0f, 90f)]
    public float throwAngle;
    public float plantDepth;

    public float cafeCamSize;
    public float terminalCamSize;
    public float camSizeChangeTime;

    // Various elements
    Stack<GameObject> items;
    float xInput;
    Camera mainCamera;

    Coroutine camCoroutine = null;


    // States

    [SerializeField]
    bool isGrounded;
    [SerializeField]
    bool isJumping;
    [SerializeField]
    bool isFacingRight = true;
    [SerializeField]
    bool isUsingTerminal = false;
    [SerializeField]
    bool isOnSoil = false;
    Vector2 nextTreePosition;
    void Awake(){
   
    }
    void OnEnable(){
    }
    void Start()
    {
        allmng = GameObject.Find("AllManager").GetComponent<AllManager>();
        mainCamera = allmng.mainCamera;
        actions = allmng.actions;
        currentActionMap = actions.PlayerMoveCafe;
        actions.PlayerMoveCafe.Jump.performed += Jump;
        actions.PlayerMoveCafe.Pickup.performed += Pickup;
        actions.PlayerMoveCafe.Throw.performed += Throw;
        actions.PlayerMoveCafe.Move.performed += Move;
        actions.PlayerMoveCafe.UseTerminal.performed += ToggleTerminal;
        actions.PlayerMoveCafe.Plant.performed += Plant;
        items = new Stack<GameObject>();
        rgbd = GetComponent<Rigidbody2D>();
        col= GetComponent<Collider2D>();
        for(int i = 0; i < maxItems; i++){
            GameObject itemSlot = new GameObject();
            itemSlot.transform.SetParent(transform);
            float playerHeight = col.bounds.max.y * 2.1f;
            itemSlot.transform.localPosition = new Vector2(0, itemHeight * i + playerHeight);
        }
        currentActionMap.Enable();
    }
    void OnDisable(){
        currentActionMap.Disable();
    }

    void Update()
    {
        // Check movement value
        xInput = actions.PlayerMoveCafe.Move.ReadValue<float>();
        
        // Check ground
        RaycastHit2D groundHit = Physics2D.Raycast(transform.position, (new Vector2(transform.position.x, transform.position.y -1) - (Vector2)transform.position).normalized, groundCheckDistance, LayerMask.GetMask("Ground", "Item", "Soil"));
        isGrounded = groundHit.collider != null;
        isOnSoil = isGrounded && groundHit.collider.gameObject.layer == LayerMask.NameToLayer("Soil");
        if(isOnSoil){
            nextTreePosition = new Vector2(groundHit.point.x, transform.position.y - plantDepth);
        }
        
        Vector2 nextVelocity = new Vector2();
        if(xInput > 0){
            nextVelocity += new Vector2(rgbd.velocity.x + moveAcceleration * Time.deltaTime, rgbd.velocity.y);
            if(nextVelocity.x > maxMoveSpeed){
                nextVelocity = new Vector2(maxMoveSpeed, rgbd.velocity.y);
            }
            isFacingRight = true;
        }
        else if(xInput < 0){
            nextVelocity += new Vector2(rgbd.velocity.x -moveAcceleration * Time.deltaTime, rgbd.velocity.y);
            if(nextVelocity.x < -maxMoveSpeed){
                nextVelocity = new Vector2(-maxMoveSpeed, rgbd.velocity.y);
            }
            isFacingRight = false;
        }
        else{
            if(rgbd.velocity.x < 0f){
                nextVelocity = new Vector2(rgbd.velocity.x + moveDeceleration * Time.deltaTime, rgbd.velocity.y);
                if(nextVelocity.x > 0f){
                    nextVelocity = new Vector2(0f, rgbd.velocity.y);
                }
            }
            else if(rgbd.velocity.x > 0f){
                nextVelocity = new Vector2(rgbd.velocity.x - moveDeceleration * Time.deltaTime, rgbd.velocity.y);
                if(nextVelocity.x < 0f){
                    nextVelocity = new Vector2(0f, rgbd.velocity.y);
                }
            }
            else{
                nextVelocity = new Vector2(0f, rgbd.velocity.y);
            }
        }
        rgbd.velocity = nextVelocity;
    }

    private void Jump(InputAction.CallbackContext context){
        Debug.Log("Jump");
        if(isGrounded){
            Vector2 jumpDirection = (new Vector2(transform.position.x, transform.position.y +1) - (Vector2)transform.position).normalized;
            rgbd.AddForce(jumpForce * jumpDirection, ForceMode2D.Impulse);

        }
    }

    private void Pickup(InputAction.CallbackContext context){
        Debug.Log("Pickup");
        RaycastHit2D itemHit = Physics2D.Raycast(transform.position, (new Vector2(transform.position.x, transform.position.y -1) - (Vector2)transform.position).normalized, pickupDistance, LayerMask.GetMask("Item"));
        if(itemHit.collider != null && items.Count < maxItems){
            GameObject item = itemHit.collider.gameObject;
            item.transform.GetComponent<Rigidbody2D>().simulated = false;
            item.transform.SetParent(transform.GetChild(items.Count));
            item.transform.localPosition = new Vector3(0, 0, 0);
            items.Push(item);
        }
    }
    private void Throw(InputAction.CallbackContext context){
        Debug.Log("Throw");
        if(items.Count > 0){
            GameObject item = items.Pop();
            item.transform.SetParent(null);
            item.GetComponent<Rigidbody2D>().simulated = true;
            Vector2 rightAxis = (new Vector2(transform.position.x + 1, transform.position.y) - (Vector2)transform.position).normalized;
            Vector2 throwPoint = new Vector2(rightAxis.x * Mathf.Cos(throwAngle) - rightAxis.y * Mathf.Sin(throwAngle), 
                rightAxis.x * Mathf.Sin(throwAngle) + rightAxis.y * Mathf.Cos(throwAngle)
            );
            if(isFacingRight){
                throwPoint = new Vector2(rightAxis.x * Mathf.Cos(180f - throwAngle) - rightAxis.y * Mathf.Sin(180f - throwAngle), 
                rightAxis.x * Mathf.Sin(180f - throwAngle) + rightAxis.y * Mathf.Cos(180f - throwAngle));
            }
            Vector2 throwDirection = (throwPoint - (Vector2)transform.position).normalized;
            item.GetComponent<Rigidbody2D>().AddForce(throwDirection * throwForce, ForceMode2D.Impulse);
        }
    }
    private void Plant(InputAction.CallbackContext conext){
        if(isOnSoil){
            GameObject topItem = items.Pop();
            GameObject newTree = GameObject.Instantiate(allmng.itemDataset[(int)topItem.GetComponent<Item>().type].treePrefab);
            GameObject.Destroy(topItem);
            newTree.transform.position = nextTreePosition;
        }
    }
    private void Move(InputAction.CallbackContext context){
        Debug.Log("Move");

    }

    public void ToggleTerminal(InputAction.CallbackContext context){
        Collider2D terminalCollider = Physics2D.OverlapCircle(transform.position, terminalRadius, LayerMask.GetMask("Terminal"));
        if(terminalCollider != null){
            if(terminalCollider.gameObject.GetComponent<Terminal>().terminalActionMap.enabled){
                terminalCollider.gameObject.GetComponent<Terminal>().terminalActionMap.Disable();
                currentActionMap.Enable();
                if(camCoroutine != null){
                    StopCoroutine(camCoroutine);
                    camCoroutine = StartCoroutine(ChangeCamSize(cafeCamSize));
                }
                else{
                    camCoroutine = StartCoroutine(ChangeCamSize(cafeCamSize));
                }
            }
            else{
                currentActionMap.Disable();
                terminalCollider.gameObject.GetComponent<Terminal>().terminalActionMap.Enable();

                if(camCoroutine != null){
                    StopCoroutine(camCoroutine);
                    camCoroutine = StartCoroutine(ChangeCamSize(terminalCamSize));
                }
                else{
                    camCoroutine = StartCoroutine(ChangeCamSize(terminalCamSize));
                }
            }
        }  
    }

    IEnumerator ChangeCamSize(float targetSize){
        float currentTime = Time.time;
        float elapsedTime = 0f;
        float currentCamSize = mainCamera.orthographicSize;
        while(Time.time - currentTime < camSizeChangeTime){
            float t = elapsedTime / camSizeChangeTime;
            mainCamera.orthographicSize = Mathf.Lerp(currentCamSize, targetSize, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    private void OnCollisionEnter2D(Collision2D other){

    }
}
