using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public GameObject GameOverMenuUI;

    bool gameOver;

    void Update()
    {
        if (GameFlags.GameOver)
        {
            if (GameFlags.PlayerWon)
            {
                DisplayPlayerWin();
            }
            else
            {
                DisplayGameOver();
            }
            return;
        }
    }

    public void DisplayGameOver()
    {
        if (gameOver)
            return;

        GameOverMenuUI.SetActive(true);
        var resultText = GameObject.Find("GameOverMenu/Text").GetComponent<Text>();
        resultText.text = "Game Over!";
        gameOver = true;
    }

    public void DisplayPlayerWin()
    {
        if (gameOver)
            return;

        GameOverMenuUI.SetActive(true);
        var resultText = GameObject.Find("GameOverMenu/Text").GetComponent<Text>();
        resultText.text = "You Win!!!";
        gameOver = true;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(0);
        GameFlags.GameOver = false;
        gameOver = false;
    }
}
