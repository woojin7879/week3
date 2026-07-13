using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ModeSelectUI : MonoBehaviour
{
    private GameObject canvasInstance;
    private int selectedIndex = 0;
    private Image[] buttonImages = new Image[4];
    private System.Action[] buttonActions = new System.Action[4];
    private Text descriptionText;

    private readonly Color normalColor = new Color(0.15f, 0.15f, 0.15f, 0.9f);
    private readonly Color highlightColor = new Color(0.4f, 0.6f, 0.9f, 1.0f); // Sleek blue highlight

    private readonly string[] modeDescriptions = new string[] {
        "스토리와 만화 컷씬을 정상 감상하며 스테이지를 즐기는 오리지널 일반 모드입니다.",
        "컷씬과 맵 선택을 모두 생략하고, 최종 클리어 시간 및 사망 횟수를 실시간 기록합니다.",
        "목숨은 단 하나! 도중에 단 한 번이라도 죽으면 1-1 단계부터 다시 강제 시작합니다.",
        "이전 메인 화면(타이틀 화면)으로 안전하게 돌아갑니다."
    };

    private void Start() {
        CreateUI();
        UpdateDescription();
    }

    private void Update() {
        // ESC key to return to title
        if (Input.GetKeyDown(KeyCode.Escape)) {
            SceneManager.LoadScene("scene0");
            return;
        }

        // Keyboard navigation
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.S)) {
            buttonImages[selectedIndex].color = normalColor;
            selectedIndex = (selectedIndex + 1) % 4;
            buttonImages[selectedIndex].color = highlightColor;
            UpdateDescription();
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.W)) {
            buttonImages[selectedIndex].color = normalColor;
            selectedIndex = (selectedIndex - 1 + 4) % 4;
            buttonImages[selectedIndex].color = highlightColor;
            UpdateDescription();
        }
        else if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) {
            if (buttonActions[selectedIndex] != null) {
                buttonActions[selectedIndex]();
            }
        }
    }

    private void UpdateDescription() {
        if (descriptionText != null) {
            descriptionText.text = modeDescriptions[selectedIndex];
        }
    }

    private void CreateUI() {
        // 1. Canvas
        canvasInstance = new GameObject("ModeSelectCanvas");
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
        panelImg.color = new Color(0, 0, 0, 0.45f); // Semi-transparent black to show the background sky

        RectTransform panelRect = panelGo.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // 3. Title Text
        GameObject titleGo = new GameObject("TitleText");
        titleGo.transform.SetParent(panelGo.transform, false);
        Text titleText = titleGo.AddComponent<Text>();
        titleText.text = "게임 모드 선택";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.color = Color.white;
        titleText.fontSize = 42;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;

        RectTransform titleRect = titleGo.GetComponent<RectTransform>();
        titleRect.sizeDelta = new Vector2(500, 80);
        titleRect.anchoredPosition = new Vector2(0, 160);

        // 4. Subtitle / Guide Text
        GameObject guideGo = new GameObject("GuideText");
        guideGo.transform.SetParent(panelGo.transform, false);
        Text guideText = guideGo.AddComponent<Text>();
        guideText.text = "방향키와 스페이스바로 선택하거나 마우스로 클릭하세요";
        guideText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        guideText.color = new Color(0.7f, 0.7f, 0.7f, 1f);
        guideText.fontSize = 18;
        guideText.alignment = TextAnchor.MiddleCenter;

        RectTransform guideRect = guideGo.GetComponent<RectTransform>();
        guideRect.sizeDelta = new Vector2(600, 40);
        guideRect.anchoredPosition = new Vector2(0, 110);

        // 5. Mode Description Display Text
        GameObject descGo = new GameObject("DescriptionText");
        descGo.transform.SetParent(panelGo.transform, false);
        descriptionText = descGo.AddComponent<Text>();
        descriptionText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        descriptionText.color = new Color(0.95f, 0.8f, 0.4f, 1f); // Premium gold
        descriptionText.fontSize = 18;
        descriptionText.alignment = TextAnchor.MiddleCenter;

        RectTransform descRect = descGo.GetComponent<RectTransform>();
        descRect.sizeDelta = new Vector2(700, 60);
        descRect.anchoredPosition = new Vector2(0, -220); // Placed at the bottom for elegant spacing

        // 6. Button Actions
        buttonActions[0] = () => {
            GameModeManager.Instance.SetMode(GameModeManager.GameMode.Normal);
            SceneManager.LoadScene("scene1");
        };

        buttonActions[1] = () => {
            GameModeManager.Instance.SetMode(GameModeManager.GameMode.TimeAttack);
            PlayerPrefs.SetInt("timeAttackDeaths", 0); // Reset death count
            SceneManager.LoadScene("scene1");
        };

        buttonActions[2] = () => {
            GameModeManager.Instance.SetMode(GameModeManager.GameMode.Hell);
            SceneManager.LoadScene("1"); // In Hell mode, start from level 1 immediately!
        };

        buttonActions[3] = () => {
            SceneManager.LoadScene("scene0"); // Back to Title Screen
        };

        // 7. Instantiate Buttons
        CreateButton(0, panelGo.transform, "일반 모드", new Vector2(0, 30));
        CreateButton(1, panelGo.transform, "타임어택 모드", new Vector2(0, -30));
        CreateButton(2, panelGo.transform, "헬 모드 (사망 시 처음부터)", new Vector2(0, -90));
        CreateButton(3, panelGo.transform, "타이틀 화면으로", new Vector2(0, -160));

        // Highlight first option
        selectedIndex = 0;
        buttonImages[selectedIndex].color = highlightColor;
    }

    private void CreateButton(int index, Transform parent, string label, Vector2 position) {
        GameObject btnGo = new GameObject("Button_" + label);
        btnGo.transform.SetParent(parent, false);

        Button btn = btnGo.AddComponent<Button>();
        Image img = btnGo.AddComponent<Image>();
        img.color = normalColor;
        btn.targetGraphic = img;
        btn.onClick.AddListener(() => {
            if (buttonActions[index] != null) {
                buttonActions[index]();
            }
        });

        buttonImages[index] = img;

        // Add PointerEnter trigger to support mouse hover selection & description updates dynamically!
        UnityEngine.EventSystems.EventTrigger trigger = btnGo.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        UnityEngine.EventSystems.EventTrigger.Entry entry = new UnityEngine.EventSystems.EventTrigger.Entry();
        entry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
        entry.callback.AddListener((data) => {
            buttonImages[selectedIndex].color = normalColor;
            selectedIndex = index;
            buttonImages[selectedIndex].color = highlightColor;
            UpdateDescription();
        });
        trigger.triggers.Add(entry);

        RectTransform rect = btnGo.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(320, 48);
        rect.anchoredPosition = position;

        GameObject textGo = new GameObject("Text");
        textGo.transform.SetParent(btnGo.transform, false);
        Text text = textGo.AddComponent<Text>();
        text.text = label;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.color = Color.white;
        text.fontSize = 20;
        text.alignment = TextAnchor.MiddleCenter;

        RectTransform textRect = textGo.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }
}
