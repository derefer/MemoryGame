using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using static System.Math;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameController : MonoBehaviour
{
    // It looks better if it's a square like 4x4...
    private const int NUM_OF_TILES = 16;
    private const int NUM_OF_CORRECT_GUESSES = NUM_OF_TILES / 2;

    [SerializeField] private Sprite backsideSprite;
    [SerializeField] private GameObject panelFinished; // TODO: Better naming
    [SerializeField] private GameObject panelPaused; // TODO: Better naming
    [SerializeField] private GameObject panelImage; // TODO: Better naming
    [SerializeField] private GameObject panelPuzzleField; // TODO: Better naming
    [SerializeField] private Transform puzzleField;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] TextMeshProUGUI timerText, movesText;

    private List<Sprite> tileSprites = new List<Sprite>();
    private List<Button> tileButtons = new List<Button>();

    private bool firstGuess, secondGuess;

    private int countGuesses;
    private int countCorrectGuesses;
    private int firstGuessIndex, secondGuessIndex;

    private string firstGuessPuzzle, secondGuessPuzzle;

    private float startTime;

    private bool isGameFinished, isGamePaused;

    private void Start()
    {
        InitSprites();
        ShuffleSprites();
        InitButtons();

        Time.timeScale = 1f;
        startTime = Time.time;
        isGameFinished = isGamePaused = false;
    }

    private void Update()
    {
        if (!isGameFinished && !isGamePaused) {
            UpdateTimer();
        }
    }

    private void UpdateTimer()
    {
        float deltaT = Time.time - startTime;
        int minutes = (int)deltaT / 60;
        int seconds = (int)deltaT % 60;
        timerText.text = GetTimeAsString(minutes, seconds);
    }

    private string GetTimeAsString(int minutes, int seconds)
    {
        string minutesString = ((minutes < 10) ? "0" : "") + minutes.ToString();
        string secondsString = ((seconds < 10) ? "0" : "") + seconds.ToString("f0");
        return minutesString + ":" + secondsString;
    }

    private void InitButtons()
    {
        for (int buttonId = 0; buttonId < NUM_OF_TILES; ++buttonId) {
            Button button = CreateButton(buttonId);
            tileButtons.Add(button);
        }
    }

    private Button CreateButton(int buttonId)
    {
        GameObject buttonGameObject = Instantiate(buttonPrefab);
        buttonGameObject.name = buttonId.ToString();
        buttonGameObject.transform.SetParent(puzzleField, false);
        Button button = buttonGameObject.GetComponent<Button>();
        button.image.sprite = backsideSprite;
        button.onClick.AddListener(() => PickAPuzzle());
        return button;
    }

    private void InitSprites()
    {
        // The "Resources" folder is a Unity convention
        Sprite[] tileSprites = Resources.LoadAll<Sprite>("Sprites/MemorySprites");
        // The backsideSprite needs to be skipped
        foreach (Sprite tileSprite in tileSprites.Skip(1)) {
            this.tileSprites.Add(tileSprite);
            this.tileSprites.Add(tileSprite);
        }
    }

    private void ShuffleSprites()
    {
        int numOfTilesToShuffle = Min(tileSprites.Count, NUM_OF_TILES);
        for (int tileSpriteIndex = 0; tileSpriteIndex < numOfTilesToShuffle; ++tileSpriteIndex) {
            Sprite temp = tileSprites[tileSpriteIndex];
            int randomIndex = Random.Range(tileSpriteIndex, numOfTilesToShuffle);
            tileSprites[tileSpriteIndex] = tileSprites[randomIndex];
            tileSprites[randomIndex] = temp;
        }
    }

    public void PickAPuzzle()
    {
        string name = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name;

        if (!firstGuess) {
            firstGuessIndex = int.Parse(name);
            firstGuess = true;
            firstGuessPuzzle = tileSprites[firstGuessIndex].name;
            tileButtons[firstGuessIndex].image.sprite = tileSprites[firstGuessIndex];
        } else if (!secondGuess) {
            secondGuessIndex = int.Parse(name);
            // Avoid double-clicking the same puzzle
            if (firstGuessIndex == secondGuessIndex) {
                return;
            }
            secondGuess = true;
            secondGuessPuzzle = tileSprites[secondGuessIndex].name;
            tileButtons[secondGuessIndex].image.sprite = tileSprites[secondGuessIndex];
            countGuesses++;
            movesText.text = countGuesses.ToString();
            StartCoroutine(CheckIfThePuzzlesMatch());
        }
    }

    private IEnumerator CheckIfThePuzzlesMatch()
    {
        yield return new WaitForSeconds(.5f);
        if (firstGuessPuzzle == secondGuessPuzzle) {
            yield return new WaitForSeconds(.1f);
            tileButtons[firstGuessIndex].interactable = false;
            tileButtons[secondGuessIndex].interactable = false;
            tileButtons[firstGuessIndex].image.color = new Color(0, 0, 0, 0);
            tileButtons[secondGuessIndex].image.color = new Color(0, 0, 0, 0);
            CheckIfTheGameIsFinished();
        } else {
            yield return new WaitForSeconds(.5f);
            tileButtons[firstGuessIndex].image.sprite = backsideSprite;
            tileButtons[secondGuessIndex].image.sprite = backsideSprite;
        }
        firstGuess = secondGuess = false;
    }

    private void CheckIfTheGameIsFinished()
    {
        countCorrectGuesses++;
        if (countCorrectGuesses == NUM_OF_CORRECT_GUESSES) {
            isGameFinished = true;
            // The order matters! TimerText must be active at this point.
            string finishedText = CreateFinishedText();
            panelFinished.SetActive(true);
            panelImage.SetActive(false);
            panelPuzzleField.SetActive(false);
            GameObject.Find("Canvas/PanelFinished/FinishedText").GetComponent<TextMeshProUGUI>().text = finishedText;
        }
    }

    private string CreateFinishedText()
    {
        string timerText = GameObject.Find("Canvas/PanelImage/TimerText").GetComponent<TextMeshProUGUI>().text.ToString();
        StringBuilder stringBuilder = new StringBuilder("Well done!\n\nIt took you\n<b>");
        stringBuilder.Append(countGuesses);
        stringBuilder.Append("</b> move(s) and <b>");
        stringBuilder.Append(timerText);
        stringBuilder.Append("</b> time\nto finish the game.");
        return stringBuilder.ToString();
    }

    public void OnPause()
    {
        panelPaused.SetActive(true);
        panelImage.SetActive(false);
        panelPuzzleField.SetActive(false);
        Time.timeScale = 0f; // Freeze the game
        isGamePaused = true;
    }

    public void OnResume()
    {
        panelPaused.SetActive(false);
        panelImage.SetActive(true);
        panelPuzzleField.SetActive(true);
        Time.timeScale = 1f;
        isGamePaused = false;
    }
}
