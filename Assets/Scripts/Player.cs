using UnityEngine;

public class Player
{
    // Bool field and readonly property respresenting whether this player is an AI
    private bool isAI;
    public bool IsAI { get { return isAI; } }

    // String field and readonly property respresenting this player's name
    private string name;
    public string Name { get { return name; } }

    // Public readonly property that returns true if this player's tray is in ascending order
    public bool IsWinner { get { return tray.CardsInAscendingOrder(); } }

    // Drawn card, only accessible by this player
    private RackoCard drawnCard;

    // Rack-O tray, only accessible by this player
    private RackoTray tray;

    public Player(string name, bool isAI)
    {
        this.name = name;
        this.isAI = isAI;
    }

    public void InitializeTray()
    {
        tray = new RackoTray();
    }

    public void EmulateTurn()
    {
        GameManager.Instance.LogMessage("Emulating turn...");

        // The tray slot that would most benefit from a swap with the top discarded card
        int bestTrayIdx = -1;
        float bestSwapBenefit = tray.GetBestSwapBenefit(RackoDeck.TopDiscardedCardValue, out bestTrayIdx);

        float minBenefitPreference = Random.value;

        // If the greatest benefit of swapping found is greater than a randomly determined minimum benefit preference, draw from the discard pile and perform the swap.
        if (bestSwapBenefit >= minBenefitPreference)
        {
            DrawFromDiscardPile();
            SwapDrawnCardAndDiscard(bestTrayIdx);
        }
        // Otherwise, draw from the draw pile
        else
        {
            DrawFromDrawPile();

            bestSwapBenefit = tray.GetBestSwapBenefit(drawnCard.Value, out bestTrayIdx);

            // Reduce the minimum benefit preference by half when determining whether to swap with drawn card
            minBenefitPreference /= 2;

            // Perform swap if benefit is greater than new minimum benefit preference
            if (bestSwapBenefit >= minBenefitPreference)
            {
                SwapDrawnCardAndDiscard(bestTrayIdx);
            }
            // Otherwise, discard card without swapping
            else
            {
                DiscardDrawnCard();
            }
        }
    }

    public void DiscardDrawnCard()
    {
        RackoDeck.DiscardCard(drawnCard);

        drawnCard = null;

        GameManager.Instance.LogMessage(name + " discarded a(n) " + RackoDeck.TopDiscardedCardValue + '.');
    }

    public void DrawFromDrawPile()
    {
        drawnCard = RackoDeck.DrawFromDrawPile();

        GameManager.Instance.LogMessage(name + " drew from the draw pile...");
    }

    public void DrawFromDiscardPile()
    {
        drawnCard = RackoDeck.DrawFromDiscardPile();

        GameManager.Instance.LogMessage(name + " drew from the discard pile...");
    }

    public void SwapDrawnCardAndDiscard(int trayCardIdx)
    {
        tray.SwapCard(trayCardIdx, ref drawnCard);

        GameManager.Instance.LogMessage(name + " swapped the drawn card with the card at position " + (trayCardIdx + 1) + " in their tray.");

        DiscardDrawnCard();
    }

    public void UpdateTrayDisplay(Transform parent)
    {
        tray.UpdateTrayDisplay(parent);
    }

    public void UpdateDrawnCardDisplay(CardFaceDisplay cardDisplay)
    {
        cardDisplay.SetCard(drawnCard.Value);
    }
}
