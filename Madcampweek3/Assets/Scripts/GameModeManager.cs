using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameModeManager : MonoBehaviour
{
    private static GameModeManager _instance;
    public static GameModeManager Instance {
        get {
            if (_instance == null) {
                _instance = FindObjectOfType<GameModeManager>();
                if (_instance == null) {
                    GameObject go = new GameObject("GameModeManager");
                    _instance = go.AddComponent<GameModeManager>();
                }
            }
            return _instance;
        }
    }

    public enum GameMode { Normal, TimeAttack, Hell }
    public GameMode currentMode = GameMode.Normal;

    public float elapsedTime = 0f;
    private bool isTimerRunning = false;

    private GameObject canvasInstance;
    private Text timerText;

    private void Awake() {
        if (_instance != null && _instance != this) {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        // Load saved mode if present
        currentMode = (GameMode)PlayerPrefs.GetInt("GameMode", 0);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update() {
        if (isTimerRunning) {
            elapsedTime += Time.deltaTime;
            UpdateTimerUI();
        }
    }

    public void SetMode(GameMode mode) {
        currentMode = mode;
        PlayerPrefs.SetInt("GameMode", (int)mode);
        
        if (mode == GameMode.Normal) {
            isTimerRunning = false;
            DestroyTimerUI();
        } else {
            elapsedTime = 0f;
            isTimerRunning = true;
            CreateTimerUI();
        }
    }

    public void ResetTimer() {
        elapsedTime = 0f;
        isTimerRunning = (currentMode != GameMode.Normal);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        // Stop timer in title screen, mode select, and ending
        string name = scene.name.ToLower();
        if (name == "scene0" || name == "mode_select" || name == "ending") {
            isTimerRunning = false;
            DestroyTimerUI();
        } else {
            // Start/resume timer for gameplay stages and map select (if in TimeAttack/Hell)
            isTimerRunning = (currentMode != GameMode.Normal);
            if (isTimerRunning) {
                CreateTimerUI();
            }
        }
    }

    private void CreateTimerUI() {
        if (canvasInstance != null) return;

        canvasInstance = new GameObject("TimerCanvas");
        Canvas canvas = canvasInstance.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000; // Always on top
        canvasInstance.AddComponent<CanvasScaler>();

        GameObject textGo = new GameObject("TimerText");
        textGo.transform.SetParent(canvasInstance.transform, false);

        timerText = textGo.AddComponent<Text>();
        timerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        timerText.color = Color.white;
        timerText.fontSize = 28;
        timerText.fontStyle = FontStyle.Bold;
        timerText.alignment = TextAnchor.UpperRight;

        // Shadow / Outline for readability
        Outline outline = textGo.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, -2);

        RectTransform rect = textGo.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(1, 1);
        rect.anchoredPosition = new Vector2(-30, -30);
        rect.sizeDelta = new Vector2(300, 50);

        UpdateTimerUI();
    }

    private void DestroyTimerUI() {
        if (canvasInstance != null) {
            Destroy(canvasInstance);
            canvasInstance = null;
            timerText = null;
        }
    }

    private void UpdateTimerUI() {
        if (timerText == null) return;

        int minutes = (int)(elapsedTime / 60f);
        int seconds = (int)(elapsedTime % 60f);
        int fraction = (int)((elapsedTime * 100f) % 100f);

        string modeLabel = currentMode == GameMode.Hell ? "헬 모드" : "타임어택";
        timerText.text = $"{modeLabel}: {minutes:00}:{seconds:00}.{fraction:00}";
    }
}
