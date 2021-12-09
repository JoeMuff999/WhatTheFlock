using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class UIManager : MonoBehaviour
{
    public static GameObject ScoreDisplay = null;
    public GameObject TimeDisplay;
    public GameObject EndScreen;

    public  GameObject DeathScreen;
    public  GameObject DeathImage;

    public static bool died = false;

    public static int PlayerScore = 0;
    public int TimeRemaining;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("gameTimer");

    }
    private void Update() {
        if(died)
        {
            ShowDeathScreen();
        }
    }

    public static void IncreaseScore()
    {
        if(!ScoreDisplay)
            ScoreDisplay = GameObject.Find("UI/ScoreDisplay");

        PlayerScore+=1;
        ScoreDisplay.GetComponent<TextMeshProUGUI>().text = "Score: " + PlayerScore;
    }

    public static void Death()
    {
        died = true;
    }
    public void ShowDeathScreen()
    {
        DeathScreen.SetActive(true);
        DeathImage.SetActive(true);
        Time.timeScale = 0;
    }    

    private IEnumerator gameTimer()
    {
        while(TimeRemaining > 0)
        {
            TimeRemaining -= 1;
            //update timer UI
            TimeDisplay.GetComponent<TextMeshProUGUI>().SetText("Time Remaining: " + TimeRemaining);
            yield return new WaitForSeconds(1);
        }
        //trigger end game stuff
        EndScreen.SetActive(true);
        DeathImage.SetActive(true);
        Time.timeScale = 0;
    }
}
