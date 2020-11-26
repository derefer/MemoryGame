using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameController : MonoBehaviour
{
    // It looks better if it's a square like 4x4...
    private const int NUM_OF_BUTTONS = 16;
    private const int NUM_OF_GAME_GUESSES = NUM_OF_BUTTONS / 2;

    [SerializeField] private Sprite backImage;
    [SerializeField] private GameObject exitPanel;
    [SerializeField] private GameObject panelImage; // TODO: Better naming
    [SerializeField] private GameObject panelPuzzleField; // TODO: Better naming
    [SerializeField] private Transform puzzleField;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] TextMeshProUGUI timerText, movesText;

    private Sprite[] puzzles;
    private List<Sprite> gamePuzzles = new List<Sprite>();
    private List<Button> buttons = new List<Button>();

    private bool firstGuess, secondGuess;

    private int countGuesses;
    private int countCorrectGuesses;
    private int firstGuessIndex, secondGuessIndex;

    private string firstGuessPuzzle, secondGuessPuzzle;

    private float startTime;

    private bool gameIsFinished;

    private void Start()
    {
        LoadSprites();
        InitButtons();
        InitPuzzles();
        ShufflePuzzles();

        startTime = Time.time;
        gameIsFinished = false;
    }

    private void Update()
    {
        if (!gameIsFinished) {
            UpdateTimer();
        }
    }

    private void UpdateTimer()
    {
        float deltaT = Time.time - startTime;
        int minutesInt = (int)deltaT / 60;
        int secondsInt = (int)deltaT % 60;
        string minutesString = ((minutesInt < 10) ? "0" : "") + minutesInt.ToString();
        string secondsString = ((secondsInt < 10) ? "0" : "") + secondsInt.ToString("f0");
        timerText.text = minutesString + ":" + secondsString;
    }

    private void LoadSprites()
    {
        // Strange, but the "Resources" folder seems to be some kind of Unity convention...
        puzzles = Resources.LoadAll<Sprite>("Sprites/MemorySprites");
    }

    private void InitButtons()
    {
        for (int i = 0; i < NUM_OF_BUTTONS; ++i) {
            GameObject buttonGameObject = Instantiate(buttonPrefab);
            buttonGameObject.name = "" + i;
            buttonGameObject.transform.SetParent(puzzleField, false);
            Button button = buttonGameObject.GetComponent<Button>();
            button.image.sprite = backImage;
            button.onClick.AddListener(() => PickAPuzzle());
            buttons.Add(button);
        }
    }

    private void InitPuzzles()
    {
        int j = 0;
        for (int i = 0; i < NUM_OF_BUTTONS; ++i) {
            if (j == NUM_OF_BUTTONS / 2) {
                j = 0;
            }
            gamePuzzles.Add(puzzles[j + 1]);
            ++j;
        }
    }

    private void ShufflePuzzles()
    {
        for (int i = 0; i < NUM_OF_BUTTONS; ++i) {
            Sprite temp = gamePuzzles[i];
            int randomIndex = Random.Range(i, NUM_OF_BUTTONS);
            gamePuzzles[i] = gamePuzzles[randomIndex];
            gamePuzzles[randomIndex] = temp;
        }
    }

    public void PickAPuzzle()
    {
        string name = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name;

        if (!firstGuess) {
            firstGuessIndex = int.Parse(name);
            firstGuess = true;
            firstGuessPuzzle = gamePuzzles[firstGuessIndex].name;
            buttons[firstGuessIndex].image.sprite = gamePuzzles[firstGuessIndex];
        } else if (!secondGuess) {
            secondGuessIndex = int.Parse(name);
            // Avoid double-clicking the same puzzle
            if (firstGuessIndex == secondGuessIndex) {
                return;
            }
            secondGuess = true;
            secondGuessPuzzle = gamePuzzles[secondGuessIndex].name;
            buttons[secondGuessIndex].image.sprite = gamePuzzles[secondGuessIndex];
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
            buttons[firstGuessIndex].interactable = false;
            buttons[secondGuessIndex].interactable = false;
            buttons[firstGuessIndex].image.color = new Color(0, 0, 0, 0);
            buttons[secondGuessIndex].image.color = new Color(0, 0, 0, 0);
            CheckIfTheGameIsFinished();
        } else {
            yield return new WaitForSeconds (.5f);
            buttons[firstGuessIndex].image.sprite = backImage;
            buttons[secondGuessIndex].image.sprite = backImage;
        }
        firstGuess = secondGuess = false;
    }

    private void CheckIfTheGameIsFinished()
    {
        countCorrectGuesses++;
        if (countCorrectGuesses == NUM_OF_GAME_GUESSES) {
            gameIsFinished = true;
            // This will not work if the UI element is not active...
            string timerText = GameObject.Find("Canvas/PanelImage/TimerText").GetComponent<TextMeshProUGUI>().text.ToString();
            exitPanel.SetActive(true);
            panelImage.SetActive(false);
            panelPuzzleField.SetActive(false);
            StringBuilder stringBuilder = new StringBuilder("Well done!\n\nIt took you\n<b>");
            stringBuilder.Append(countGuesses);
            stringBuilder.Append("</b> move(s) and <b>");
            stringBuilder.Append(timerText);
            stringBuilder.Append("</b> time\nto finish the game.");
            GameObject.Find("Canvas/PanelFinished/FinishedText").GetComponent<TextMeshProUGUI>().text = stringBuilder.ToString();
        }
    }
}
