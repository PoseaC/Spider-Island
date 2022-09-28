using UnityEngine;
using TMPro;

public class Score : MonoBehaviour
{
    public int killCount;
    public TextMeshProUGUI counterDisplay;
    public TextMeshProUGUI highscoreDisplay;

    private void Start()
    {
        counterDisplay.text = "Kill Count: 0";
    }

    public void UpdateScore()
    {
        killCount += 1;
        counterDisplay.text = "Kill Count: " + killCount;
    }

    public void SetHighScore()
    {
        if(killCount > PlayerPrefs.GetInt("Highscore", 0))
        {
            PlayerPrefs.SetInt("Highscore", killCount);
        }
        highscoreDisplay.text = "Highscore: " + PlayerPrefs.GetInt("Highscore", 0);
    }
}
