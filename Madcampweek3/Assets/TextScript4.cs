using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class TextScript4 : MonoBehaviour
{
    
public Text ScriptTxt;
    int cnt = 0;        
    Color color;
    public Image image;  
    string[] intro = new string[]{"점점 혼자 살아가는게 쉽지 않다는 사실을 느끼는 다람이", "겨울잠에 들 날이 다가오는데..","어느날 다람이의 꿈에 아버지가 나와 이렇게 말한다","다람아, 겨울엔 땅이 꽁꽁 어니 미끄러지지 않게 조심하렴.."};
    private float sceneStartTime;
    // Use this for initialization
    void Start()
    {
        sceneStartTime = Time.time;
        color = image.color;
        color.a = 0.0f;
        image.color = color;
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
                SceneManager.LoadScene("10");
            }
            else {
                cnt++;
                ScriptTxt.text = intro[cnt];
                if(cnt == 2){
                    color.a = 1.0f;
                    image.color = color;
                }
            }
        }
    }
}
