namespace FF9.ConsoleGame.UI;

public class CommandPanel
{
    private const string AttackLabel = "Attack";
    private const string StealLabel = "Steal";
    private const string ItemLabel = "Item";
    private const string DefendLabel = "Defend";
    private const string EmptyLabel = "";
    private const string ChangeLabel = "Change";

    private readonly (int left, int top) _panelLeftTopPosition;

    public CommandPanel() : this ((0,2))
    { }

    public CommandPanel((int left, int top) panelLeftTopPosition)
    {
        _panelLeftTopPosition = panelLeftTopPosition;
    }
    
    public void DrawBattleMenu()
    {
        // Start drawing menu three lines below first line.
        Console.SetCursorPosition(_panelLeftTopPosition.left, _panelLeftTopPosition.top);
        Console.WriteLine("|---------|---------|");
        Console.WriteLine("| {0}| {1}|          ", AttackLabel.PadRight(8), DefendLabel.PadRight(8));
        Console.WriteLine("|---------|---------|          ");
        Console.WriteLine("| {0}| {1}|         ", StealLabel.PadRight(8), EmptyLabel.PadRight(8));
        Console.WriteLine("|---------|---------|          ");
        Console.WriteLine("| {0}| {1}|          ", ItemLabel.PadRight(8), ChangeLabel.PadRight(8));
        Console.WriteLine("|---------|---------|          ");
    }

    public void SetBattleMenuCursor(int left, int top)
    {
        Console.SetCursorPosition(left, top);
        Console.Write(">");
    }
}