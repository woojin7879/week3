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
    private bool isDeathUIActive = false;
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
            if (!isDeathUIActive) {
                isDeathUIActive = true;
                ShowDeathUI();
            }
            Invoke("PlayerReposition", 1.5f); // Increased delay slightly to show Death UI
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

    private void ShowDeathUI() {
        GameObject canvasGo = new GameObject("DeathCanvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999; // Top-most overlay
        canvasGo.AddComponent<CanvasScaler>();

        // Dark red-black background overlay
        GameObject panelGo = new GameObject("Background");
        panelGo.transform.SetParent(canvasGo.transform, false);
        Image panelImg = panelGo.AddComponent<Image>();
        panelImg.color = new Color(0.12f, 0.01f, 0.01f, 0.78f); // Transparent blood red overlay

        RectTransform panelRect = panelGo.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // "YOU DIED" / "사 망" Text
        GameObject textGo = new GameObject("DeathText");
        textGo.transform.SetParent(panelGo.transform, false);
        Text deathText = textGo.AddComponent<Text>();
        deathText.text = "사 망";
        deathText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        deathText.color = new Color(0.9f, 0.1f, 0.1f, 1f); // Bright blood red
        deathText.fontSize = 64;
        deathText.fontStyle = FontStyle.Bold;
        deathText.alignment = TextAnchor.MiddleCenter;

        RectTransform textRect = textGo.GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(500, 100);
        textRect.anchoredPosition = new Vector2(0, 30);

        // Shadow for text
        Outline outline = textGo.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, -2);

        // Subtitle text (Mode specific info)
        GameObject subGo = new GameObject("SubtitleText");
        subGo.transform.SetParent(panelGo.transform, false);
        Text subText = subGo.AddComponent<Text>();
        subText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        subText.color = Color.white;
        subText.fontSize = 20;
        subText.alignment = TextAnchor.MiddleCenter;

        RectTransform subRect = subGo.GetComponent<RectTransform>();
        subRect.sizeDelta = new Vector2(500, 80);
        subRect.anchoredPosition = new Vector2(0, -50);

        int gameMode = PlayerPrefs.GetInt("GameMode", 0);
        if (gameMode == 2) { // Hell Mode
            subText.text = "헬 모드 실패!\n처음 단계(1-1)부터 다시 시작합니다...";
            subText.color = new Color(1f, 0.7f, 0.2f, 1f); // Warning orange
        } else if (gameMode == 1) { // Time Attack
            int deaths = PlayerPrefs.GetInt("timeAttackDeaths", 0);
            subText.text = $"타임어택 재시도 중...\n현재 누적 사망: {deaths}회";
        } else { // Normal Mode
            subText.text = "구역 재시도 중...";
        }
    }
}
