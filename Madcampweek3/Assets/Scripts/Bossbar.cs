using UnityEngine;
using UnityEngine.UI;

public class Bossbar : MonoBehaviour
{
    [SerializeField] private BossMove bossMove;
    [SerializeField] public Slider bossbar;
    [SerializeField] private Image fillImage;

    private void Start() {
        if (fillImage == null && bossbar != null) {
            // Dynamically locate the Fill Image component of the Slider
            Transform fillArea = bossbar.transform.Find("Fill Area");
            if (fillArea != null) {
                Transform fill = fillArea.Find("Fill");
                if (fill != null) {
                    fillImage = fill.GetComponent<Image>();
                }
            }
        }
    }

    private void Update() {
        if (bossMove == null || bossMove.health == null) return;

        // Dynamic HP scaling based on current health / starting health
        float maxHp = bossMove.health.startingHealth > 0 ? bossMove.health.startingHealth : 5f;
        float imsi = (float)bossMove.health.currentHealth / maxHp;
        bossbar.value = Mathf.Lerp(bossbar.value, imsi, Time.deltaTime * 10);

        // Visual enhancement: Change health bar color to flashing red in Rage Mode!
        if (fillImage != null) {
            if (bossMove.isRageMode()) {
                // Flashing red/pink warning effect
                float pingPong = Mathf.PingPong(Time.time * 6f, 1f);
                fillImage.color = Color.Lerp(new Color(1f, 0.1f, 0.1f, 1f), new Color(1f, 0.6f, 0.6f, 1f), pingPong);
            } else {
                // Default dark red color
                fillImage.color = new Color(0.8f, 0.2f, 0.2f, 1f);
            }
        }
    }
}
