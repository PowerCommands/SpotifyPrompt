namespace PainKiller.SpotifyPromptClient.DomainObjects;

public record YearRange(int Start, int End)
{
    public int Start { get; set; } = Start;
    public int End { get; set; } = End;
    public bool IsInRange(int year) => year >= Start && year <= End;
    public override string ToString() => $"{Start}-{End}";
}