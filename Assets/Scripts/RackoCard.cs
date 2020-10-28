public class RackoCard
{
    private int value;

    // Publicly accessible read-only property that returns this card's value
    public int Value { get { return value; } }

    public RackoCard(int value)
    {
        this.value = value;
    }
}
