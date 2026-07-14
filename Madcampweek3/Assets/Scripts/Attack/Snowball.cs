using UnityEngine;

public class Snowball : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    private Rigidbody2D rigid;
    private float lifetime;

    private void Awake() {
        rigid = GetComponent<Rigidbody2D>();
    }

    public void Setup(Vector2 direction, bool isFalling = false) {
        if (isFalling) {
            rigid.gravityScale = 1.5f; // Fall a bit faster
            rigid.linearVelocity = new Vector2(0, -3f); // Initial downward push
        } else {
            rigid.linearVelocity = direction.normalized * speed;
            // Add a slight vertical push for a nice throwing arc
            rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, rigid.linearVelocity.y + 3f);
        }
        lifetime = 0;
    }

    public void SetupShockwave(float directionX) {
        if (rigid == null) rigid = GetComponent<Rigidbody2D>();
        rigid.gravityScale = 0f; // Slide along ground without falling
        rigid.linearVelocity = new Vector2(directionX * speed * 0.55f, 0f); // Slower, easy to jump over (speed 5.5)
        
        // Flatten the sprite so it looks like a ground wave
        transform.localScale = new Vector3(transform.localScale.x * 2.0f, transform.localScale.y * 0.5f, transform.localScale.z);
        lifetime = 0;
    }

    private void Update() {
        lifetime += Time.deltaTime;
        if (lifetime > 5f) {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Player")) {
            Health playerHealth = collision.GetComponent<Health>();
            PlayerMove playerMove = collision.GetComponent<PlayerMove>();
            
            if (playerHealth != null && !playerHealth.hurt) {
                playerHealth.TakeDamage(1);
                if (playerMove != null) {
                    playerMove.OnDamaged(transform.position);
                }
                Destroy(gameObject);
            }
        }
        // Destroy when hitting solid terrain (Platform, Default, Fake). Passes through FloatingPlatform.
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Platform") ||
                 collision.gameObject.layer == LayerMask.NameToLayer("Default") ||
                 collision.gameObject.layer == LayerMask.NameToLayer("Fake")) {
            Destroy(gameObject);
        }
    }
}
