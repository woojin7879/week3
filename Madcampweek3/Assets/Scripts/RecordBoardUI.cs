using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class RecordBoardUI : MonoBehaviour
{
    private GameObject canvasInstance;
    private Image backButtonImage;

    private readonly Color normalColor = new Color(0.15f, 0.15f, 0.15f, 0.9f);
    private readonly Color highlightColor = new Color(0.4f, 0.6f, 0.9f, 1.0f); // Sleek blue highlight

    private void Start() {
        MigrateLegacyRecords();
        CreateUI();
    }

    private void Update() {
        // ESC key to return to title
        if (Input.GetKeyDown(KeyCode.Escape)) {
            ReturnToTitle();
            return;
        }

        // Space or Enter to return to title (since there's only one button)
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) {
            ReturnToTitle();
        }
    }

    private void ReturnToTitle() {
        SceneManager.LoadScene("scene0");
    }

    private void MigrateLegacyRecords() {
        // Migrate legacy single record "bestTimeAttack" to "bestTimeAttack_0"
        float legacyTA = PlayerPrefs.GetFloat("bestTimeAttack", -1f);
        if (legacyTA > 0f) {
            PlayerPrefs.SetFloat("bestTimeAttack_0", legacyTA);
            PlayerPrefs.DeleteKey("bestTimeAttack");
        }

        // Migrate legacy single record "bestHellTime" to "bestHellTime_0"
        float legacyHell = PlayerPrefs.GetFloat("bestHellTime", -1f);
        if (legacyHell > 0f) {
            PlayerPrefs.SetFloat("bestHellTime_0", legacyHell);
            PlayerPrefs.DeleteKey("bestHellTime");
        }
    }

    private void CreateUI() {
        // 1. Canvas
        canvasInstance = new GameObject("RecordBoardCanvas");
        Canvas canvas = canvasInstance.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasInstance.AddComponent<CanvasScaler>();
        canvasInstance.AddComponent<GraphicRaycaster>();

        // Ensure EventSystem is present
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null) {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // 2. Background Panel
        GameObject panelGo = new GameObject("Panel");
        panelGo.transform.SetParent(canvasInstance.transform, false);
        Image panelImg = panelGo.AddComponent<Image>();
        panelImg.color = new Color(0, 0, 0, 0.45f); // Semi-transparent black overlay

        RectTransform panelRect = panelGo.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // 3. Title Text
        GameObject titleGo = new GameObject("TitleText");
        titleGo.transform.SetParent(panelGo.transform, false);
        Text titleText = titleGo.AddComponent<Text>();
        titleText.text = "스피드런 최고 기록";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.color = new Color(0.95f, 0.8f, 0.3f, 1.0f); // Premium gold
        titleText.fontSize = 40;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;

        RectTransform titleRect = titleGo.GetComponent<RectTransform>();
        titleRect.sizeDelta = new Vector2(600, 70);
        titleRect.anchoredPosition = new Vector2(0, 180);

        // 4. Columns for Top 3 Clear Times
        CreateTop3Column(panelGo.transform, "타임어택", "bestTimeAttack", new Vector2(-160, 55));
        CreateTop3Column(panelGo.transform, "헬 모드", "bestHellTime", new Vector2(160, 55));

        // 5. Speedrun History Log (Recent Runs)
        CreateHistoryPanel(panelGo.transform, new Vector2(0, -75));

        // 6. Back Button
        GameObject btnGo = new GameObject("Button_Back");
        btnGo.transform.SetParent(panelGo.transform, false);

        Button btn = btnGo.AddComponent<Button>();
        backButtonImage = btnGo.AddComponent<Image>();
        backButtonImage.color = highlightColor; // Pre-highlighted since it's the only option
        btn.targetGraphic = backButtonImage;
        btn.onClick.AddListener(ReturnToTitle);

        RectTransform rect = btnGo.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(240, 42);
        rect.anchoredPosition = new Vector2(0, -185);

        GameObject textGo = new GameObject("Text");
        textGo.transform.SetParent(btnGo.transform, false);
        Text text = textGo.AddComponent<Text>();
        text.text = "타이틀 화면으로";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.color = Color.white;
        text.fontSize = 16;
        text.alignment = TextAnchor.MiddleCenter;

        RectTransform textRect = textGo.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }

    private void CreateTop3Column(Transform parent, string header, string prefPrefix, Vector2 anchoredPos) {
        GameObject colGo = new GameObject("Column_" + header);
        colGo.transform.SetParent(parent, false);

        RectTransform colRect = colGo.AddComponent<RectTransform>();
        colRect.sizeDelta = new Vector2(300, 160);
        colRect.anchoredPosition = anchoredPos;

        // Column Header Text
        GameObject headerGo = new GameObject("Header");
        headerGo.transform.SetParent(colGo.transform, false);
        Text headerText = headerGo.AddComponent<Text>();
        headerText.text = $"{header}\n최고 기록 TOP 3";
        headerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        headerText.color = new Color(0.3f, 0.8f, 0.95f, 1f); // Sky blue column header
        headerText.fontSize = 20;
        headerText.fontStyle = FontStyle.Bold;
        headerText.alignment = TextAnchor.MiddleCenter;

        RectTransform headerRect = headerGo.GetComponent<RectTransform>();
        headerRect.sizeDelta = new Vector2(300, 50);
        headerRect.anchoredPosition = new Vector2(0, 55);

        // Fetch Top 3 Times
        for (int i = 0; i < 3; i++) {
            float time = PlayerPrefs.GetFloat(prefPrefix + "_" + i, -1f);
            string timeStr = time > 0f ? FormatTime(time) : "-";

            GameObject rowGo = new GameObject($"Row_{i}");
            rowGo.transform.SetParent(colGo.transform, false);

            Text rowText = rowGo.AddComponent<Text>();
            
            // Set medal colors: 1st (Gold), 2nd (Silver), 3rd (Bronze)
            string medalColor = "#ffffff";
            if (i == 0) medalColor = "#ffd700"; // Gold
            else if (i == 1) medalColor = "#c0c0c0"; // Silver
            else if (i == 2) medalColor = "#cd7f32"; // Bronze

            string deathSuffix = "";
            if (time > 0f && prefPrefix == "bestTimeAttack") {
                int d = PlayerPrefs.GetInt(prefPrefix + "_" + i + "_deaths", 0);
                deathSuffix = $" ({d}데스)";
            }

            rowText.text = $"<color={medalColor}>{i + 1}등</color>    {timeStr}{deathSuffix}";
            rowText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            rowText.color = Color.white;
            rowText.fontSize = 18;
            rowText.alignment = TextAnchor.MiddleCenter;
            rowText.supportRichText = true;

            RectTransform rowRect = rowGo.GetComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(300, 30);
            rowRect.anchoredPosition = new Vector2(0, 10 - i * 28);
        }
    }

    private void CreateHistoryPanel(Transform parent, Vector2 anchoredPos) {
        GameObject panelGo = new GameObject("HistoryPanel");
        panelGo.transform.SetParent(parent, false);

        RectTransform panelRect = panelGo.AddComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(600, 140);
        panelRect.anchoredPosition = anchoredPos;

        // Header Text
        GameObject headerGo = new GameObject("Header");
        headerGo.transform.SetParent(panelGo.transform, false);
        Text headerText = headerGo.AddComponent<Text>();
        headerText.text = "--- 최근 스피드런 기록 ---";
        headerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        headerText.color = new Color(0.85f, 0.85f, 0.85f, 0.9f);
        headerText.fontSize = 18;
        headerText.fontStyle = FontStyle.Bold;
        headerText.alignment = TextAnchor.MiddleCenter;

        RectTransform headerRect = headerGo.GetComponent<RectTransform>();
        headerRect.sizeDelta = new Vector2(600, 30);
        headerRect.anchoredPosition = new Vector2(0, 55);

        // Fetch History list
        string historyStr = PlayerPrefs.GetString("speedrunHistory", "");
        string[] entries = string.IsNullOrEmpty(historyStr) ? new string[0] : historyStr.Split(';');

        if (entries.Length == 0) {
            GameObject emptyGo = new GameObject("EmptyMsg");
            emptyGo.transform.SetParent(panelGo.transform, false);
            Text emptyText = emptyGo.AddComponent<Text>();
            emptyText.text = "기록된 스피드런 내역이 없습니다.";
            emptyText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            emptyText.color = new Color(0.6f, 0.6f, 0.6f, 0.8f);
            emptyText.fontSize = 16;
            emptyText.alignment = TextAnchor.MiddleCenter;

            RectTransform emptyRect = emptyGo.GetComponent<RectTransform>();
            emptyRect.sizeDelta = new Vector2(600, 30);
            emptyRect.anchoredPosition = new Vector2(0, 15);
        } else {
            // Draw up to 4 recent runs
            int showCount = Mathf.Min(4, entries.Length);
            for (int i = 0; i < showCount; i++) {
                string[] parts = entries[i].Split('|');
                if (parts.Length < 3) continue;

                string mode = parts[0];
                // Convert legacy English mode names in the list to Korean for visual unity
                if (mode == "Time Attack") mode = "타임어택";
                else if (mode == "Hell Mode") mode = "헬 모드";

                float time = float.Parse(parts[1]);
                string date = parts[2];
                int deaths = parts.Length >= 4 ? int.Parse(parts[3]) : 0;

                string timeStr = FormatTime(time);
                string deathSuffix = "";
                if (mode == "타임어택") {
                    deathSuffix = $" ({deaths}데스)";
                }

                GameObject rowGo = new GameObject($"HistoryRow_{i}");
                rowGo.transform.SetParent(panelGo.transform, false);

                Text rowText = rowGo.AddComponent<Text>();
                // Layout: [Date]  -  [Mode]  -  [Time (Deaths)]
                rowText.text = $"{date}    |    {mode,-10}    |    <color=#4fc3f7>{timeStr}</color>{deathSuffix}";
                rowText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                rowText.color = new Color(0.9f, 0.9f, 0.9f, 0.95f);
                rowText.fontSize = 16;
                rowText.alignment = TextAnchor.MiddleCenter;
                rowText.supportRichText = true;

                RectTransform rowRect = rowGo.GetComponent<RectTransform>();
                rowRect.sizeDelta = new Vector2(600, 24);
                rowRect.anchoredPosition = new Vector2(0, 20 - i * 22);
            }
        }
    }

    private string FormatTime(float time) {
        int minutes = (int)(time / 60f);
        int seconds = (int)(time % 60f);
        int fraction = (int)((time * 100f) % 100f);
        return $"{minutes:00}:{seconds:00}.{fraction:00}";
    }
}
