using System;
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
    GameObject itemHighlight;

    Actions actions;
    InputActionMap currentActionMap;

    // General settings
    public int coins;
    public float health;
    public float maxMoveSpeed;
    public float moveAcceleration;
    public float moveDeceleration;
    public float jumpForce;
    public float invincibleTimer;
    public float throwForce;
    public float groundCheckDistance;
    public float pickupDistance;
    public float pickupBoxX;
    public float pickupBoxY;
    public float itemHeight;
    public float enemyHitForce;
    public float terminalRadius;
    public int maxItems;
    [Range(0f, 90f)]
    public float throwAngle;
    public float plantDepth;
    public float treePlantDistance;

    public float cafeCamSize;
    public float terminalCamSize;
    public float camSizeChangeTime;

    // Various elements
    Stack<GameObject> items;
    float xInput;
    float yInput;
    [SerializeField]
    Vector2 lookDirection;
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
    bool invincibleTimerRunning = false;
    Vector2 nextTreePosition;
    
    GameObject nearbyItem;
    void Awake(){
   
    }
    void OnEnable(){
    }
    void Start()
    {
        allmng = GameObject.Find("AllManager").GetComponent<AllManager>();
        itemHighlight = GameObject.Find("ItemHighlight");
        mainCamera = allmng.mainCamera;
        actions = allmng.actions;
        currentActionMap = actions.PlayerMoveCafe;
        actions.PlayerMoveCafe.Jump.performed += Jump;
        actions.PlayerMoveCafe.Pickup.performed += Pickup;
        actions.PlayerMoveCafe.Throw.performed += Throw;
        actions.PlayerMoveCafe.UseTerminal.performed += ToggleTerminal;
        actions.PlayerMoveCafe.Plant.performed += Plant;
        actions.PlayerMoveCafe.Harvest.performed += Harvest;
        actions.PlayerMoveCafe.UseItem.performed += UseItem;
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
        StartCoroutine(ChangeCamSize(cafeCamSize));
    }
    void OnDisable(){
        currentActionMap.Disable();
    }

    void Update()
    {
        // Check movement value
        xInput = actions.PlayerMoveCafe.Horizontal.ReadValue<float>();
        yInput = actions.PlayerMoveCafe.Vertical.ReadValue<float>();
        Vector2 pickupBoxSize = new Vector2(pickupBoxX, pickupBoxY);
        if(xInput != 0){
          lookDirection = new Vector2(xInput, 0f);  
        }
        else if(yInput != 0){
            lookDirection = new Vector2(0f, yInput);
        }
        if(lookDirection.x == 0){
            pickupBoxSize = new Vector2(pickupBoxY, pickupBoxY);
        }

        // Get closest item via look direction
        Collider2D[] nearbyItemColliders = Physics2D.OverlapBoxAll((Vector2)transform.position + lookDirection * 1.5f, pickupBoxSize, 0f, LayerMask.GetMask("Item"));
        if(nearbyItemColliders.Length > 0){
            float minDistance = 999f;
            GameObject closestItem = null;
            foreach(Collider2D itemCollider in nearbyItemColliders){
                float itemDistance = Vector2.Distance((Vector2)transform.position, (Vector2)itemCollider.gameObject.transform.position);
                if(itemDistance < minDistance && itemCollider.gameObject.GetComponent<Item>().canBeHold){
                    minDistance = itemDistance;
                    closestItem = itemCollider.gameObject;
                }
            }
            nearbyItem = closestItem;
            itemHighlight.transform.position = nearbyItem.transform.position;
        }
        else{
            nearbyItem = null;
            itemHighlight.transform.position = new Vector2(-9999, -9999f);
        }

        
        // Check ground
        RaycastHit2D groundHit = Physics2D.Raycast(transform.position, (new Vector2(transform.position.x, transform.position.y -1) - (Vector2)transform.position).normalized, groundCheckDistance, LayerMask.GetMask("Ground", "Item", "Soil", "Platform"));
        isGrounded = groundHit.collider != null;
        isOnSoil = isGrounded && groundHit.collider.gameObject.layer == LayerMask.NameToLayer("Soil");
        if(isOnSoil){
            nextTreePosition = new Vector2(groundHit.point.x, transform.position.y - plantDepth);
        }

        // Check if jumped on enemy
        RaycastHit2D enemyHit = Physics2D.BoxCast(transform.position, new Vector2(col.bounds.size.x, 0.5f), 0f, VariousStuff.DownDirection((Vector2)transform.position), 0.3f, LayerMask.GetMask("Ground", "Item", "Soil", "Platform"));
        if(enemyHit.collider != null){
            if(enemyHit.collider.gameObject.tag == "Mintman" && !enemyHit.collider.gameObject.GetComponent<Item>().canBeHold){
                Mintman mintman = enemyHit.collider.GetComponent<Mintman>();
                rgbd.AddForce(VariousStuff.UpDirection((Vector2)transform.position) * enemyHitForce, ForceMode2D.Impulse);
                mintman.Damage();
            }
        }

        if(yInput < 0){
            Debug.Log("Should Try fall through");
            RaycastHit2D hit = Physics2D.Raycast(transform.position, (new Vector2(transform.position.x, transform.position.y - 1) - (Vector2)transform.position).normalized, groundCheckDistance, LayerMask.GetMask("Platform"));
            if(hit.collider != null){
                Debug.Log("Platform found");
                hit.collider.GetComponent<OneWayPlatform>().FallThrough();
            }
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
        /* UNCOMMENT FOR DOWN DIRECTION PICKUP
        RaycastHit2D itemHit = Physics2D.Raycast(transform.position, (new Vector2(transform.position.x, transform.position.y -1) - (Vector2)transform.position).normalized, pickupDistance, LayerMask.GetMask("Item"));
        if(itemHit.collider != null && items.Count < maxItems){
            GameObject item = itemHit.collider.gameObject;
            item.transform.GetComponent<Rigidbody2D>().simulated = false;
            item.transform.SetParent(transform.GetChild(items.Count));
            item.transform.localPosition = new Vector3(0, 0, 0);
            items.Push(item);
        }
        */
        if(nearbyItem != null && items.Count < maxItems){
            nearbyItem.transform.GetComponent<Rigidbody2D>().simulated = false;
            nearbyItem.transform.SetParent(transform.GetChild(items.Count));
            nearbyItem.transform.localPosition = new Vector3(0, 0, 0);
            items.Push(nearbyItem);
        }
        
    }
    private void Throw(InputAction.CallbackContext context){
        Debug.Log("Throw");
        if(items.Count > 0){
            GameObject item = items.Pop();
            item.transform.SetParent(null);
            item.GetComponent<Rigidbody2D>().simulated = true;
            Vector2 rightAxis = new Vector2(item.transform.position.x + 1, item.transform.position.y) - (Vector2)item.transform.position;
            Vector2 throwPoint = new Vector2(rightAxis.x * Mathf.Cos(throwAngle) - rightAxis.y * Mathf.Sin(throwAngle), 
                rightAxis.x * Mathf.Sin(throwAngle) + rightAxis.y * Mathf.Cos(throwAngle)
            );
            if(isFacingRight){
                throwPoint = new Vector2(rightAxis.x * Mathf.Cos(180f - throwAngle) - rightAxis.y * Mathf.Sin(180f - throwAngle), 
                rightAxis.x * Mathf.Sin(180f - throwAngle) + rightAxis.y * Mathf.Cos(180f - throwAngle));
            }
            Debug.Log(throwPoint);

            if(isFacingRight){
                throwPoint = new Vector2(item.transform.position.x + 0.5f, item.transform.position.y +0.5f);
            }
            else{
                throwPoint = new Vector2(item.transform.position.x - 0.5f, item.transform.position.y +0.5f);
            }

            Vector2 throwDirection = (throwPoint - (Vector2)item.transform.position).normalized;
            item.GetComponent<Rigidbody2D>().AddForce(throwDirection * throwForce, ForceMode2D.Impulse);
        }
    }
    private void ThrowStraight(InputAction.CallbackContext context){
        if(items.Count > 0){
            GameObject throwItem = items.Pop();
            throwItem.transform.SetParent(null);
            throwItem.GetComponent<Rigidbody2D>().simulated = true;
            Vector2 throwDirection = new Vector2();
            if(isFacingRight){
                throwDirection = (new Vector2(throwItem.transform.position.x +1, throwItem.transform.position.y) - (Vector2)throwItem.transform.position).normalized;
            }
            else{
                throwDirection = (new Vector2(throwItem.transform.position.x +1, throwItem.transform.position.y) - (Vector2)throwItem.transform.position).normalized;
            }
            throwItem.GetComponent<Rigidbody2D>().AddForce(throwDirection * throwForce, ForceMode2D.Impulse);
        }
    }
    private void Plant(InputAction.CallbackContext context){
        Debug.Log("Plant");
        if(isOnSoil){
            Collider2D[] nearbyTreeColliders = Physics2D.OverlapBoxAll(nextTreePosition, new Vector2(treePlantDistance * 2f, 0.1f), 0f, LayerMask.GetMask("Tree"));
            if(nearbyTreeColliders.Length == 0){
                GameObject topItem = items.Pop();
                GameObject newTree = GameObject.Instantiate(allmng.itemDataset[(int)topItem.GetComponent<Item>().type].treePrefab);
                GameObject.Destroy(topItem);
                newTree.transform.position = nextTreePosition;
            }
            else{
                // TODO: Show some popup telling the player that another tree is too close
            }
        }
    }
    private void Harvest(InputAction.CallbackContext context){
        Debug.Log("Harvest");
        RaycastHit2D hit = Physics2D.Raycast(transform.position, (new Vector2(transform.position.x, transform.position.y - 1) - (Vector2)transform.position).normalized, 2f, LayerMask.GetMask("Tree"));
        if(hit.collider != null){
            Debug.Log("Trying to harvest tree");
            hit.collider.gameObject.GetComponent<Tree>().HarvestTree();
        }
    }
    private void Move(InputAction.CallbackContext context){
        Debug.Log("Move");

    }
    private void UseItem(InputAction.CallbackContext context){
        if(items.Count > 0){
            GameObject item = items.Peek();
            switch(item.GetComponent<Item>().type){
                case Item.ItemType.apple:{
                item = items.Pop();
                GameObject.Find("Player").GetComponent<Player>().health += 1f;
                Destroy(item.gameObject);
                break;
            }
            case Item.ItemType.wateringCan:{
                WateringCan wateringCan = item.GetComponent<WateringCan>();
                if(wateringCan.currentWater > 0f){
                    Collider2D[] plantColliderGroup = Physics2D.OverlapCircleAll(transform.position, 30f, LayerMask.GetMask("Tree"));
                    if(plantColliderGroup.Length > 0){
                        float minDistance = 9999f;
                        Tree targetTree = plantColliderGroup[0].GetComponent<Tree>();
                        foreach(Collider2D plantCollider in plantColliderGroup){
                            float distanceToPlant = Vector2.Distance(plantCollider.transform.position, item.transform.position);
                            if(distanceToPlant < minDistance){
                                minDistance = distanceToPlant;
                                targetTree = plantCollider.GetComponent<Tree>();
                            }
                        }
                        // TODO: shoot
                        StartCoroutine(wateringCan.Water(targetTree));
                    }
                }
                
                
                break;
            }
            }
        }
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


    public void Damage(float damage, Vector2 damageSource, float force = 0f){
        if(!invincibleTimerRunning){
            health -= damage;
            if(health > 0f){
                invincibleTimerRunning = true;
                Vector2 toPlayerDirection = ((Vector2)transform.position - damageSource).normalized;
                Vector2 pushPoint = new Vector2(transform.position.x + toPlayerDirection.x, transform.position.y + toPlayerDirection.y);
                Vector2 pushDirection = (pushPoint - (Vector2)transform.position).normalized;
                StartCoroutine(DamageTimer(pushDirection * force));
            }
            else{
                // Death animation start
                StartCoroutine(Death());
            }

        }
    }

    private IEnumerator DamageTimer(Vector2 pushForce){
        // Damage player and push in direction
        rgbd.AddForce(pushForce, ForceMode2D.Impulse);
        float startTime = Time.time;
        float elapsedTime = 0f;
        while(Time.time - startTime < invincibleTimer){
            float t = elapsedTime / invincibleTimer;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        invincibleTimerRunning = false;
    }
    private IEnumerator Death(){
        // TODO: IMPLEMENT DEATH ANIMATION
        throw new NotImplementedException();
    }

    

    private void OnCollisionEnter2D(Collision2D other){

    }
}
