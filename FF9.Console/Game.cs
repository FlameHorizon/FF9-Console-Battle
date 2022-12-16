using System.ComponentModel;
using FF9.Console.Battle;

namespace FF9.Console;

public class Game
{
    private const int MessageLinePositionTop = 0;
    private const int MessageLinePositionLeft = 0;

    private const int SpaceBetweenMessageLineAndBattleMenu = 2;

    private const int BattleMenuPositionLeft = 0;
    private const int BattleMenuPositionTop = MessageLinePositionTop + SpaceBetweenMessageLineAndBattleMenu;

    private int _battleMenuCursorPositionLeft = 1;
    private int _battleMenuCursorPositionTop = BattleMenuPositionTop + 1;
    private PlayerAction? _currentPlayerAction = PlayerAction.Attack;

    private const int ZidaneHpStartLeft = 42;
    private const int ZidaneHpEndLeft = 46;
    private const int ZidaneTop = 3 + SpaceBetweenMessageLineAndBattleMenu;

    private readonly Dictionary<string, PlayerAction> _battleMenuPlayerAction = new()
    {
        { "Attack", PlayerAction.Attack },
        { "Steal", PlayerAction.Steal },
        { "Defend", PlayerAction.Defend },
        { "Item", PlayerAction.UseItem },
        { "Change", PlayerAction.Change }
    };

    private readonly BattleEngine _btlEngine;

    public Game(BattleEngine btlEngine)
    {
        _btlEngine = btlEngine;
        System.Console.CursorVisible = false;
    }

    public void Start()
    {
        System.Console.WriteLine(" ");
        WriteMessage($"{_btlEngine.CurrentUnitTurn.Name}'s turn.");

        // Start drawing menu three lines below first line.
        System.Console.SetCursorPosition(0, 2);
        System.Console.WriteLine( "|---------|---------|          ");
        System.Console.WriteLine( "| Attack  | Defend  |          |------------------|");
        System.Console.WriteLine( "|---------|---------|          |Name     |  HP| MP|");
        System.Console.WriteLine("| Steal   |         |          | {0}  | {1}| 36|", _btlEngine.UnitsInBattle.First().Name, _btlEngine.UnitsInBattle.First().Health);
        System.Console.WriteLine( "|---------|---------|          |                  |");
        System.Console.WriteLine( "| Item    | Change  |          |                  |");
        System.Console.WriteLine( "|---------|---------|          |                  |");

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
            {
                switch (_currentPlayerAction)
                {
                    case PlayerAction.Attack:
                        ExecuteAttackAction();
                        break;
                    case PlayerAction.Defend:
                        ExecuteDefendAction();
                        break;
                    case PlayerAction.Steal:
                        break;
                    case PlayerAction.UseItem:
                        break;
                    case PlayerAction.Change:
                        break;
                    case null:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }

    private void ExecuteDefendAction()
    {
        _btlEngine.TurnDefence();
        
        var msg = $"{_btlEngine.CurrentUnitTurn.Name} increase it's defence by 10%.";
        WriteMessage(msg);
        
        Thread.Sleep(2000);
        _btlEngine.NextTurn();
        WriteMessage($"{_btlEngine.CurrentUnitTurn.Name}'s turn.");
    }

    private void ExecuteAttackAction()
    {
        Unit target = _btlEngine.CurrentUnitTurn.Name == "Zidane"
            ? _btlEngine.UnitsInBattle.Single(u => u.Name == "Masked Man")
            : _btlEngine.UnitsInBattle.Single(u => u.Name == "Zidane");
        
        _btlEngine.TurnAttack(target);

        if (target.Name == "Zidane")
        {
            UpdateZidaneHealthOnConsole(target);
        }

        string msg = $"{_btlEngine.CurrentUnitTurn.Name} " +
                     $"dealt {_btlEngine.LastDamageValue} " +
                     $"to {target.Name}";
        WriteMessage(msg);
        
        
        if (target.IsAlive == false)
        {
            Thread.Sleep(2000);
            
            var playerIsDeadMsg = $"{target.Name} died.";
            WriteMessage(playerIsDeadMsg);
            
            Thread.Sleep(2000);

            var winnerMsg = $"{_btlEngine.CurrentUnitTurn.Name} won.";
            WriteMessage(winnerMsg);
            return;
        }
        
        Thread.Sleep(2000);
        _btlEngine.NextTurn();
        WriteMessage($"{_btlEngine.CurrentUnitTurn.Name}'s turn.");
    }

    private void UpdateZidaneHealthOnConsole(Unit target)
    {
        for (int left = ZidaneHpStartLeft; left < ZidaneHpEndLeft; left++)
        {
            System.Console.SetCursorPosition(left, ZidaneTop);
            System.Console.Write(' ');
        }
        
        char[] split = target.Health.ToString().ToCharArray();
        
        int i = 0;
        // Clear current value of Zidane's health.
        for (int left = ZidaneHpStartLeft + (4 - split.Length); left < ZidaneHpEndLeft; left++)
        {
            System.Console.SetCursorPosition(left, ZidaneTop);
            System.Console.Write(split[i]);

            i++;
        }
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

    private void SetBattleMenuCursor(int left, int top)
    {
        System.Console.SetCursorPosition(left, top);
        System.Console.Write(">");
    }

    private static void WriteMessage(string msg)
    {
        ConsoleExtensions.ClearLine(MessageLinePositionTop);
        System.Console.SetCursorPosition(MessageLinePositionLeft, MessageLinePositionTop);
        System.Console.Write(msg);
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