using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class TextScript3 : MonoBehaviour
{
    public Text ScriptTxt;
    int cnt = 0;          
    Color color;
    public Image image;
    string[] intro = new string[]{"자유롭게 하늘을 날며 여름을 즐겁게 난 다람이", "어느덧 날이 쌀쌀해지고..","어느날 다람이의 꿈에 어머니가 나와 이렇게 말한다","다람아, 눈에 보이는게 전부가 아니란다.."};
    // Use this for initialization
    void Start()
    {
        color = image.color;
        color.a = 0.0f;
        image.color = color;
        ScriptTxt.text = intro[0];
    }
    
    // Update is called once per frame
    void Update ()
    {
        if (Input.GetKeyDown(KeyCode.Space)==true)
        {
            if(cnt>=3) 
            {
                cnt = 0;
                ScriptTxt.text = "";
                SceneManager.LoadScene("7");
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
