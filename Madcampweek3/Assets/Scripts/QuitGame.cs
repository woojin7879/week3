using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class QuitGame : MonoBehaviour
{
    private static QuitGame instance;
    private GameObject canvasGo;
    private bool isPaused = false;

    // [RuntimeInitializeOnLoadMethod]를 사용하면 유니티 에디터에서 
    // 아무 작업(오브젝트 생성, 드래그 앤 드롭 등)을 하지 않아도 
    // 게임 시작 시 자동으로 이 스크립트가 실행되고 UI가 생성됩니다.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoInitialize()
    {
        if (instance != null) return;

        GameObject go = new GameObject("AutoPauseManager");
        instance = go.AddComponent<QuitGame>();
        DontDestroyOnLoad(go); // 씬이 바뀌어도 파괴되지 않고 유지됩니다.
    }

    void Start()
    {
        CreatePauseMenuUI();
    }

    void Update()
    {
        // ESC 키 입력 감지
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    // 동적으로 Canvas와 일시정지 메뉴 UI를 생성하는 코드
    private void CreatePauseMenuUI()
    {
        // 1. Canvas 생성
        canvasGo = new GameObject("AutoPauseCanvas");
        canvasGo.transform.SetParent(this.transform);
        
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; // 다른 UI보다 항상 위에 뜨도록 설정
        
        canvasGo.AddComponent<CanvasScaler>();
        canvasGo.AddComponent<GraphicRaycaster>();

        // 2. EventSystem 생성 (버튼 클릭 작동을 위해 필수)
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        // 3. 반투명 배경 패널 생성
        GameObject panelGo = new GameObject("PauseMenuPanel");
        panelGo.transform.SetParent(canvasGo.transform, false);
        
        Image panelImage = panelGo.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.65f); // 65% 불투명도 검은색

        // 패널을 화면 전체 크기로 채우기
        RectTransform panelRect = panelGo.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0);
        panelRect.anchorMax = new Vector2(1, 1);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // 4. 타이틀 텍스트 추가 ("PAUSED")
        GameObject titleGo = new GameObject("TitleText");
        titleGo.transform.SetParent(panelGo.transform, false);
        Text titleText = titleGo.AddComponent<Text>();
        titleText.text = "PAUSED";
        titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        titleText.color = Color.white;
        titleText.fontSize = 50;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;

        RectTransform titleRect = titleGo.GetComponent<RectTransform>();
        titleRect.sizeDelta = new Vector2(400, 100);
        titleRect.anchoredPosition = new Vector2(0, 150);

        // 5. 계속하기 (Resume) 버튼 생성
        CreateButton(panelGo.transform, "ResumeButton", "Resume Game", new Vector2(0, 20), Resume);

        // 6. 게임 종료 (Quit) 버튼 생성
        CreateButton(panelGo.transform, "QuitButton", "Exit Game", new Vector2(0, -50), ExitGame);

        // 기본적으로는 비활성화 상태로 둡니다.
        canvasGo.SetActive(false);
    }

    // 버튼을 코드로 동적 생성하는 헬퍼 함수
    private void CreateButton(Transform parent, string buttonName, string buttonText, Vector2 position, UnityEngine.Events.UnityAction onClickAction)
    {
        GameObject buttonGo = new GameObject(buttonName);
        buttonGo.transform.SetParent(parent, false);

        Button button = buttonGo.AddComponent<Button>();
        Image buttonImage = buttonGo.AddComponent<Image>();
        buttonImage.color = new Color(0.15f, 0.15f, 0.15f, 0.9f); // 어두운 회색 버튼
        button.targetGraphic = buttonImage;
        button.onClick.AddListener(onClickAction);

        // 버튼 크기 및 위치 설정
        RectTransform rect = buttonGo.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(250, 50);
        rect.anchoredPosition = position;

        // 버튼 텍스트 추가
        GameObject textGo = new GameObject("Text");
        textGo.transform.SetParent(buttonGo.transform, false);
        Text text = textGo.AddComponent<Text>();
        text.text = buttonText;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.color = Color.white;
        text.fontSize = 20;
        text.alignment = TextAnchor.MiddleCenter;

        // 텍스트를 버튼 크기에 맞춤
        RectTransform textRect = textGo.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }

    // 게임 계속하기
    public void Resume()
    {
        if (canvasGo != null)
        {
            canvasGo.SetActive(false);
        }
        Time.timeScale = 1f;
        isPaused = false;
    }

    // 게임 일시정지
    public void Pause()
    {
        if (canvasGo != null)
        {
            canvasGo.SetActive(true);
        }
        Time.timeScale = 0f;
        isPaused = true;
    }

    // 게임 종료
    public void ExitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
