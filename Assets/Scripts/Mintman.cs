using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mintman : Item
{
    Animator animator;
    SpriteRenderer renderer;
    public float sleepTime;
    public float walkSpeed;
    public float damage;
    public float wallCheckDistance;
    public float maxWallAngle;
    float currentSleepTime;
    bool isWalkingRight;
    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();
        renderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if(currentSleepTime > sleepTime && rgbd.simulated){
            animator.SetBool("isAwake", true);
            renderer.flipX = !isWalkingRight;
            canBeHold = false;
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            if(state.IsName("Walk")){
                rgbd.velocity = isWalkingRight ? new Vector2(walkSpeed, rgbd.velocity.y) : new Vector2(-walkSpeed, rgbd.velocity.y);
            }
            RaycastHit2D wallHit = new RaycastHit2D();
            Vector2 groundDirection = new Vector2();
            
            if(isWalkingRight){
                Vector2 rayStart = new Vector2(col.bounds.max.x, col.bounds.min.y + 0.1f);
                Vector2 rayDirection = (new Vector2(rayStart.x +1f, rayStart.y) - rayStart).normalized;
                wallHit = Physics2D.Raycast(rayStart, rayDirection, wallCheckDistance, LayerMask.GetMask("Default", "Ground", "Soil"));
                
                groundDirection = (new Vector2(wallHit.point.x -1f, wallHit.point.y) - wallHit.point).normalized;
            }
            else{
                Vector2 rayStart = new Vector2(col.bounds.min.x, col.bounds.min.y + 0.1f);
                Vector2 rayDirection = (new Vector2(rayStart.x -1f, rayStart.y) - rayStart).normalized;
                wallHit = Physics2D.Raycast(rayStart, rayDirection, wallCheckDistance, LayerMask.GetMask("Default", "Ground", "Soil"));
                groundDirection = (new Vector2(wallHit.point.x +1f, wallHit.point.y) - wallHit.point).normalized;
                
            }
            float angle = Vector2.Angle(wallHit.normal, groundDirection);
            Debug.DrawRay(wallHit.point, wallHit.normal, Color.green, 10f);
            Debug.DrawRay(wallHit.point, groundDirection, Color.yellow, 10f);
            if(angle < maxWallAngle && angle != 0f){
                Debug.Log(angle);
                isWalkingRight = !isWalkingRight;
            }
            
        }
        else{
            animator.SetBool("isAwake", false);
            canBeHold = true;
        }
        if(!onTree && rgbd.simulated){
            currentSleepTime += Time.deltaTime;
        }
    }

    private void OnCollisionEnter2D(Collision2D other){
        if(other.gameObject.layer == LayerMask.NameToLayer("Player")){
            other.gameObject.GetComponent<Player>().health -= damage;
        }
    }

    public void Damage(){
        currentSleepTime = 0f;
    }
}
