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
    [SerializeField] float rageMoveSpeed = 2.5f;
    [SerializeField] float chaseRange = 18f;
    [SerializeField] private GameObject snowballPrefab;
    [SerializeField] private float[] waveYLevels = new float[] { -4.5f, -1.5f, 1.5f, 4.5f, 7.5f };
    
    private bool isCharging = false;
    private bool isDead = false;
    private float lastHurtTime = -999f;

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
        
        // Ignore collision with FloatingPlatform layer
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        int bossMoveLayer = LayerMask.NameToLayer("BossMove");
        int floatingPlatformLayer = LayerMask.NameToLayer("FloatingPlatform");
        
        Debug.Log($"[BossMove] Enemy Layer Index: {enemyLayer}, BossMove Layer Index: {bossMoveLayer}, FloatingPlatform Layer Index: {floatingPlatformLayer}");
        
        if (enemyLayer != -1 && floatingPlatformLayer != -1) {
            Physics2D.IgnoreLayerCollision(enemyLayer, floatingPlatformLayer, true);
            Debug.Log("[BossMove] Successfully ignored collision between Enemy and FloatingPlatform layers.");
        }
        if (bossMoveLayer != -1 && floatingPlatformLayer != -1) {
            Physics2D.IgnoreLayerCollision(bossMoveLayer, floatingPlatformLayer, true);
            Debug.Log("[BossMove] Successfully ignored collision between BossMove and FloatingPlatform layers.");
        }
        
        Invoke("Think", 0);
        StartCoroutine(AILoop());
    }

    void FixedUpdate() {
        if (isDead) return;

        bool isRage = isRageMode();
        float currentSpeed = isRage ? rageMoveSpeed : moveSpeed;

        // Apply persistent rage tint
        if (isRage && !isCharging && spriteRenderer.color == Color.white) {
            spriteRenderer.color = new Color(1f, 0.5f, 0.5f, 1f);
        }

        // Move
        if (!isCharging) {
            // If player is close, move towards player
            if (player != null && Vector2.Distance(transform.position, player.position) < chaseRange) {
                float direction = player.position.x - transform.position.x;
                nextMove = direction > 0 ? 1 : -1;
                spriteRenderer.flipX = (nextMove == 1);
            }
            rigid.linearVelocity = new Vector2(nextMove * currentSpeed, rigid.linearVelocity.y);
        }

        if (anim != null) {
            anim.SetBool("isWalking", nextMove != 0 && !isCharging);
        }

        // Platform / Wall Check
        Vector2 frontVec = new Vector2(rigid.position.x + nextMove * 0.5f, rigid.position.y);
        Debug.DrawRay(frontVec, Vector3.down, new Color(0,1,0));
        RaycastHit2D rayHit = Physics2D.Raycast(frontVec, new Vector2(Mathf.Sign(nextMove), 0), 1.5f, LayerMask.GetMask("Platform", "Default", "Fake", "FloatingPlatform"));
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

    bool isRageMode() {
        return health != null && health.currentHealth <= health.startingHealth / 2.0f;
    }

    IEnumerator AILoop() {
        while (!isDead) {
            float cooldown = isRageMode() ? 3.0f : 5.0f;
            yield return new WaitForSeconds(cooldown);

            if (isDead || isCharging) continue;
            if (player == null) continue;

            float dist = Vector2.Distance(transform.position, player.position);
            if (dist < chaseRange) {
                float randVal = Random.value;
                if (dist < 5.5f) {
                    // Close range: 60% Jump Slam, 20% Snowball Rain, 20% Snowball Burst
                    if (randVal < 0.6f) {
                        StartCoroutine(JumpSlamRoutine());
                    } else if (randVal < 0.8f) {
                        StartCoroutine(SnowballRainRoutine());
                    } else {
                        StartCoroutine(ThrowSnowballRoutine());
                    }
                } else {
                    // Medium/Far range: 40% Charge, 30% Snowball Rain, 30% Snowball Burst
                    if (randVal < 0.4f) {
                        StartCoroutine(ChargeRoutine());
                    } else if (randVal < 0.7f) {
                        StartCoroutine(SnowballRainRoutine());
                    } else {
                        StartCoroutine(ThrowSnowballRoutine());
                    }
                }
            }
        }
    }

    IEnumerator ChargeRoutine() {
        isCharging = true;
        nextMove = 0;
        rigid.linearVelocity = new Vector2(0, rigid.linearVelocity.y);

        // Warning phase (Flash red)
        float elapsed = 0f;
        while (elapsed < 0.8f) {
            spriteRenderer.color = (elapsed * 10) % 2 > 1 ? new Color(1, 0.2f, 0.2f, 1) : Color.white;
            elapsed += Time.deltaTime;
            yield return null;
        }
        spriteRenderer.color = Color.white;

        // Charge phase
        float chargeDir = player.position.x - transform.position.x > 0 ? 1 : -1;
        spriteRenderer.flipX = (chargeDir == 1);
        nextMove = (int)chargeDir;
        
        float dashSpeed = isRageMode() ? 19f : 12f;
        float dashDuration = 0.8f;
        elapsed = 0f;
        
        // Loop to sustain velocity regardless of mass and linear drag
        while (elapsed < dashDuration && !isDead) {
            rigid.linearVelocity = new Vector2(chargeDir * dashSpeed, rigid.linearVelocity.y);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        isCharging = false;
        Think();
    }

    IEnumerator JumpSlamRoutine() {
        isCharging = true;
        nextMove = 0;
        rigid.linearVelocity = new Vector2(0, rigid.linearVelocity.y);

        // Warning phase (Flash orange)
        float elapsed = 0f;
        while (elapsed < 0.6f) {
            spriteRenderer.color = (elapsed * 10) % 2 > 1 ? new Color(1f, 0.6f, 0f, 1) : Color.white;
            elapsed += Time.deltaTime;
            yield return null;
        }
        spriteRenderer.color = Color.white;

        // Jump towards player
        float chargeDir = player.position.x - transform.position.x > 0 ? 1 : -1;
        spriteRenderer.flipX = (chargeDir == 1);
        
        // Compensate for mass of 10 by multiplying force by rigid.mass
        rigid.AddForce(new Vector2(chargeDir * 5f * rigid.mass, 16f * rigid.mass), ForceMode2D.Impulse);

        yield return new WaitForSeconds(0.4f);
        // Wait until peak of jump and falling
        while (rigid.linearVelocity.y > 0.1f && !isDead) {
            yield return null;
        }

        if (!isDead) {
            // Slam down!
            rigid.AddForce(Vector2.down * 15f * rigid.mass, ForceMode2D.Impulse);
        }

        yield return new WaitForSeconds(0.5f); // Wait for landing impact

        // Landing shockwaves (slide along the floor left and right) at 2 random Y levels out of the array
        if (snowballPrefab != null && !isDead && waveYLevels != null && waveYLevels.Length > 0) {
            int idxL = Random.Range(0, waveYLevels.Length);
            int idxR = Random.Range(0, waveYLevels.Length);
            if (waveYLevels.Length > 1) {
                while (idxR == idxL) {
                    idxR = Random.Range(0, waveYLevels.Length);
                }
            }

            Vector3 leftSpawn = new Vector3(transform.position.x - 1.2f, waveYLevels[idxL], 0);
            Vector3 rightSpawn = new Vector3(transform.position.x + 1.2f, waveYLevels[idxR], 0);
            
            GameObject waveL = Instantiate(snowballPrefab, leftSpawn, Quaternion.identity);
            waveL.GetComponent<Snowball>().SetupShockwave(-1f);
            
            GameObject waveR = Instantiate(snowballPrefab, rightSpawn, Quaternion.identity);
            waveR.GetComponent<Snowball>().SetupShockwave(1f);
        }
        
        isCharging = false;
        Think();
    }

    IEnumerator ThrowSnowballRoutine() {
        isCharging = true;
        nextMove = 0;
        rigid.linearVelocity = new Vector2(0, rigid.linearVelocity.y);

        // Warning phase (Flash cyan/blue)
        float elapsed = 0f;
        while (elapsed < 0.6f) {
            spriteRenderer.color = (elapsed * 10) % 2 > 1 ? new Color(0.3f, 0.7f, 1f, 1) : Color.white;
            elapsed += Time.deltaTime;
            yield return null;
        }
        spriteRenderer.color = Color.white;

        // Throw 3 snowballs in sequence with a delay
        int throwCount = 3;
        for (int i = 0; i < throwCount; i++) {
            if (isDead) break;
            if (player == null) break;

            // Turn to face the player's updated position for each throw
            float dir = player.position.x - transform.position.x > 0 ? 1 : -1;
            spriteRenderer.flipX = (dir == 1);

            if (snowballPrefab != null) {
                Vector3 spawnPos = transform.position + new Vector3(dir * 1.5f, 0.5f, 0);
                GameObject sb = Instantiate(snowballPrefab, spawnPos, Quaternion.identity);
                
                Vector2 throwDir = new Vector2(player.position.x - spawnPos.x, player.position.y - spawnPos.y);
                throwDir.y += 1.5f; // Slight upward arch
                sb.GetComponent<Snowball>().Setup(throwDir);
            }

            yield return new WaitForSeconds(0.25f); // Delay between throws
        }

        yield return new WaitForSeconds(0.3f); // Brief recovery after throwing all snowballs
        isCharging = false;
        Think();
    }

    IEnumerator SnowballRainRoutine() {
        isCharging = true;
        nextMove = 0;
        rigid.linearVelocity = new Vector2(0, rigid.linearVelocity.y);

        // Warning phase (Flash white)
        float elapsed = 0f;
        while (elapsed < 0.8f) {
            spriteRenderer.color = (elapsed * 15) % 2 > 1 ? new Color(0.8f, 0.9f, 1f, 1) : Color.white;
            elapsed += Time.deltaTime;
            yield return null;
        }
        spriteRenderer.color = Color.white;

        // Rain down snowballs above the player
        int rainCount = isRageMode() ? 16 : 10;
        float spawnInterval = isRageMode() ? 0.08f : 0.15f;

        for (int i = 0; i < rainCount; i++) {
            if (isDead) break;
            if (player == null) break;

            // Spawn 9 units above player's current position with wider horizontal offset (-13 to 13)
            float offsetX = Random.Range(-13f, 13f);
            Vector3 spawnPos = new Vector3(player.position.x + offsetX, player.position.y + 9f, 0);

            if (snowballPrefab != null) {
                GameObject sb = Instantiate(snowballPrefab, spawnPos, Quaternion.identity);
                sb.GetComponent<Snowball>().Setup(Vector2.zero, true); // true for falling
            }

            yield return new WaitForSeconds(spawnInterval);
        }

        yield return new WaitForSeconds(0.4f);
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
            rigid.AddForce(Vector2.up * 6 * rigid.mass, ForceMode2D.Impulse);
            return;
        }

        if (anim != null) anim.SetTrigger("hurt");
        StartCoroutine(HurtFlashRoutine());
    }

    IEnumerator HurtFlashRoutine() {
        spriteRenderer.color = new Color(1, 0.4f, 0.4f, 0.6f);
        yield return new WaitForSeconds(0.2f);
        spriteRenderer.color = isRageMode() ? new Color(1f, 0.5f, 0.5f, 1f) : Color.white;
    }

    public bool TakeStompDamage(float damage) {
        if (isDead) return false;
        
        // Cooldown check (3 seconds)
        if (Time.time - lastHurtTime < 3.0f) {
            return false;
        }
        
        lastHurtTime = Time.time;
        health.TakeDamage(damage);
        OnDamaged();
        return true;
    }
}
