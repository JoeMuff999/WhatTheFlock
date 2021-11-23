using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class UIManager : MonoBehaviour
{
    public static GameObject ScoreDisplay = null;
    public GameObject TimeDisplay;
    public GameObject EndScreen;

    public static int PlayerScore = 0;
    public int TimeRemaining;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("gameTimer");
    }

    public static void IncreaseScore()
    {
        if(!ScoreDisplay)
            ScoreDisplay = GameObject.Find("UI/ScoreDisplay");

        PlayerScore+=1;
        ScoreDisplay.GetComponent<TextMeshProUGUI>().text = "Score: " + PlayerScore;
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
    }
}
