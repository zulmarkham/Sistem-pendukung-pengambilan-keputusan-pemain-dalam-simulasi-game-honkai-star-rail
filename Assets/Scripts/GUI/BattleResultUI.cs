using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class BattleResultUI : MonoBehaviour
{
    public GameObject resultPanel;
    public TMP_Text resultText;

    public void ShowWin()
    {
        ShowResult(true);
    }

    public void ShowLose()
    {
        ShowResult(false);
    }

    void ShowResult(bool isWin)
    {
        resultPanel.SetActive(true);
        resultText.text = isWin ? "YOU WIN" : "YOU LOSE";

        BattleRuntime.BattleActive = false;
        Time.timeScale = 0f;
    }

    public void RetryBattle()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ExitGame()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
