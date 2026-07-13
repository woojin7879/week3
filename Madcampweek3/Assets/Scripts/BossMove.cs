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
    // Start is called before the first frame update
    void Awake() {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        health = GetComponent<Health>();
        Invoke("Think", 0);
    }

    // Update is called once per frame
    void FixedUpdate() {
        //Move
        rigid.linearVelocity =  new Vector2(nextMove, rigid.linearVelocity.y);

        //Platform Check
        Vector2 frontVec = new Vector2(rigid.position.x + nextMove * 0.5f, rigid.position.y);
        Debug.DrawRay(frontVec, Vector3.down, new Color(0,1,0));
        RaycastHit2D rayHit = Physics2D.Raycast(frontVec, new Vector2(Mathf.Sign(rigid.linearVelocity.x), 0), 3, LayerMask.GetMask("Platform"));
        if(rayHit.collider != null){
            if(rayHit.distance < 0.5f){
                nextMove *= -1;
                CancelInvoke();
                Invoke("Think", 0);
            }
        }
    }

    void Think(){
        //랜덤에서 최댓값은 랜덤에 포함이 안됨
        nextMove = Random.Range(-1, 2);

        float nextThinkTime;
        if(nextMove !=0)
            nextThinkTime = Random.Range(2f,5f);
        else
            nextThinkTime = Random.Range(0.5f,1f);

        Invoke("Think", nextThinkTime);
        if(nextMove !=0)
            spriteRenderer.flipX = (nextMove == 1);
    }

    public void OnDamaged(){
        if(health.currentHealth <= 0) {
            //Sprite Alpha
            spriteRenderer.color = new Color(1,1,1,0.4f);
            //Sprite Flip Y
            spriteRenderer.flipY = true;
            //Collider Disable
            capsuleCollider.enabled = false;
            //Die Effect Jump
            rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);
            return;
        }
        //Sprite Alpha
        spriteRenderer.color = new Color(1,1,1,0.4f);
        //Destroy
        Invoke("DeActive", 3);
    }

    void DeActive(){
        gameObject.SetActive(false);
    }
    
}
