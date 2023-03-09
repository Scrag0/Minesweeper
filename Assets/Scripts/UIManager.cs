using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    private float time = 0f;
    private TMP_Text timerText;
    private TMP_Text minesText;
    private TMP_Text flagsText;
    private GameManager gameManager;

    public float Time { get => time; set => time = value; }

    void Awake()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        timerText = GameObject.Find("Timer Text").GetComponent<TMP_Text>();
        minesText = GameObject.Find("Mines Text").GetComponent<TMP_Text>();
        flagsText = GameObject.Find("Flags Text").GetComponent<TMP_Text>();
    }

    void Update()
    {
        if (!gameManager.IsGameOver)
        {
            Timer();
            Mines();
            Flags();
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void Timer()
    {
        Time += UnityEngine.Time.deltaTime;
        float minutes = Mathf.FloorToInt(Time / 60);
        float hours = Mathf.FloorToInt(minutes / 60);
        float seconds = Mathf.FloorToInt(Time % 60);
        timerText.text = "Time: " + hours + ":" + minutes + ":" + seconds;
    }

    private void Mines()
    {
        minesText.text = "Mines: " + gameManager.MinesAmount;
    }

    private void Flags()
    {
        flagsText.text = "Flags: " + gameManager.FlagsAmount;
    }
}
