using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class PlayerMove : MonoBehaviour
{
    public GameManager gameManager;
    [SerializeField] public float maxSpeed;
    public float jumPower;
    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    Animator anim;
    CapsuleCollider2D capsuleCollider;
    public Health health;
    [SerializeField] private LayerMask groundLayer;
    private float wallJumpCooldown;
    private float horizontalInput;
    private int cnt_hurt_frame;
    private bool isGliding;
    public int skill;
    [SerializeField] private Image fire_skilled;
    [SerializeField] private Image thunder_skilled;
    [SerializeField] private Image rock_skilled;
    public float glideCooldown;
    public int num_skill;
    public bool isTutorial1 =  false;

    [SerializeField] AudioClip jumpSound;
    [SerializeField] AudioClip walljumpSound;
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        health = GetComponent<Health>();
        gameObject.layer = 10; // Ensure player starts on Player layer to prevent self-grounding/walling on Default layer
        gameManager.cnt_dotory = 0;
        cnt_hurt_frame = 0;
        isGliding = false;
        System.Random rand = new System.Random();
        gameManager.skill = rand.Next(3);
        skill = gameManager.skill;
        if(skill == 0) fire_skilled.gameObject.SetActive(true);
        else if(skill == 1) thunder_skilled.gameObject.SetActive(true);
        else if(skill == 2) rock_skilled.gameObject.SetActive(true);
        glideCooldown = 0;
        num_skill = 3;
    }

    private void Update() {
        horizontalInput = Input.GetAxis("Horizontal");

        //Jump
        if(Input.GetButtonDown("Jump"))
                Jump();
        wallJumpCooldown += Time.deltaTime;

        //Stop speed when no input
         if(Input.GetButtonUp("Horizontal")){
            if(int.Parse(SceneManager.GetActiveScene().name) >=10) rigid.linearVelocity = new Vector2(rigid.linearVelocity.normalized.x * 0.5f, rigid.linearVelocity.y);
            else rigid.linearVelocity = new Vector2(0, rigid.linearVelocity.y);
         }

        //Direction change
        if(wallJumpCooldown > 0.7f) {
            if(horizontalInput > 0.01f)
                transform.localScale = Vector3.one;
            else if(horizontalInput < -0.01f)
                transform.localScale = new Vector3(-1, 1, 1);
        }

        //walk animation setting
        if(Mathf.Abs(rigid.linearVelocity.x)< 0.3 || isGliding)
            anim.SetBool("isWalking", false);
        else
            anim.SetBool("isWalking", true);

        if(health.hurt && cnt_hurt_frame < 100) {
            anim.SetTrigger("hurt");
            cnt_hurt_frame++;
        }
        else if(!health.hurt) cnt_hurt_frame = 0;

        if(!isGrounded() && Input.GetKey(KeyCode.G) && glideCooldown <= 1.0f) {
            glideCooldown += Time.deltaTime;
            anim.SetBool("isGliding", true);
            if(!isGliding) VelocityZero();
            isGliding = true;
            rigid.gravityScale = 0.3f;
        }
        else {
            anim.SetBool("isGliding", false);
            isGliding = false;
            rigid.gravityScale = 4;
        }

        if(wallJumpCooldown > 1.0f) {
            //Move by Control
            float h = Input.GetAxisRaw("Horizontal");
            if(h != 0){
                if(int.Parse(SceneManager.GetActiveScene().name) >=10 && isGrounded()) rigid.AddForce(new Vector2(h / 6f, 0), ForceMode2D.Impulse);
                else rigid.AddForce(new Vector2(h, 0), ForceMode2D.Impulse);
            }
            anim.SetBool("isWalljumping", false);
        }

        //Maxspeed control
        if (rigid.linearVelocity.x > maxSpeed && isGrounded())
            rigid.linearVelocity = new Vector2(maxSpeed, rigid.linearVelocity.y);
        else if (rigid.linearVelocity.x < maxSpeed * (-1) && isGrounded())
            rigid.linearVelocity = new Vector2(maxSpeed * (-1), rigid.linearVelocity.y);
        float maxAirSpeed = (wallJumpCooldown <= 1.0f) ? 15f : 5f;
        if (rigid.linearVelocity.x > maxAirSpeed)
            rigid.linearVelocity = new Vector2(maxAirSpeed, rigid.linearVelocity.y);
        else if (rigid.linearVelocity.x < maxAirSpeed * (-1))
            rigid.linearVelocity = new Vector2(maxAirSpeed * (-1), rigid.linearVelocity.y);

        //Landing Platform
        if (isGrounded() && rigid.linearVelocity.y <= 0.01f) {
            glideCooldown = 0;
            anim.SetBool("isJumping", false);
        }
    }

    private void Jump() {
        // Prevent jumping during the active knockback phase of being hurt (approx. 0.4 seconds / 25 frames)
        if (health.hurt && cnt_hurt_frame < 25) return;

        if(isGrounded()) {
            // Reset vertical velocity first to prevent cumulative force/velocity bugs (super-jumps)
            rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, 0);
            rigid.AddForce(Vector2.up * jumPower, ForceMode2D.Impulse);
            SoundManager.instance.PlaySound(jumpSound);
            anim.SetBool("isJumping", true);
        }
        else if(onWall() && !isGrounded()) {
            SoundManager.instance.PlaySound(walljumpSound);
            anim.SetBool("isWalljumping", true);
            wallJumpCooldown = 0;
            rigid.linearVelocity = new Vector2(-Mathf.Sign(transform.localScale.x)*5, 20);
            transform.localScale = new Vector3(-Mathf.Sign(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            anim.SetBool("isJumping", true);
        }
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if(collision.gameObject.tag == "Enemy"){
            bool isStomp = false;
            if (collision.contactCount > 0) {
                // If collision normal points upwards (enemy is below player), it's a stomp!
                if (collision.contacts[0].normal.y > 0.5f) {
                    isStomp = true;
                }
            }
            if(isStomp){
                OnAttack(collision.transform);
            }else{
                health.TakeDamage(1);
                OnDamaged(collision.transform.position);
            }
        }
        if(collision.gameObject.tag == "Spike"){
            health.TakeDamage(1);
            OnDamaged(collision.transform.position);
        }
        if(collision.gameObject.tag == "Boss"){
            bool isStomp = false;
            if (collision.contactCount > 0) {
                // If collision normal points upwards (boss is below player), it's a stomp!
                if (collision.contacts[0].normal.y > 0.5f) {
                    isStomp = true;
                }
            }
            if(isStomp){
                OnAttackBoss(collision.transform);
            }else{
                health.TakeDamage(1);
                OnDamaged(collision.transform.position);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision) {
        if(collision.gameObject.tag == "Item"){
            Debug.Log("Item!!!!!!!!!!!!!!!!");
            gameManager.cnt_dotory++;
            //Point Earn
            bool isBronze =  collision.gameObject.name.Contains("Bronze");
            bool isSilver =  collision.gameObject.name.Contains("Silver");
            bool isGold =  collision.gameObject.name.Contains("Gold");
            if(isBronze)
                gameManager.stagePoint += 50;
            else if(isSilver)
                gameManager.stagePoint += 100;
            else if(isGold)
                gameManager.stagePoint += 300;
            //Deactivate Item
            collision.gameObject.SetActive(false);
        }
        if(collision.gameObject.tag == "Finish" && gameManager.cnt_dotory >= 3){
            //Finish -> to Next Stage
            gameManager.NextStage();
        }
        if(collision.gameObject.tag == "Fake"){
            Debug.Log("Fake!!!!!!!!!!!!!!!!");
            if(rigid.linearVelocity.y > 0 && transform.position.y < collision.transform.position.y){
                Debug.Log("Hit!!!!!!!!!!!!!!!!!!!!");
                collision.GetComponent<BoxCollider2D>().isTrigger = false;
                collision.GetComponent<SpriteRenderer>().color = new Color(1,1,1,1);
            }
        }
        if(collision.gameObject.tag == "Tutorial1"){
            collision.gameObject.SetActive(false);
            isTutorial1 = true;
        }

        if(collision.gameObject.tag == "Tutorial2"){
            collision.gameObject.SetActive(false);
            isTutorial1 = true;
        }
    }

    void OnTriggerStay2D(Collider2D collision) {
        if(collision.gameObject.tag == "Finish" && gameManager.cnt_dotory >= 3){
            //Finish -> to Next Stage (handles cases where portal is activated while player is already inside)
            gameManager.NextStage();
        }
    }

    public void OnDamaged(Vector2 targetPos){
        //Change Layer
        gameObject.layer = 11;
        //View Alpha 피격시
        spriteRenderer.color = new Color(1,1,1,0.4f);
        //Reaction Force
        int direction = transform.position.x-targetPos.x > 0 ? 1: -1;
        rigid.AddForce(new Vector2(direction,1)*7, ForceMode2D.Impulse);

        Invoke("OffDamaged", 1.5f);
    }

    void OffDamaged(){
        health.hurt = false;
        gameObject.layer = 10;
        spriteRenderer.color = new Color(1,1,1,1);
    }

    void OnAttack(Transform enemy){
        //Point
        gameManager.stagePoint += 100;
        //Reaction Force
        rigid.AddForce(Vector2.up * 10, ForceMode2D.Impulse);

        //Enemy die
        EnemyMove enemyMove = enemy.GetComponent<EnemyMove>();
        enemyMove.health.TakeDamage(1);
        if(enemyMove.health.currentHealth <= 0)
            enemyMove.OnDamaged();
    }

    void OnAttackBoss(Transform boss){
        // Calculate bounce direction away from the boss center
        float pushDir = transform.position.x - boss.position.x >= 0 ? 1f : -1f;

        // Lock horizontal controls temporarily (for 0.5 seconds) to prevent immediate steering override
        wallJumpCooldown = 0.5f;

        // Set velocity directly for a much stronger horizontal bounce (sideways 12, upward 8)
        rigid.linearVelocity = new Vector2(pushDir * 12f, 8f);

        BossMove bossMove = boss.GetComponent<BossMove>();
        if (bossMove != null) {
            bool success = bossMove.TakeStompDamage(1);
            if (success) {
                // Point only when actual damage is dealt (not during cooldown)
                gameManager.stagePoint += 100;
            }
        }
    }

    public void VelocityZero(){
        rigid.linearVelocity = Vector2.zero;
    }

    private bool isGrounded() {
        // Platform (groundLayer), Default (0), Fake (14), and FloatingPlatform (16) layers
        int layerMask = groundLayer.value | (1 << 0) | (1 << 14) | (1 << 16);
        Vector2 size = new Vector2(capsuleCollider.bounds.size.x * 0.8f, 0.1f);
        // Offset origin slightly below the capsule collider bottom (by 0.02f) to prevent self-collision
        Vector2 origin = (Vector2)capsuleCollider.bounds.center + Vector2.down * (capsuleCollider.bounds.size.y / 2f + 0.02f);
        RaycastHit2D raycastHit = Physics2D.BoxCast(origin, size, 0, Vector2.down, 0.1f, layerMask);
        return raycastHit.collider != null;
    }

    private bool onWall() {
        // Platform (groundLayer), Default (0), Fake (14), and FloatingPlatform (16) layers
        int layerMask = groundLayer.value | (1 << 0) | (1 << 14) | (1 << 16);
        RaycastHit2D raycastHit = Physics2D.BoxCast(capsuleCollider.bounds.center, capsuleCollider.bounds.size, 0, new Vector2(transform.localScale.x, 0), 0.3f, layerMask);
        return raycastHit.collider != null;
    }

    public bool canAttack() {
        return horizontalInput == 0 && isGrounded();
    }

    public void OnDie(){
        //Sprite Alpha
        spriteRenderer.color = new Color(1,1,1,0.4f);
        //Sprite Flip Y
        spriteRenderer.flipY = true;
        //Collider Disable
        capsuleCollider.enabled = false;
        //Die Effect Jump
        //rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);

    }
}