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
            Invoke("PlayerReposition",1);
        }
    }
    
    public void NextStage()
    {
    

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
        PlayerPrefs.SetInt("trycount",trycount);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
        if(stage%3 == 0) {
            if(stage == 12) SceneManager.LoadScene("ending");
            else SceneManager.LoadScene("scene1");
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
}
