using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;



public class GameManager : MonoBehaviour
{
    public int totalPoint;
    public int stagePoint;
    public int stageIndex;
    public int cnt_dotory;
    public int skill;
    public int trycount;

    public PlayerMove player;
    public GameObject[] Stages;

    [SerializeField] private Health playerHealth;

    [SerializeField] private Slider slider; 
    [SerializeField] private GameObject[] fireballs;
    [SerializeField] private GameObject[] thunders;
    [SerializeField] private GameObject[] rocks;
    public Text StageNum;
    public Color color;

    public int stage;
    private void Awake() {
        stage = int.Parse(SceneManager.GetActiveScene().name);
        color = StageNum.color;
        trycount = PlayerPrefs.GetInt("trycount",0);
        trycount++;
        PlayerPrefs.SetInt("trycount",trycount);
        StageNum.text = "Stage "+ ((stage-1)/3+1) + "-"+ ((stage-1)%3+1) +"\n"+"count: "+ (trycount);
        Invoke("SetInvisible",1.0f);
    }
    
    private void Update() {
        if(playerHealth.currentHealth == 0){
            player.OnDie();
            Invoke("PlayerReposition",1);
        }
    }
    
    public void NextStage()
    {
        // Save highest cleared level when the stage is cleared (Normal Mode only)
        if (PlayerPrefs.GetInt("GameMode", 0) == 0) {
            PlayerPrefs.SetInt("highestClearedLevel", Mathf.Max(PlayerPrefs.GetInt("highestClearedLevel", 0), stage));
        }

        //Game Clear
        //Player Control Lock
        if(stage%3 == 0){
            PlayerPrefs.SetInt("stage", stage/3+1);
            //Result UI
            Debug.Log("클리어");
            //Restart Button UI
        }
        NextPlayerReposition();
    }

    void OnTriggerEnter2D(Collider2D collision) {
        if(collision.gameObject.tag == "Player"){
            //Player Reposition
            PlayerReposition();
        }
    }

    public void PlayerReposition(){
        CancelInvoke();
        
        int gameMode = PlayerPrefs.GetInt("GameMode", 0);
        if (gameMode == 2) { // Hell Mode (2)
            if (GameModeManager.Instance != null) {
                GameModeManager.Instance.ResetTimer();
            }
            trycount = 0;
            PlayerPrefs.SetInt("trycount", 0);
            SceneManager.LoadScene("1");
        } else {
            if (gameMode == 1) { // Time Attack (1)
                int d = PlayerPrefs.GetInt("timeAttackDeaths", 0);
                PlayerPrefs.SetInt("timeAttackDeaths", d + 1);
            }
            PlayerPrefs.SetInt("trycount", trycount);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        // player.transform.position = new Vector3(0, 0, 0);
        // player.transform.localScale = Vector3.one;
        // playerHealth.currentHealth = 3;
        // player.GetComponent<Animator>().SetBool("isJumping", false);
        // player.GetComponent<Animator>().SetBool("isWalking", false);
        // player.GetComponent<Animator>().SetBool("isGliding", false);
        // player.GetComponent<Animator>().SetBool("isWalljumping", false);
        // player.VelocityZero();
        // player.glideCooldown = 0.0f;
        // for(int i = 0; i < 10; ++i) {
        //     fireballs[i].gameObject.SetActive(false);
        //     thunders[i].gameObject.SetActive(false);
        //     rocks[i].gameObject.SetActive(false);
        // }
    }

    public void NextPlayerReposition(){
        PlayerPrefs.SetInt("trycount",trycount-1);
        
        int gameMode = PlayerPrefs.GetInt("GameMode", 0);
        if(stage%3 == 0) {
            if(stage == 12) {
                // Game cleared! Update best clear times (Top 3)
                if (GameModeManager.Instance != null) {
                    float clearTime = GameModeManager.Instance.elapsedTime;
                    int deaths = PlayerPrefs.GetInt("timeAttackDeaths", 0);
                    if (gameMode == 1) { // Time Attack
                        SaveTop3Time("bestTimeAttack", clearTime, deaths);
                    } else if (gameMode == 2) { // Hell Mode
                        SaveTop3Time("bestHellTime", clearTime, 0);
                    }

                    // Save to history log (newest first)
                    string modeName = (gameMode == 1) ? "타임어택" : "헬 모드";
                    string dateStr = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                    string recordEntry = $"{modeName}|{clearTime}|{dateStr}|{deaths}";
                    
                    string history = PlayerPrefs.GetString("speedrunHistory", "");
                    if (string.IsNullOrEmpty(history)) {
                        history = recordEntry;
                    } else {
                        history = recordEntry + ";" + history;
                    }
                    PlayerPrefs.SetString("speedrunHistory", history);
                }
                SceneManager.LoadScene("ending");
            } else {
                if (gameMode == 1 || gameMode == 2) { // TimeAttack (1) or Hell (2)
                    SceneManager.LoadScene((stage+1).ToString());
                } else {
                    SceneManager.LoadScene("scene1");
                }
            }
        }
        else SceneManager.LoadScene((stage+1).ToString());
        // player.transform.position = new Vector3(0, 0, 0);
        // player.transform.localScale = Vector3.one;
        // playerHealth.currentHealth = 3;
        // player.GetComponent<Animator>().SetBool("isJumping", false);
        // player.GetComponent<Animator>().SetBool("isWalking", false);
        // player.GetComponent<Animator>().SetBool("isGliding", false);
        // player.GetComponent<Animator>().SetBool("isWalljumping", false);
        // player.VelocityZero();
        // player.glideCooldown = 0.0f;
        // for(int i = 0; i < 10; ++i) {
        //     fireballs[i].gameObject.SetActive(false);
        //     thunders[i].gameObject.SetActive(false);
        //     rocks[i].gameObject.SetActive(false);
        //}
    }
    void SetInvisible(){
        color.a = 0.0f;
        StageNum.color = color;
    }

    public void ExitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private void SaveTop3Time(string prefix, float newTime, int deaths) {
        List<System.Tuple<float, int>> records = new List<System.Tuple<float, int>>();
        for (int i = 0; i < 3; i++) {
            float val = PlayerPrefs.GetFloat(prefix + "_" + i, 999999f);
            if (val < 999998f) {
                int d = PlayerPrefs.GetInt(prefix + "_" + i + "_deaths", 0);
                records.Add(new System.Tuple<float, int>(val, d));
            }
        }
        records.Add(new System.Tuple<float, int>(newTime, deaths));
        records.Sort((a, b) => a.Item1.CompareTo(b.Item1)); // Sort by time ascending

        for (int i = 0; i < 3; i++) {
            if (i < records.Count) {
                PlayerPrefs.SetFloat(prefix + "_" + i, records[i].Item1);
                PlayerPrefs.SetInt(prefix + "_" + i + "_deaths", records[i].Item2);
            }
        }
    }
}
