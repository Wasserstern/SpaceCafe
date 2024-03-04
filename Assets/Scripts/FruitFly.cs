using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FruitFly : MonoBehaviour
{
    public float health;
    public float damage;
    public float damageForce;
    public float attackInterval;

    public float moveTimeInSeconds;
    public float minMoveDistance;
    public float maxMoveDistance;

    public float minMoveInterval;
    public float maxMoveInterval;
    public float damageTreshhold;

    float moveInterval;
    float attackTimer;
    float moveTimer;
    bool isMoving;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(moveTimer >= moveInterval){
            if(!isMoving){
                isMoving = true;
                StartCoroutine(Move());
                moveTimer = 0f;
                moveInterval = UnityEngine.Random.Range(minMoveInterval, maxMoveInterval);
            }
        }
        moveTimer += Time.deltaTime;
        attackTimer += Time.deltaTime;
    }

    IEnumerator Move(){
        float elapsedTime = 0f;
        float startTime = Time.time;
        bool movePositionFound = false;
        float moveDistance = UnityEngine.Random.Range(minMoveDistance, maxMoveDistance);
        float randomAngle = UnityEngine.Random.Range(0f, 360f);
        Vector2 movePosition = new Vector2();
        Vector2 currentPositon = transform.position;
        while(!movePositionFound){
            Vector2 moveDirection = (new Vector2(transform.position.x +1, transform.position.y) - (Vector2)transform.position).normalized;
            moveDirection = VariousStuff.RotateVector2(moveDirection, randomAngle);
            movePosition = currentPositon + moveDistance * moveDirection;
            
            RaycastHit2D wallHit = Physics2D.Raycast(currentPositon, moveDirection, moveDistance, LayerMask.GetMask("Ground", "Soil"));
            if(wallHit.collider == null){
                movePositionFound = true;
            }
            moveDistance /= 3;
            randomAngle += 10f;
        }
        while(Time.time - startTime < moveTimeInSeconds){
            float t = elapsedTime / moveTimeInSeconds;
            transform.position = Vector2.Lerp(currentPositon, movePosition, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        isMoving = false;
    }
    private void OnTriggerEnter2D(Collider2D other){
        if(other.gameObject.layer == LayerMask.NameToLayer("Player")){
            other.gameObject.GetComponent<Player>().Damage(damage, (Vector2)transform.position, damageForce);
        }
    }
    private void OnCollisionEnter2D(Collision2D other){

        //TODO: Remove this.
        if(other.relativeVelocity.magnitude >= damageTreshhold){
            health -= 1f;
            if(health <= 0){
                Destroy(this.gameObject);
            }
        }

    }
}
