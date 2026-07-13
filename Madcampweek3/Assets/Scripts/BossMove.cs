using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMove : MonoBehaviour
{
    Rigidbody2D rigid;
    public int nextMove;
    SpriteRenderer spriteRenderer;
    public Health health;
    CapsuleCollider2D capsuleCollider;
    
    private Animator anim;
    private Transform player;
    
    [SerializeField] float moveSpeed = 1.5f;
    [SerializeField] float chaseRange = 15f;
    
    private bool isCharging = false;
    private bool isDead = false;

    void Awake() {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        health = GetComponent<Health>();
        anim = GetComponent<Animator>();
        
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) {
            player = playerObj.transform;
        }
        
        Invoke("Think", 0);
        InvokeRepeating("AttemptCharge", 4f, 6f); // Try to charge every 6 seconds
    }

    void FixedUpdate() {
        if (isDead) return;

        // Move
        if (!isCharging) {
            // If player is close, move towards player
            if (player != null && Vector2.Distance(transform.position, player.position) < chaseRange) {
                float direction = player.position.x - transform.position.x;
                nextMove = direction > 0 ? 1 : -1;
                spriteRenderer.flipX = (nextMove == 1);
            }
            rigid.linearVelocity = new Vector2(nextMove * moveSpeed, rigid.linearVelocity.y);
        }

        if (anim != null) {
            anim.SetBool("isWalking", nextMove != 0 && !isCharging);
        }

        // Platform / Wall Check
        Vector2 frontVec = new Vector2(rigid.position.x + nextMove * 0.5f, rigid.position.y);
        Debug.DrawRay(frontVec, Vector3.down, new Color(0,1,0));
        RaycastHit2D rayHit = Physics2D.Raycast(frontVec, new Vector2(Mathf.Sign(nextMove), 0), 1.5f, LayerMask.GetMask("Platform"));
        if(rayHit.collider != null && rayHit.distance < 0.8f){
            nextMove *= -1;
            if (!isCharging) {
                CancelInvoke("Think");
                Invoke("Think", 0);
            }
        }
    }

    void Think(){
        if (isDead || isCharging) return;

        // Random movement when not chasing
        if (player == null || Vector2.Distance(transform.position, player.position) >= chaseRange) {
            nextMove = Random.Range(-1, 2);
            if(nextMove != 0)
                spriteRenderer.flipX = (nextMove == 1);
        }

        float nextThinkTime = nextMove != 0 ? Random.Range(2f, 5f) : Random.Range(0.5f, 1f);
        Invoke("Think", nextThinkTime);
    }

    void AttemptCharge() {
        if (isDead || isCharging) return;
        if (player == null || Vector2.Distance(transform.position, player.position) > chaseRange) return;

        StartCoroutine(ChargeRoutine());
    }

    IEnumerator ChargeRoutine() {
        isCharging = true;
        nextMove = 0;
        rigid.linearVelocity = new Vector2(0, rigid.linearVelocity.y);

        // Warning phase (Flash red or shake)
        float elapsed = 0f;
        while (elapsed < 0.8f) {
            spriteRenderer.color = (elapsed * 10) % 2 > 1 ? new Color(1, 0.3f, 0.3f, 1) : Color.white;
            elapsed += Time.deltaTime;
            yield return null;
        }
        spriteRenderer.color = Color.white;

        // Charge phase
        float chargeDir = player.position.x - transform.position.x > 0 ? 1 : -1;
        spriteRenderer.flipX = (chargeDir == 1);
        nextMove = (int)chargeDir;
        
        rigid.AddForce(new Vector2(chargeDir * 12f, 0), ForceMode2D.Impulse);

        yield return new WaitForSeconds(1.2f); // Dash duration
        
        isCharging = false;
        Think();
    }

    public void OnDamaged(){
        if (isDead) return;

        if(health.currentHealth <= 0) {
            isDead = true;
            CancelInvoke();
            StopAllCoroutines();

            if (anim != null) anim.SetTrigger("die");
            spriteRenderer.color = new Color(1, 1, 1, 0.4f);
            spriteRenderer.flipY = true;
            capsuleCollider.enabled = false;
            rigid.AddForce(Vector2.up * 6, ForceMode2D.Impulse);
            return;
        }

        if (anim != null) anim.SetTrigger("hurt");
        StartCoroutine(HurtFlashRoutine());
    }

    IEnumerator HurtFlashRoutine() {
        spriteRenderer.color = new Color(1, 0.4f, 0.4f, 0.6f);
        yield return new WaitForSeconds(0.2f);
        spriteRenderer.color = Color.white;
    }
}
