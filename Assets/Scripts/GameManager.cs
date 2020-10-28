/* Rack-O
 * 
 * This application allows two or more human/AI players to play Rack-O using a user interface.
 * 
 * Name: Ben Grund
 * Last Modified: 3-14-17 
 */ 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    #region Instance

    public static GameManager Instance { get; set; }

    // Set publicly accessible static instance to this when the script is awoken
    private void Awake()
    {
        Instance = this;
    }

    #endregion

    #region UI Elements

    [SerializeField] private GameObject setupUI;
    [SerializeField] private GameObject gameUI;
    [SerializeField] private InputField numCardsInput;
    [SerializeField] private InputField numTraysSlotsInput;
    [SerializeField] private InputField AITurnDelayInput;
    [SerializeField] private Transform playersParent;
    [SerializeField] private GameObject trayUI;
    [SerializeField] private GameObject drawnCardUI;
    [SerializeField] private GameObject endGameUI;
    [SerializeField] private Text winnerText;
    [SerializeField] private CardFaceDisplay discardPileDisplay;
    [SerializeField] private CardFaceDisplay drawnCardDisplay;
    [SerializeField] private GameObject drawPileDisplay;
    [SerializeField] private Transform trayCardsDisplayParent;
    [SerializeField] private Transform trayGuideDisplayParent;

    // Text UI element for logging game events
    [SerializeField] private Text logText;

    #endregion

    #region Private Fields

    private List<Player> players;
    private int AITurnDelay;
    private int currentPlayerIdx = -1;

    #endregion

    #region Initialization

    public void InitializeGame()
    {
        RackoTray.NumTraySlots = int.Parse(numTraysSlotsInput.text);
        RackoDeck.NumCards = int.Parse(numCardsInput.text);
        AITurnDelay = int.Parse(AITurnDelayInput.text);

        // Initialize players
        players = new List<Player>();
        for (int i = 0; i < playersParent.childCount; i++)
        {
            // Use player information from UI inputs. Probably should validate player name text but I'm running out of time.
            players.Add(new Player(playersParent.GetChild(i).GetComponentInChildren<InputField>().text, playersParent.GetChild(i).GetComponentInChildren<Toggle>().isOn));
        }

        setupUI.SetActive(false);
        gameUI.SetActive(true);

        InitializeTrayDisplay();

        BeginGame();
    }

    public void BeginGame()
    {
        SetEndGameUIActive(false);

        LogMessage("\n------------------------------");

        RackoDeck.InitializeDeck();

        RackoDeck.UpdateDiscardPileDisplay(discardPileDisplay);

        LogMessage("Randomizing player order...");

        players.Shuffle();

        LogMessage("Initializing trays...");

        foreach (Player player in players)
        {
            player.InitializeTray();

            // Immediately end the game if a player's tray has been dealt in order
            if (player.IsWinner)
            {
                currentPlayerIdx = players.IndexOf(player);
                EndGame();
                return;
            }
        }

        SetTrayUIActive(false);

        LogMessage("Starting game...\n------------------------------");

        StartNextPlayerTurn();
    }

    /// <summary>
    /// Setup tray display by instantiating a <see cref="RackoTray.NumTraySlots"/> number of CardFaceDisplay objects as children of the <see cref="trayCardsDisplayParent"/>, adding <see cref="SwapCard(GameObject)"/> to their <see cref="Button"/> click event listeners, and initializing the tray guide text.
    /// </summary>
    private void InitializeTrayDisplay()
    {
        // Clear current tray display
        for(int i = 0; i < trayCardsDisplayParent.childCount; i++)
        {
            Destroy(trayCardsDisplayParent.GetChild(i));
            Destroy(trayGuideDisplayParent.GetChild(i));
        }

        for (int i = RackoTray.NumTraySlots - 1; i >= 0; i--)
        {
            GameObject cardFaceDisplay = Instantiate(Resources.Load("CardFaceDisplay"), trayCardsDisplayParent) as GameObject;
            cardFaceDisplay.GetComponent<Button>().onClick.AddListener(() => SwapCard(RackoTray.NumTraySlots - 1 - cardFaceDisplay.transform.GetSiblingIndex()));

            int guideVal = RackoTray.GetGuideValueAt(i);
            GameObject guideTextObj = Instantiate(new GameObject(guideVal.ToString(), typeof(Text)), trayGuideDisplayParent);

            Text guideText = guideTextObj.GetComponent<Text>();

            guideText.rectTransform.rect.Set(0, 0, 30, 71);
            guideText.text = guideVal.ToString();
            guideText.font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
            guideText.fontStyle = FontStyle.Normal;
            guideText.fontSize = 16;
            guideText.alignment = TextAnchor.UpperCenter;
            guideText.color = Color.white;
            guideText.raycastTarget = false;
        }
    }

    #endregion

    #region Game Status Methods

    /// <summary>
    /// Swaps drawn card with card in current player's tray at position <paramref name="trayCardIdx"/>, discards the swapped card, and ends the current player's turn
    /// </summary>
    /// <param name="trayCardIdx"></param>
    private void SwapCard(int trayCardIdx)
    {
        players[currentPlayerIdx].SwapDrawnCardAndDiscard(trayCardIdx);

        StartCoroutine(EndTurn());
    }

    private void StartNextPlayerTurn()
    {
        currentPlayerIdx = (currentPlayerIdx < players.Count - 1) ? (currentPlayerIdx + 1) : 0;

        LogMessage("\n------------------------------\n" + players[currentPlayerIdx].Name + "'s turn.");

        if (players[currentPlayerIdx].IsAI)
        {
            players[currentPlayerIdx].EmulateTurn();

            StartCoroutine(EndTurn());
        }
        else
        {
            players[currentPlayerIdx].UpdateTrayDisplay(trayCardsDisplayParent);
            SetTrayUIActive(true);
            SetTrayInteractionActive(false);
            SetCardDrawingActive(true);
            LogMessage("First, draw from either the draw pile or the discard pile by clicking on the pile you wish to draw from.");
        }
    }

    /// <summary>
    /// Ends current player's turn by disabling human-player UI if current player is human and starts the next player's turn after a delay <para/>
    /// Returns IEnumerator so it can be run as a Unity Coroutine, allowing execution of this method to be delayed
    /// </summary>
    /// <returns></returns>
    private IEnumerator EndTurn()
    {
        LogMessage("Ending " + players[currentPlayerIdx].Name + "'s turn...\n------------------------------");

        if (players[currentPlayerIdx].IsWinner)
        {
            EndGame();
        }
        else
        {
            if (RackoDeck.DrawPileCount == 0)
            {
                RackoDeck.ResetDrawPile();
            }

            RackoDeck.UpdateDrawPileDisplay(drawPileDisplay);
            RackoDeck.UpdateDiscardPileDisplay(discardPileDisplay);

            if (players[currentPlayerIdx].IsAI)
            {
                yield return new WaitForSeconds(AITurnDelay);
            }
            else
            {
                SetTrayUIActive(false);
                SetDrawnCardUIActive(false);

                yield return new WaitForSeconds(1f);
            }

            StartNextPlayerTurn();
        }
    }

    private void EndGame()
    {
        players[currentPlayerIdx].UpdateTrayDisplay(trayCardsDisplayParent);
        SetDrawnCardUIActive(false);
        SetTrayUIActive(true);
        SetTrayInteractionActive(false);
        SetEndGameUIActive(true);
    }

    #endregion

    #region UI Updates

    private void SetTrayUIActive(bool active)
    {
        trayUI.SetActive(active);
    }

    private void SetCardDrawingActive(bool active)
    {
        // Enable/disable clicks on draw and discard piles
        discardPileDisplay.GetComponent<Button>().interactable = active;
        drawPileDisplay.GetComponent<Button>().interactable = active;
    }

    public void SetTrayInteractionActive(bool active)
    {
        // Disable button clicks on each card in the tray
        for (int i = 0; i < trayCardsDisplayParent.childCount; i++)
        {
            trayCardsDisplayParent.GetChild(i).GetComponent<Button>().interactable = active;
        }
    }

    private void SetDrawnCardUIActive(bool active)
    {
        drawnCardUI.SetActive(active);
    }

    private void SetEndGameUIActive(bool active)
    {
        endGameUI.SetActive(active);

        if (active)
        {
            winnerText.text = players[currentPlayerIdx].Name;
        }
    }

    private void UpdateDrawnCardDisplay()
    {
        players[currentPlayerIdx].UpdateDrawnCardDisplay(drawnCardDisplay);

        LogMessage("Next, select from your tray the card you wish to swap or click the \"Discard\" button to discard without swapping.");
    }

    #endregion

    #region Button Clicks

    /// <summary>
    /// Adds a player data UI instance to the game setup UI
    /// </summary>
    public void AddPlayerButton_Click()
    {
        GameObject playerDataObj = Instantiate(Resources.Load("PlayerData"), playersParent) as GameObject;
        playerDataObj.GetComponentInChildren<Button>().onClick.AddListener(() => Destroy(playerDataObj));
    }

    public void DiscardButton_Click()
    {
        if (players[currentPlayerIdx].IsAI)
        {
            return;
        }

        players[currentPlayerIdx].DiscardDrawnCard();

        StartCoroutine(EndTurn());
    }

    public void DrawPileButton_Click()
    {
        if (players[currentPlayerIdx].IsAI)
        {
            return;
        }

        players[currentPlayerIdx].DrawFromDrawPile();

        UpdateDrawnCardDisplay();

        RackoDeck.UpdateDrawPileDisplay(drawPileDisplay);

        SetCardDrawingActive(false);
        SetTrayInteractionActive(true);
        SetDrawnCardUIActive(true);
    }

    public void DiscardPileButton_Click()
    {
        if (players[currentPlayerIdx].IsAI)
        {
            return;
        }

        players[currentPlayerIdx].DrawFromDiscardPile();

        UpdateDrawnCardDisplay();

        RackoDeck.UpdateDiscardPileDisplay(discardPileDisplay);

        SetCardDrawingActive(false);
        SetTrayInteractionActive(true);
        SetDrawnCardUIActive(true);
    }

    public void NewGameButton_Click()
    {
        BeginGame();
    }

    #endregion

    #region Logging

    public void LogMessage(string message)
    {
        if (logText.text.Length > 10000)
        {
            logText.text = "";
        }

        logText.text += '\n' + message;
    }

    #endregion
}