using UnityEngine;
using TMPro;

public class RaceManager : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI winnerText;

    private float raceTime = 0f;
    private bool raceActive = true;

    void Start()
    {
        raceTime = 0f;
        raceActive = true;

        if (winnerText != null)
        {
            winnerText.text = "";
        }

        UpdateTimerText();
    }

    void Update()
    {
        if (!raceActive)
        {
            return;
        }

        raceTime += Time.deltaTime;
        UpdateTimerText();
    }

    void UpdateTimerText()
    {
        if (timerText != null)
        {
            timerText.text = "Time: " + raceTime.ToString("F2");
        }
    }

    public void PlayerFinished(int playerNumber)
    {
        if (!raceActive)
        {
            return;
        }

        raceActive = false;

        if (winnerText != null)
        {
            winnerText.text = "Player " + playerNumber + " Wins!\nTime: " + raceTime.ToString("F2") + "s";
        }

        Debug.Log("Player " + playerNumber + " won in " + raceTime.ToString("F2") + " seconds");
    }

    public bool IsRaceActive()
    {
        return raceActive;
    }
}