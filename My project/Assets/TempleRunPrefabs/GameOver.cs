using UnityEngine;
using LootLocker.Requests;
using System.Collections;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{

    [SerializeField]
    private GameObject gameOverCanvas;

    [SerializeField]
    private TMP_InputField nameInputField;

    [SerializeField]
    private TextMeshProUGUI leaderboardScoreText;

    [SerializeField]
    private TextMeshProUGUI leaderboardNameText;

    [SerializeField]
    private TextMeshProUGUI scoreText;

    private int score = 0;
    private string leaderboardId = "18683";
    private int leaderboardTopCount = 10;

    public void StopGame(int score)
    {
        gameOverCanvas.SetActive(true);
        this.score = score;
        scoreText.text = score.ToString();
        SubmitScore();
    }

    public void SubmitScore()
    {
        StartCoroutine(SubmitScoreToLeaderboard());
    }

    private IEnumerator SubmitScoreToLeaderboard()
    {
        bool nameSet = false;
        nameSet = SetPlayerNameRequest(nameSet);

        yield return new WaitUntil(() => nameSet);
        if (!nameSet) yield break;

        bool scoreSubmitted = false;
        scoreSubmitted = SubmitScoreRequest(scoreSubmitted);

        yield return new WaitUntil(() => scoreSubmitted);
        if (!scoreSubmitted) yield break;

        GetLeaderboardRequest();
    }

    private void GetLeaderboardRequest()
    {
        LootLockerSDKManager.GetScoreList(leaderboardId, leaderboardTopCount, (response) =>
        {
            if (response.success)
            {
                Debug.Log("Successfully got scores from leaderboard");

                string leaderboardName = "";
                string leaderboardScore = "";
                LootLockerLeaderboardMember[] members = response.items;

                for (int i = 0; i < members.Length; i++)
                {
                    var player = members[i].player;
                    if (player == null) { continue; }

                    leaderboardName += (player.name != "" ? player.name : player.id) + "\n";
                    leaderboardScore += members[i].score + "\n";
                }

                leaderboardNameText.SetText(leaderboardName);
                leaderboardScoreText.SetText(leaderboardScore);

                return;
            }

            Debug.Log("Failed to get scores from leaderboard.");
        });
    }

    private bool SubmitScoreRequest(bool scoreSubmitted)
    {
        LootLockerSDKManager.SubmitScore("", score, leaderboardId, response =>
        {
            if (response.success)
            {
                Debug.Log("Successfully submitted score.");
                scoreSubmitted = true;
                return;
            }

            Debug.Log("Failed to submit the score.");
        });
        return scoreSubmitted;
    }

    private bool SetPlayerNameRequest(bool nameSet)
    {
        LootLockerSDKManager.SetPlayerName(nameInputField.text, response =>
        {
            if (response.success)
            {
                Debug.Log("Successfully set player name.");
                nameSet = true;
                return;
            }

            Debug.Log("Failed to set player name.");
        });
        return nameSet;
    }

    public void AddXP(int score)
    {

    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
