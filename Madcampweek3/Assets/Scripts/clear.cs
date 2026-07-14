using UnityEngine;

public class clear : MonoBehaviour
{
    [SerializeField] BossMove bossMove;
    [SerializeField] GameObject finish;  // First finish portal
    [SerializeField] GameObject finish2; // Second finish portal
    [SerializeField] GameObject grid;    // First grid platform
    [SerializeField] GameObject grid2;   // Second grid platform
    [SerializeField] GameObject spike;   // First spike hazard
    [SerializeField] GameObject spike2;  // Second spike hazard

    // Start is called before the first frame update
    void Start()
    {
        // Deactivate all finish portals, grid platforms, and spikes at start
        if (finish != null)
        {
            finish.SetActive(false);
        }
        if (finish2 != null)
        {
            finish2.SetActive(false);
        }
        if (grid != null)
        {
            grid.SetActive(false);
        }
        if (grid2 != null)
        {
            grid2.SetActive(false);
        }
        if (spike != null)
        {
            spike.SetActive(false);
        }
        if (spike2 != null)
        {
            spike2.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (bossMove != null && bossMove.health != null)
        {
            if (bossMove.health.currentHealth <= 0)
            {
                if (finish != null)
                {
                    finish.SetActive(true); // Activate first finish portal
                }
                if (finish2 != null)
                {
                    finish2.SetActive(true); // Activate second finish portal
                }
                if (grid != null)
                {
                    grid.SetActive(true); // Activate first grid
                }
                if (grid2 != null)
                {
                    grid2.SetActive(true); // Activate second grid
                }
                if (spike != null)
                {
                    spike.SetActive(true); // Activate first spike hazard
                }
                if (spike2 != null)
                {
                    spike2.SetActive(true); // Activate second spike hazard
                }
            }
        }
    }
}
