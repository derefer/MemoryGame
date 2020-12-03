using System.Text;
using UnityEngine;
using TMPro;

public class UiHandler : MonoBehaviour
{
    [SerializeField] private GameObject finishedPanel;
    [SerializeField] private TextMeshProUGUI timerText, guessesText;

    public void UpdateTimerDisplay(int minutes, int seconds)
    {
        timerText.text = GetTimeAsString(minutes, seconds);
    }

    private string GetTimeAsString(int minutes, int seconds)
    {
        string minutesString = ((minutes < 10) ? "0" : "") + minutes.ToString();
        string secondsString = ((seconds < 10) ? "0" : "") + seconds.ToString("f0");
        return minutesString + ":" + secondsString;
    }

    public void UpdateGuessesDisplay(int guesses)
    {
        guessesText.text = guesses.ToString();
    }

    public void DisplayFinishedPanel()
    {
        string finishedText = CreateFinishedText();
        finishedPanel.SetActive(true);
        // Keep as an example to GameObject.Find()
        GameObject.Find("FinishedPanel/FinishedText").GetComponent<TextMeshProUGUI>().text = finishedText;
    }

    private string CreateFinishedText()
    {
        StringBuilder stringBuilder = new StringBuilder("Well done!\n\nIt took you\n<b>");
        stringBuilder.Append(guessesText.text.ToString());
        stringBuilder.Append("</b> move(s) and <b>");
        stringBuilder.Append(timerText.text.ToString());
        stringBuilder.Append("</b> time\nto finish the game.");
        return stringBuilder.ToString();
    }
}
