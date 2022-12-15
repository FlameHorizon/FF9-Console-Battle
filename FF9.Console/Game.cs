using System.ComponentModel;

namespace FF9.Console;

public class Game
{
    private const int MessageLinePositionTop = 0;
    private const int MessageLinePositionLeft = 0;

    private const int BattleMenuPositionLeft = 0;
    private const int BattleMenuPositionTop = MessageLinePositionTop + 2;

    private int _battleMenuCursorPositionLeft = 1;
    private int _battleMenuCursorPositionTop = BattleMenuPositionTop + 1;
    private PlayerAction? _currentPlayerAction = PlayerAction.Attack;

    private readonly Dictionary<string, PlayerAction> _battleMenuPlayerAction = new()
    {
        { "Attack", PlayerAction.Attack },
        { "Steal", PlayerAction.Steal },
        { "Defend", PlayerAction.Defend },
        { "Item", PlayerAction.UseItem },
        { "Change", PlayerAction.Change }
    };


    public void Start()
    {
        System.Console.CursorVisible = false;

        System.Console.WriteLine("This is message line.");

        // Start drawing menu three lines below first line.
        System.Console.SetCursorPosition(0, 2);
        System.Console.WriteLine("|---------|---------|");
        System.Console.WriteLine("| Attack  | Defend  |");
        System.Console.WriteLine("|---------|---------|");
        System.Console.WriteLine("| Steal   |         |");
        System.Console.WriteLine("|---------|---------|");
        System.Console.WriteLine("| Item    | Change  |");
        System.Console.WriteLine("|---------|---------|");

        SetBattleMenuCursor(_battleMenuCursorPositionLeft, _battleMenuCursorPositionTop);

        while (true)
        {
            ConsoleKeyInfo keyPressed = System.Console.ReadKey(true);

            if (keyPressed.Key == ConsoleKey.B)
            {
                System.Console.WriteLine("Wrote B, exiting program.");
                break;
            }

            if (ArrowKeyPressed(keyPressed))
            {
                HandleArrowKey(keyPressed);
                UpdateCurrentPlayerAction();
            }
            else if (ConsoleKey.Enter == keyPressed.Key)
                ExecuteAction();
            else
                System.Console.WriteLine();
        }
    }

    private void UpdateCurrentPlayerAction()
    {
        // Get line where currently cursor is to retrieve selected action name.
        string line = ReadConsole.GetText(0, _battleMenuCursorPositionTop);

        string actionName = line.Split("|")
            .First(x => x.Contains('>'))
            .Replace(">", string.Empty)
            .Trim();

        _currentPlayerAction = string.IsNullOrEmpty(actionName)
            ? null
            : _battleMenuPlayerAction[actionName];
    }

    private void ExecuteAction()
    {
        string msg = _currentPlayerAction switch
        {
            PlayerAction.Attack => "Player attacked another player",
            PlayerAction.Steal => "Player steals an item",
            PlayerAction.UseItem => "Player uses item",
            PlayerAction.Defend => "Player defends",
            PlayerAction.Change => "Player changes",
            null => string.Empty,
            _ => throw new InvalidEnumArgumentException()
        };

        WriteMessage(msg);
    }

    private static void WriteMessage(string msg)
    {
        ConsoleExtensions.ClearLine(MessageLinePositionTop);
        System.Console.SetCursorPosition(MessageLinePositionLeft, MessageLinePositionTop);
        System.Console.Write(msg);
    }

    private static bool ArrowKeyPressed(ConsoleKeyInfo keyPressed)
    {
        return new[] { ConsoleKey.DownArrow, ConsoleKey.UpArrow, ConsoleKey.RightArrow, ConsoleKey.LeftArrow }
            .Contains(keyPressed.Key);
    }

    private void HandleArrowKey(ConsoleKeyInfo keyPressed)
    {
        if (keyPressed.Key == ConsoleKey.DownArrow)
            MoveBattleMenuCursor(CursorMoveDirection.Down);

        else if (keyPressed.Key == ConsoleKey.UpArrow)
            MoveBattleMenuCursor(CursorMoveDirection.Up);

        else if (keyPressed.Key == ConsoleKey.RightArrow)
            MoveBattleMenuCursor(CursorMoveDirection.Right);

        else if (keyPressed.Key == ConsoleKey.LeftArrow)
            MoveBattleMenuCursor(CursorMoveDirection.Left);
    }

    private void SetBattleMenuCursor(int left, int top)
    {
        System.Console.SetCursorPosition(left, top);
        System.Console.Write(">");
    }

    private void MoveBattleMenuCursor(CursorMoveDirection direction)
    {
        if (direction == CursorMoveDirection.Up)
            MoveBattleMenuCursor((0, -2));

        else if (direction == CursorMoveDirection.Down)
            MoveBattleMenuCursor((0, 2));

        else if (direction == CursorMoveDirection.Left)
            MoveBattleMenuCursor((-10, 0));

        else if (direction == CursorMoveDirection.Right)
            MoveBattleMenuCursor((10, 0));

        else
            throw new InvalidEnumArgumentException();
    }

    private void MoveBattleMenuCursor((int left, int top) offset)
    {
        // Check boundaries.
        if (IsWithinBoundaries(offset) == false)
            return;

        // Clear cursor behind.
        System.Console.SetCursorPosition(_battleMenuCursorPositionLeft, _battleMenuCursorPositionTop);
        System.Console.Write(" ");

        (int left, int top) newCursorPosition =
            (_battleMenuCursorPositionLeft + offset.left, _battleMenuCursorPositionTop + offset.top);

        // Write new cursor location.
        System.Console.SetCursorPosition(newCursorPosition.left, newCursorPosition.top);
        _battleMenuCursorPositionLeft = newCursorPosition.left;
        _battleMenuCursorPositionTop = newCursorPosition.top;
        System.Console.Write(">");
    }

    private bool IsWithinBoundaries((int left, int top) offset)
    {
        return _battleMenuCursorPositionTop + offset.top >= BattleMenuPositionTop + 1
               && _battleMenuCursorPositionTop + offset.top <= BattleMenuPositionTop + 5
               && _battleMenuCursorPositionLeft + offset.left <= BattleMenuPositionLeft + 11
               && _battleMenuCursorPositionLeft + offset.left >= BattleMenuPositionLeft + 0;
    }
}