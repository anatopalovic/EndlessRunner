using UnityEngine;

public class GameOver : MonoBehaviour
{

    [SerializeField]
    private GameObject gameOverCanvas;

    public void StopGame(int score)
    {
        gameOverCanvas.SetActive(true);
    }

    public void RestartLevel()
    {

    }

    public void SubmitScore()
    {

    }

    public void AddXP(int score)
    {

    }
}
