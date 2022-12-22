namespace FF9.ConsoleGame.UI;

public class CommandPanel
{
    private const string AttackLabel = "Attack";
    private const string StealLabel = "Steal";
    private const string ItemLabel = "Item";
    private const string DefendLabel = "Defend";
    private const string EmptyLabel = "";
    private const string ChangeLabel = "Change";

    private (int left, int top) _cursorPosition;
    private readonly (int left, int top) _panelPosition;
    private readonly int _panelPositionRight;
    private readonly (int left, int top) _initialCursorPosition;

    public BattleAction? CurrentPlayerAction { get; private set; } = BattleAction.Attack;
    public bool IsVisible { get; private set; }

    public CommandPanel((int left, int top) panelPosition)
    {
        _panelPosition = panelPosition;
        _cursorPosition = (_panelPosition.left + 1, _panelPosition.top + 1);
        _initialCursorPosition = _cursorPosition;
        _panelPositionRight = _panelPosition.left + 21;
    }

    public void Draw()
    {
        // Start drawing menu three lines below first line.
        Console.SetCursorPosition(_panelPosition.left, _panelPosition.top);
        Console.Write("|---------|---------|");

        Console.SetCursorPosition(_panelPosition.left, _panelPosition.top + 1);
        Console.Write("| {0}| {1}|         ", AttackLabel.PadRight(8), DefendLabel.PadRight(8));

        Console.SetCursorPosition(_panelPosition.left, _panelPosition.top + 2);
        Console.Write("|---------|---------|         ");

        Console.SetCursorPosition(_panelPosition.left, _panelPosition.top + 3);
        Console.Write("| {0}| {1}|         ", StealLabel.PadRight(8), EmptyLabel.PadRight(8));

        Console.SetCursorPosition(_panelPosition.left, _panelPosition.top + 4);
        Console.Write("|---------|---------|         ");

        Console.SetCursorPosition(_panelPosition.left, _panelPosition.top + 5);
        Console.Write("| {0}| {1}|         ", ItemLabel.PadRight(8), ChangeLabel.PadRight(8));

        Console.SetCursorPosition(_panelPosition.left, _panelPosition.top + 6);
        Console.Write("|---------|---------|         ");

        SetCursorAtInitialPositionInBattleMenu();
        IsVisible = true;
    }

    private void SetCursorAtInitialPositionInBattleMenu()
    {
        SetBattleMenuCursor(_cursorPosition.left, _cursorPosition.top);
    }

    private void SetBattleMenuCursor(int left, int top)
    {
        Console.SetCursorPosition(left, top);
        Console.Write(">");
    }

    public void MoveBattleMenuCursor(CursorMoveDirection direction)
    {
        var directionOffsetMap = new Dictionary<CursorMoveDirection, (int left, int top)>
        {
            { CursorMoveDirection.Down, (0, 2) },
            { CursorMoveDirection.Up, (0, -2) },
            { CursorMoveDirection.Left, (-10, 0) },
            { CursorMoveDirection.Right, (10, 0) }
        };

        MoveBattleMenuCursor(directionOffsetMap[direction]);
    }

    private void MoveBattleMenuCursor((int left, int top) offset)
    {
        // Check boundaries.
        if (IsWithinBoundaries(offset) == false)
            return;

        // Clear cursor behind.
        Console.SetCursorPosition(_cursorPosition.left, _cursorPosition.top);
        Console.Write(" ");

        _cursorPosition =
            (_cursorPosition.left + offset.left, _cursorPosition.top + offset.top);

        // Write new cursor location.
        Console.SetCursorPosition(_cursorPosition.left, _cursorPosition.top);
        Console.Write(">");
    }

    private bool IsWithinBoundaries((int left, int top) offset)
    {
        return _cursorPosition.top + offset.top >= _panelPosition.top + 1
               && _cursorPosition.top + offset.top <= _panelPosition.top + 5
               && _cursorPosition.left + offset.left <= _panelPosition.left + 11
               && _cursorPosition.left + offset.left >= _panelPosition.left + 0;
    }

    public void UpdateCurrentPlayerAction()
    {
        var battleMenuPlayerAction = new Dictionary<string, BattleAction>
        {
            { AttackLabel, BattleAction.Attack },
            { StealLabel, BattleAction.Steal },
            { DefendLabel, BattleAction.Defend },
            { ItemLabel, BattleAction.UseItem },
            { ChangeLabel, BattleAction.Change }
        };

        // Get line where currently cursor is to retrieve selected action name.
        string line = ConsoleExtensions.GetText(0, _cursorPosition.top);

        string actionName = line.Split("|")
            .First(x => x.Contains('>'))
            .Replace(">", string.Empty)
            .Trim();

        CurrentPlayerAction = string.IsNullOrEmpty(actionName)
            ? null
            : battleMenuPlayerAction[actionName];
    }

    public void Hide()
    {
        ConsoleExtensions.ClearRange((_panelPosition.left, _panelPositionRight), _panelPosition.top);
        ConsoleExtensions.ClearRange((_panelPosition.left, _panelPositionRight), _panelPosition.top + 1);
        ConsoleExtensions.ClearRange((_panelPosition.left, _panelPositionRight), _panelPosition.top + 2);
        ConsoleExtensions.ClearRange((_panelPosition.left, _panelPositionRight), _panelPosition.top + 3);
        ConsoleExtensions.ClearRange((_panelPosition.left, _panelPositionRight), _panelPosition.top + 4);
        ConsoleExtensions.ClearRange((_panelPosition.left, _panelPositionRight), _panelPosition.top + 5);
        ConsoleExtensions.ClearRange((_panelPosition.left, _panelPositionRight), _panelPosition.top + 6);

        _cursorPosition = _initialCursorPosition;
        IsVisible = false;
    }
}