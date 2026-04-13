using TMPro;
using UnityEngine;

public class LeaderboardEntryUI : MonoBehaviour
{
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text scoreText;

    private void Awake()
    {
        AutoBindIfNeeded();
    }

    public void SetData(int rank, string playerName, int score)
    {
        AutoBindIfNeeded();

        if (rankText != null)
        {
            rankText.text = rank.ToString();
        }

        if (nameText != null)
        {
            nameText.text = playerName;
        }

        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }
    }

    private void AutoBindIfNeeded()
    {
        if (rankText != null && nameText != null && scoreText != null)
        {
            return;
        }

        TMP_Text[] allTexts = GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < allTexts.Length; i++)
        {
            TMP_Text t = allTexts[i];
            if (t == null)
            {
                continue;
            }

            string lower = t.gameObject.name.ToLowerInvariant();
            if (rankText == null && (lower.Contains("rank") || lower.Contains("stt") || lower.Contains("order") || lower.Contains("index")))
            {
                rankText = t;
                continue;
            }

            if (nameText == null && lower.Contains("name"))
            {
                nameText = t;
                continue;
            }

            if (scoreText == null && (lower.Contains("score") || lower.Contains("point") || lower.Contains("diem")))
            {
                scoreText = t;
            }
        }

        if (nameText == null && allTexts.Length > 0)
        {
            nameText = allTexts[0];
        }

        if (rankText == null && allTexts.Length > 1)
        {
            rankText = allTexts[1];
        }

        if (scoreText == null && allTexts.Length > 2)
        {
            scoreText = allTexts[2];
        }
    }
}
