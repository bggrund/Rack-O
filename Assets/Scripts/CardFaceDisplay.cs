using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This should be attached to the Unity GameObjects used for displaying a card face
/// </summary>
public class CardFaceDisplay : MonoBehaviour
{
    [SerializeField] private RectTransform cardValueSlot;
    [SerializeField] private Text cardValueText;

    /// <summary>
    /// Set cardValueText to <paramref name="cardValue"/> and set horizontal offset of <see cref="cardValueText"/> an amount proportional to <paramref name="cardValue"/>. If <paramref name="<paramref name="cardValue"/>"/> is 0, hides the card display.
    /// </summary>
    /// <param name="cardValue">Value of <see cref="RackoCard"/> to display</param>
    public void SetCard(int cardValue)
    {
        // Disable this card display if cardValue is 0
        gameObject.SetActive(cardValue > 0);

        cardValueText.text = cardValue.ToString();
        cardValueText.rectTransform.anchoredPosition = new Vector2(cardValue / (float)RackoDeck.NumCards * cardValueSlot.rect.width, cardValueText.rectTransform.anchoredPosition.y);

        // Due to a bug in Unity, setting the anchoredPosition of a UI element sometimes does not update the position of the text associated with it, but these two lines of code fixes this issue
        gameObject.transform.parent.parent.gameObject.SetActive(false);
        gameObject.transform.parent.parent.gameObject.SetActive(true);
    }
}
