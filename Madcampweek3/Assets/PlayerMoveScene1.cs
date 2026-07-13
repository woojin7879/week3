using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerMoveScene1 : MonoBehaviour
{
    private float speed = 4f;
    public int stagenum = 1;
    Rigidbody2D rigid;

    private bool isUIActive = false;
    private GameObject uiCanvasInstance;

    private int selectedUIIndex = 0;
    private Image[] uiButtonImages = new Image[4];
    private System.Action[] uiActions = new System.Action[4];
    private bool[] isButtonEnabled = new bool[4];

    // Colors for selection state
    private readonly Color normalColor = new Color(0.15f, 0.15f, 0.15f, 0.9f);
    private readonly Color highlightColor = new Color(0.4f, 0.6f, 0.9f, 1.0f); // Sleek blue highlight

    void Awake(){
        rigid = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        stagenum = 1;
    }

    void Update()
    {
        if (isUIActive) {
            HandleUINavigation();
            return;
        }

        GameObject tempObj = null;
        int stage = PlayerPrefs.GetInt("stage", 1);

        if(Input.GetKeyDown(KeyCode.Space) == true){
            if(stagenum <= stage) {
                ShowSubStageUI();
            }
            else Debug.Log("이전 스테이지를 먼저 클리어하세요.");
        }

        if(Input.GetKeyDown(KeyCode.RightArrow) == true){
            if(stagenum < 4){
                stagenum++;
                tempObj = GameObject.Find("Stage"+stagenum.ToString());
                if(tempObj != null){
                    Debug.Log("받기 성공");
                    transform.position = Vector2.MoveTowards(transform.position, tempObj.transform.position, speed);
                }
                else{
                    Debug.Log("받기 실패");
                }
            }
        }
        else if(Input.GetKeyDown(KeyCode.LeftArrow) == true){
            if(stagenum > 1){
                stagenum--;
                tempObj = GameObject.Find("Stage"+stagenum.ToString());
                if(tempObj != null){
                    Debug.Log("받기 성공");
                    transform.position = Vector2.MoveTowards(transform.position, tempObj.transform.position, speed);
                }
                else{
                    Debug.Log("받기 실패");
                }
            }
        }
    }

    private void HandleUINavigation()
    {
        // ESC key to close sub-stage selection
        if (Input.GetKeyDown(KeyCode.Escape)) {
            CloseSubStageUI();
            return;
        }

        // Navigate down
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.RightArrow)) {
            uiButtonImages[selectedUIIndex].color = isButtonEnabled[selectedUIIndex] ? normalColor : new Color(0.1f, 0.1f, 0.1f, 0.4f);
            do {
                selectedUIIndex = (selectedUIIndex + 1) % 4;
            } while (!isButtonEnabled[selectedUIIndex]);
            uiButtonImages[selectedUIIndex].color = highlightColor;
        }
        // Navigate up
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.LeftArrow)) {
            uiButtonImages[selectedUIIndex].color = isButtonEnabled[selectedUIIndex] ? normalColor : new Color(0.1f, 0.1f, 0.1f, 0.4f);
            do {
                selectedUIIndex = (selectedUIIndex - 1 + 4) % 4;
            } while (!isButtonEnabled[selectedUIIndex]);
            uiButtonImages[selectedUIIndex].color = highlightColor;
        }
        // Select with Space
        else if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) {
            if (isButtonEnabled[selectedUIIndex] && uiActions[selectedUIIndex] != null) {
                uiActions[selectedUIIndex]();
            }
        }
    }

    private void ShowSubStageUI()
    {
        isUIActive = true;
        selectedUIIndex = 0;

        // Retrieve level progress (Normal Mode only checks highestClearedLevel)
        int stagePrefs = PlayerPrefs.GetInt("stage", 1);
        int defaultHighestCleared = (stagePrefs - 1) * 3;
        int highestClearedLevel = Mathf.Max(PlayerPrefs.GetInt("highestClearedLevel", 0), defaultHighestCleared);

        // Determine which sub-levels are unlocked
        isButtonEnabled[0] = true; // Level X-1 is always unlocked if stage group is unlocked
        isButtonEnabled[1] = ((stagenum - 1) * 3 + 1 <= highestClearedLevel); // Level X-2 is unlocked if X-1 is cleared
        isButtonEnabled[2] = ((stagenum - 1) * 3 + 2 <= highestClearedLevel); // Level X-3 is unlocked if X-2 is cleared
        isButtonEnabled[3] = true; // Cancel button is always unlocked
        
        // 1. Create Canvas
        uiCanvasInstance = new GameObject("SubStageCanvas");
        Canvas canvas = uiCanvasInstance.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        
        uiCanvasInstance.AddComponent<CanvasScaler>();
        uiCanvasInstance.AddComponent<GraphicRaycaster>();
        
        // Ensure EventSystem is present
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
        
        // 2. Panel background
        GameObject panelGo = new GameObject("Panel");
        panelGo.transform.SetParent(uiCanvasInstance.transform, false);
        Image panelImg = panelGo.AddComponent<Image>();
        panelImg.color = new Color(0, 0, 0, 0.75f);
        
        RectTransform panelRect = panelGo.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        // 3. Title Text
        GameObject titleGo = new GameObject("TitleText");
        titleGo.transform.SetParent(panelGo.transform, false);
        Text titleText = titleGo.AddComponent<Text>();
        titleText.text = $"Select Level (Stage {stagenum})";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.color = Color.white;
        titleText.fontSize = 32;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform titleRect = titleGo.GetComponent<RectTransform>();
        titleRect.sizeDelta = new Vector2(400, 80);
        titleRect.anchoredPosition = new Vector2(0, 150);
        
        // 4. Create Action delegates
        string storySceneName = "";
        if (stagenum == 1) storySceneName = "scene2";
        else if (stagenum == 2) storySceneName = "scene3";
        else if (stagenum == 3) storySceneName = "scene4";
        else if (stagenum == 4) storySceneName = "scene5";
        
        uiActions[0] = () => {
            CloseSubStageUI();
            SceneManager.LoadScene(storySceneName);
        };
        
        int lvl2 = (stagenum - 1) * 3 + 2;
        uiActions[1] = () => {
            CloseSubStageUI();
            SceneManager.LoadScene(lvl2.ToString());
        };
        
        int lvl3 = (stagenum - 1) * 3 + 3;
        uiActions[2] = () => {
            CloseSubStageUI();
            SceneManager.LoadScene(lvl3.ToString());
        };
        
        uiActions[3] = () => {
            CloseSubStageUI();
        };

        // 5. Instantiate Buttons
        CreateUIButton(0, panelGo.transform, $"Story & Level {stagenum}-1", new Vector2(0, 50), isButtonEnabled[0]);
        CreateUIButton(1, panelGo.transform, $"Level {stagenum}-2", new Vector2(0, -10), isButtonEnabled[1]);
        CreateUIButton(2, panelGo.transform, $"Level {stagenum}-3", new Vector2(0, -70), isButtonEnabled[2]);
        CreateUIButton(3, panelGo.transform, "Cancel", new Vector2(0, -140), isButtonEnabled[3]);

        // Highlight the initial selected button
        uiButtonImages[selectedUIIndex].color = highlightColor;
    }

    private void CloseSubStageUI()
    {
        if (uiCanvasInstance != null) {
            Destroy(uiCanvasInstance);
        }
        isUIActive = false;
    }

    private void CreateUIButton(int index, Transform parent, string label, Vector2 position, bool isEnabled)
    {
        GameObject btnGo = new GameObject("Button_" + label);
        btnGo.transform.SetParent(parent, false);
        
        Button btn = btnGo.AddComponent<Button>();
        Image img = btnGo.AddComponent<Image>();
        
        if (isEnabled) {
            img.color = normalColor;
            btn.targetGraphic = img;
            btn.onClick.AddListener(() => {
                if (uiActions[index] != null) {
                    uiActions[index]();
                }
            });
        } else {
            img.color = new Color(0.1f, 0.1f, 0.1f, 0.4f); // Greyed out locked button
            btn.interactable = false; // Disable button interactions
        }
        
        uiButtonImages[index] = img; // Store Image reference for highlighting

        RectTransform rect = btnGo.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(280, 45);
        rect.anchoredPosition = position;
        
        GameObject textGo = new GameObject("Text");
        textGo.transform.SetParent(btnGo.transform, false);
        Text text = textGo.AddComponent<Text>();
        text.text = isEnabled ? label : label + " (Locked)";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.color = isEnabled ? Color.white : new Color(0.6f, 0.6f, 0.6f, 0.5f);
        text.fontSize = 18;
        text.alignment = TextAnchor.MiddleCenter;
        
        RectTransform textRect = textGo.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }
}
