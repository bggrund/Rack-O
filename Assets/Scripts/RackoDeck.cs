using System.Collections.Generic;
using UnityEngine;

public static class RackoDeck
{
    private static int numCards;
    private static bool numCardsHasBeenSet = false;

    /// <summary>
    /// Public property representing number of cards available in the deck, set from the GameManager script as the value specified within Unity from the inspector for the GameManager GameObject. <para/>
    /// Because of this method of assignment, the field cannot be const or readonly. To ensure that this property is only set once, a flag is raised when the value is first set which prevents further assignment attempts.
    /// </summary>
    public static int NumCards
    {
        get { return numCards; }
        set
        {
            if (!numCardsHasBeenSet)
            {
                numCards = value;
                numCardsHasBeenSet = true;
            }
            else
            {
                throw new System.InvalidOperationException("NumCards can only be set once");
            }
        }
    }

    // Stack of RackoCards present in the discard pile
    private static Stack<RackoCard> discardPile;

    // Stack of RackoCards present in the draw pile
    private static Stack<RackoCard> drawPile;

    // Public read-only property that returns the value of the top card in the discard pile
    public static int TopDiscardedCardValue { get { return discardPile.Count > 0 ? discardPile.Peek().Value : 0; } }

    // Public read-only property that returns the number of cards present in the draw pile
    public static int DrawPileCount { get { return drawPile.Count; } }

    /// <summary>
    /// Initializes draw pile with the number of cards specified by <see cref="NumCards"/>, shuffles the cards, pops the top card off the draw pile and pushes it onto the discard pile
    /// </summary>
    public static void InitializeDeck()
    {
        GameManager.Instance.LogMessage("Initializing Rack-O deck...");

        drawPile = new Stack<RackoCard>();
        discardPile = new Stack<RackoCard>();

        for (int i = 1; i <= NumCards; i++)
        {
            drawPile.Push(new RackoCard(i));
        }

        drawPile.Shuffle();

        discardPile.Push(drawPile.Pop());
    }

    /// <summary>
    /// Shuffles all cards in the discard pile and transfers them to the draw pile. Also pops the top card from the new draw pile to start the discard pile. This will only execute if there are no cards remaining in the draw pile.
    /// </summary>
    public static void ResetDrawPile()
    {
        if(drawPile.Count > 0)
        {
            return;
        }

        GameManager.Instance.LogMessage("Shuffling discard pile...");

        List<RackoCard> discardPileList = new List<RackoCard>(discardPile);
        discardPileList.Shuffle();

        discardPile.Clear();
        drawPile = new Stack<RackoCard>(discardPileList);

        discardPile.Push(drawPile.Pop());
    }

    public static RackoCard DrawFromDiscardPile()
    {
        return discardPile.Pop();
    }

    public static RackoCard DrawFromDrawPile()
    {
        return drawPile.Pop();
    }

    public static void DiscardCard(RackoCard card)
    {
        discardPile.Push(card);
    }

    public static void UpdateDrawPileDisplay(GameObject drawPileDisplay)
    {
        if (DrawPileCount == 0)
        {
            drawPileDisplay.SetActive(false);
        }
        else
        {
            drawPileDisplay.SetActive(true);
        }
    }

    public static void UpdateDiscardPileDisplay(CardFaceDisplay discardPileDisplay)
    {
        discardPileDisplay.SetCard(TopDiscardedCardValue);
    }
}
