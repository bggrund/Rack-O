using UnityEngine;

public class RackoTray
{
    private static int numTraySlots;
    private static bool numTraySlotsHasBeenSet = false;

    /// <summary>
    /// Public static property representing number of tray slots available in each RackoTray, set from the GameManager script as the value specified within Unity from the inspector for the GameManager GameObject. <para/>
    /// Because of this method of assignment, the field cannot be const or readonly. To ensure that this property is only set once, a flag is raised when the value is first set which prevents further assignment attempts.
    /// </summary>
    public static int NumTraySlots
    {
        get { return numTraySlots; }
        set
        {
            if (!numTraySlotsHasBeenSet)
            {
                numTraySlots = value;
                numTraySlotsHasBeenSet = true;
            }
            else
            {
                throw new System.InvalidOperationException("NumTraySlots can only be set once");
            }
        }
    }

    // List of cards contained by this tray, where index 0 represents the bottommost card.
    private RackoCard[] cards;

    public RackoTray()
    {
        cards = new RackoCard[NumTraySlots];

        // Draw cards into tray from top to bottom
        for(int i = NumTraySlots - 1; i >= 0; i--)
        {
            cards[i] = RackoDeck.DrawFromDrawPile();
        }
    }

    /// <summary>
    /// Sets tray display's card values to those of this tray's cards
    /// </summary>
    /// <param name="parent"></param>
    public void UpdateTrayDisplay(Transform parent)
    {
        // The first child object in Unity (index = 0) is displayed as the top card, so its card value needs to be set as the value of the card at the last index in this tray's array of cards
        for(int i = 0; i < NumTraySlots; i++)
        {
            parent.GetChild(i).GetComponent<CardFaceDisplay>().SetCard(cards[cards.Length - i - 1].Value);
        }
    }

    /// <summary>
    /// Returns true if this tray's cards are in ascending order from bottom to top
    /// </summary>
    /// <returns></returns>
    public bool CardsInAscendingOrder()
    {
        for(int i = 1; i < cards.Length; i++)
        {
            if(cards[i].Value < cards[i - 1].Value)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Swaps specified <see cref="RackoCard"/> with the card at position <paramref name="trayCardIdx"/> in this tray
    /// </summary>
    /// <param name="trayCardIdx">Index of card in this tray to swap with</param>
    /// <param name="swapCard">Card to swap with another in this tray</param>
    public void SwapCard(int trayCardIdx, ref RackoCard swapCard)
    {
        RackoCard oldTrayCard = cards[trayCardIdx];
        cards[trayCardIdx] = swapCard;
        swapCard = oldTrayCard;
    }

    /// <summary>
    /// Returns value of card at specified <paramref name="idx"/>
    /// </summary>
    /// <param name="idx">Index of card in this tray</param>
    /// <returns></returns>
    public int GetCardValueAt(int idx)
    {
        if (idx < 0 || idx >= cards.Length)
        {
            GameManager.Instance.LogMessage("Error accessing card at position " + idx + " (index out of range)");

            return -1;
        }

        return cards[idx].Value;
    }

    /// <summary>
    /// Returns guide value at specified tray index
    /// </summary>
    /// <param name="trayIdx">Index in tray</param>
    /// <returns></returns>
    public static int GetGuideValueAt(int trayIdx)
    {
        return RackoDeck.NumCards * (trayIdx + 1) / (NumTraySlots + 2);
    }

    /// <summary>
    /// Returns index of position in tray that has a guide value closest to <paramref name="cardValue"/>
    /// </summary>
    /// <param name="cardValue">Value of card to determine best guide position of</param>
    /// <returns></returns>
    public static int GetGuideIndexOf(int cardValue)
    {
        // If the card's value is less than minimum guide value, its guide index is the lowest index
        if(cardValue < GetGuideValueAt(0))
        {
            return 0;
        }
        // If the card's value is greater than maximum guide value, its guide index is the highest index
        if(cardValue > GetGuideValueAt(NumTraySlots - 1))
        {
            return NumTraySlots - 1;
        }

        // Otherwise, the card's value is somewhere in the middle, so find the index of the guide value closest to the card's value
        for(int i = 0; i < NumTraySlots - 1; i++)
        {
            int lowerGuideVal = GetGuideValueAt(i);
            int upperGuideVal = GetGuideValueAt(i + 1);

            if (cardValue >= lowerGuideVal && cardValue <= upperGuideVal)
            {
                return (cardValue - lowerGuideVal < upperGuideVal - cardValue) ? lowerGuideVal : upperGuideVal;
            }
        }

        return -1;
    }

    /// <summary>
    /// Returns the benefit (as a float between -1.0 and 1.0 where 1.0 is the greatest benefit) of swapping the card at <paramref name="trayIdx"/> with a card that has a value of <paramref name="swapCardValue"/>
    /// Best case is when the value of the card currently at <paramref name="trayIdx"/> is farthest from its guide value and the <paramref name="swapCardValue"/> is equal to the guide value
    /// </summary>
    /// <param name="trayIdx">Index of card in tray that would be swapped</param>
    /// <param name="swapCardValue">Value of card that would be swapped for the card at <paramref name="trayIdx"/></param>
    /// <returns></returns>
    public float GetSwapBenefit(int swapCardValue, int trayIdx)
    {
        // Guide value at the specified tray index
        int guideVal = GetGuideValueAt(trayIdx);

        // The maximum possible difference between a card's value and the guide value at the specified index
        int maxGuideDiff = Mathf.Abs(guideVal - (RackoDeck.NumCards / 2)) + (RackoDeck.NumCards / 2);

        int currentGuideDiff = Mathf.Abs(GetCardValueAt(trayIdx) - guideVal);
        int swappedGuideDiff = Mathf.Abs(swapCardValue - guideVal);

        return (float)(currentGuideDiff - swappedGuideDiff) / maxGuideDiff;
    }

    /// <summary>
    /// Returns the greatest calculated benefit (as a float between -1.0 and 1.0 where 1.0 is the greatest benefit) of swapping a card that has a value of <paramref name="swapCardValue"/> with another in this tray
    /// </summary>
    /// <param name="swapCardValue">>Value of card that would be swapped for another in this tray</param>
    /// <param name="bestTrayIdx">Reference to int storing the index in this tray that has the greatest swap benefit</param>
    /// <returns></returns>
    public float GetBestSwapBenefit(int swapCardValue, out int bestTrayIdx)
    {
        bestTrayIdx = -1;
        float bestSwapBenefit = 0;

        // Calculate the benefit of swapping the top discarded card with each card in this player's tray and update bestSwapBenefit accordingly
        for (int i = 0; i < NumTraySlots; i++)
        {
            float swapBenefit = GetSwapBenefit(swapCardValue, i);

            if (swapBenefit > bestSwapBenefit)
            {
                bestSwapBenefit = swapBenefit;
                bestTrayIdx = i;
            }
        }

        return bestSwapBenefit;
    }
}