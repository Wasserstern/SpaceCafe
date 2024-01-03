using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Customer : MonoBehaviour
{
    AllManager allmng;
    Player playerScript;
    public float customerSpeed;
    public float orderRange;
    public float avoidRangeOtherCustomers;
    public float avoidRangeCafe;
    public float leaveRange;

    Vector2 cafePosition;

    bool ordered;
    bool served;

    public CafeOrder order;
    void Start()
    {
        cafePosition = GameObject.Find("Cafe").transform.position;   
        order = GetComponentInChildren<CafeOrder>();
        playerScript = GameObject.Find("Player").GetComponent<Player>();
        allmng = GameObject.Find("AllManager").GetComponent<AllManager>();
    }

    void Update()
    {
        if(!ordered){
            if(Vector2.Distance(transform.position, cafePosition) > orderRange)
            {
                transform.position = Vector2.MoveTowards(transform.position, cafePosition, customerSpeed * Time.deltaTime);
            }
            else{
                ordered = true;
                order.Activate();
            }
        }
        else if(!served){
            
            Collider2D[] nearbyCustomers = Physics2D.OverlapCircleAll(transform.position, avoidRangeOtherCustomers,LayerMask.GetMask("Customer"));
            Vector2 avoidDirection = new Vector2(0f, 0f);
            foreach(Collider2D nearbyCustomer in nearbyCustomers){
                avoidDirection += (Vector2)transform.position - (Vector2)nearbyCustomer.transform.position;
            }
            Collider2D cafeCollider = Physics2D.OverlapCircle(transform.position, avoidRangeCafe, LayerMask.GetMask("Ground"));
            if(cafeCollider != null){
                avoidDirection += (Vector2)transform.position - (Vector2)cafeCollider.transform.position;
            }
            avoidDirection = avoidDirection.normalized;
            Vector2 targetPosition = new Vector2(transform.position.x + avoidDirection.x, transform.position.y + avoidDirection.y);
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, customerSpeed * Time.deltaTime);
            
        }
        else{
            Vector2 leaveDirection = ((Vector2)transform.position - cafePosition).normalized;
            transform.position = Vector2.MoveTowards(transform.position, new Vector2(transform.position.x + leaveDirection.x, transform.position.y + leaveDirection.y), customerSpeed * Time.deltaTime);
            if(Vector2.Distance(transform.position, cafePosition) >= leaveRange){
                Destroy(this.gameObject);
            }
        }

    }
    void OnTriggerEnter2D(Collider2D other){
        if(other.gameObject.layer == LayerMask.NameToLayer("Item")){
            if(order.TryServeItem(other.gameObject.GetComponent<Item>().type)){
                //TODO: Play some animation // sound
                
                if(order.requestedItems.Count == 0){
                    served = true;
                    playerScript.coins += (int)order.orderWorth;
                    allmng.currentOrders.Remove(order);
                }
                Destroy(other.gameObject);
                
            }
        }
    }
}
