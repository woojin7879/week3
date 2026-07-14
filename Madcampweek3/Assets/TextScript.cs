using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class TextScript : MonoBehaviour
{
    public Image dad;
 public Image mom;
public Text ScriptTxt;
    int cnt = 0;          
    Color color1;
    Color color2;
    string[] intro = new string[]{"옛날 옛적, 숲속 마을에 한 아기다람쥐가 살았다.", "아기다람쥐 다람이는 상냥한 부모님과 함께 행복하게 살았는데..","어느날 먹이를 구하러 나가신 부모님이 영영 돌아오지 않으셨고,","다람이는 혼자가 되었다.","혼자가 된 다람이의 첫 겨울나기를 도와주자!" };
    private float sceneStartTime;
    // Use this for initialization
    void Start()
    {
        sceneStartTime = Time.time;
        color1 = dad.color;
        color2 = mom.color;
        ScriptTxt.text = intro[0];
    }
    
    // Update is called once per frame
    void Update ()
    {
        // Ignore Space key for the first 2 seconds to prevent accidental transition skips
        if (Time.time - sceneStartTime < 2.0f) return;

        if (Input.GetKeyDown(KeyCode.Space)==true)
        {
            if(cnt>=4) 
            {
                cnt = 0;
                ScriptTxt.text = "";
                SceneManager.LoadScene("1");
            }
            else {
                cnt++;
                ScriptTxt.text = intro[cnt];
                if(cnt ==3) StartCoroutine(FadeCoroutine());
            }
        }
    }
    IEnumerator FadeCoroutine(){
        float fadeCount = 0;
        color1.a = 0.0f;
        color2.a = 0.0f;
        while(fadeCount < 1.0f){
            fadeCount += 0.01f;
            yield return new WaitForSeconds(0.01f);
            // dad.color = new Color(0,0,0,fadeCount);
            // mom.color = new Color(0,0,0,fadeCount);
            dad.color = color1;
            mom.color = color2;
        }
    }
}
