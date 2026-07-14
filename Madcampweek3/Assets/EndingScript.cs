using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndingScript : MonoBehaviour
{
 public Text ScriptTxt;
 public Image image;
 public Image ring;
    int cnt = 0;          
    string[] intro = new string[]{"혼자 힘으로 도토리를 왕창 모은 다람이는 따뜻한 굴안에서 겨울잠에 들었습니다.", "다람이는..","행복한 꿈을..","꾸었을까요..?"};
    private float sceneStartTime;

    // Use this for initialization
    void Start()
    {
        sceneStartTime = Time.time;
        int gameMode = PlayerPrefs.GetInt("GameMode", 0);
        if (gameMode == 1 || gameMode == 2) { // Time Attack (1) or Hell (2)
            float finalTime = 0f;
            if (GameModeManager.Instance != null) {
                finalTime = GameModeManager.Instance.elapsedTime;
            }
            int deaths = PlayerPrefs.GetInt("timeAttackDeaths", 0);
            
            string modeName = gameMode == 1 ? "타임어택" : "헬 모드";
            int minutes = (int)(finalTime / 60f);
            int seconds = (int)(finalTime % 60f);
            int fraction = (int)((finalTime * 100f) % 100f);
            string timeStr = $"{minutes:00}:{seconds:00}.{fraction:00}";

            if (gameMode == 1) {
                intro = new string[] {
                    $"축하합니다! {modeName} 클리어!",
                    $"최종 클리어 시간: {timeStr}",
                    $"총 사망 횟수: {deaths}회",
                    "스페이스바를 누르면 타이틀로 돌아갑니다."
                };
            } else {
                intro = new string[] {
                    $"축하합니다! 헬 모드 클리어!",
                    $"최종 클리어 시간: {timeStr}",
                    "단 한 번도 죽지 않고 생존에 성공했습니다!",
                    "스페이스바를 누르면 타이틀로 돌아갑니다."
                };
            }
        }
        ScriptTxt.text = intro[0];
    }
    
    // Update is called once per frame
    void Update ()
    {
        // Ignore Space key for the first 2 seconds to prevent accidental transition skips
        if (Time.time - sceneStartTime < 2.0f) return;

        if (Input.GetKeyDown(KeyCode.Space)==true)
        {
            if(cnt>=3) 
            {
                cnt = 0;
                ScriptTxt.text = "";
                StartCoroutine(FadeCoroutine());
            }
            else {
                cnt++;
                ScriptTxt.text = intro[cnt];
            }
        }
    }
    IEnumerator FadeCoroutine(){
        float fadeCount = 0;
        while(fadeCount < 1.0f){
            fadeCount += 0.01f;
            yield return new WaitForSeconds(0.01f);
            image.color = new Color(0,0,0,fadeCount);
            ring.color = new Color(255,255,255,fadeCount);
        }
        SceneManager.LoadScene("scene0");
    }
}
