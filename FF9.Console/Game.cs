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

    private const string AttackLabel = "Attack";
    private const string StealLabel = "Steal";
    private const string ItemLabel = "Item";
    private const string DefendLabel = "Defend";
    private const string EmptyLabel = "";
    private const string ChangeLabel = "Change";

    private readonly Dictionary<string, PlayerAction> _battleMenuPlayerAction = new()
    {
        { AttackLabel, PlayerAction.Attack },
        { StealLabel, PlayerAction.Steal },
        { DefendLabel, PlayerAction.Defend },
        { ItemLabel, PlayerAction.UseItem },
        { ChangeLabel, PlayerAction.Change }
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
        WriteMessage($"{_btlEngine.Source.Name}'s turn.");

        Unit playerUnit = _btlEngine.UnitsInBattle.First(u => u.IsPlayer);
        string playerNameWithPadding = playerUnit.Name.PadRight(7);
        string playerHpWithPadding = playerUnit.Hp.ToString().PadLeft(3);

        // Start drawing menu three lines below first line.
        System.Console.SetCursorPosition(0, 2);
        System.Console.WriteLine("|---------|---------|          ");
        System.Console.WriteLine("| {0}| {1}|          |------------------|", AttackLabel.PadRight(8), DefendLabel.PadRight(8));
        System.Console.WriteLine("|---------|---------|          |Name     |  HP| MP|");
        System.Console.WriteLine("| {0}| {1}|          | {2} | {3}|  0|", StealLabel.PadRight(8), EmptyLabel.PadRight(8) ,playerNameWithPadding,
            playerHpWithPadding);
        System.Console.WriteLine("|---------|---------|          |                  |");
        System.Console.WriteLine("| {0}| {1}|          |                  |", ItemLabel.PadRight(8), ChangeLabel.PadRight(8));
        System.Console.WriteLine("|---------|---------|          |                  |");

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
                        ExecuteStealAction();
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

            if (_btlEngine.EnemyDefeated)
            {
                break;
            }
        }
    }

    private void ExecuteStealAction()
    {
        _btlEngine.TurnSteal();

        Item? stolenItem = _btlEngine.LastStolenItem;

        string msg;
        if (stolenItem is not null)
        {
            msg = $"{_btlEngine.Source} stole {stolenItem.Name} from {_btlEngine.Target}";
        }
        else
        {
            msg = "Couldn't steal an item";
        }
        
        WriteMessage(msg);
    }

    private void ExecuteDefendAction()
    {
        _btlEngine.TurnDefence();

        var msg = $"{_btlEngine.Source.Name} is in defence stance. Incoming damage reduced by 50%.";
        WriteMessage(msg);

        Thread.Sleep(1000);
        _btlEngine.NextTurn();
        WriteMessage($"{_btlEngine.Source.Name}'s turn.");
    }

    private void ExecuteAttackAction()
    {
        Unit target = _btlEngine.Target;
        _btlEngine.TurnAttack(target);

        if (target.IsPlayer)
        {
            UpdatePlayerHealthOnConsole(target);
        }

        string msg;
        if (_btlEngine.LastDamageValue == 0)
        {
            // Miss
            msg = $"{_btlEngine.Source.Name} " +
                  $"missed attack.";
        }
        else
        {
            msg = $"{_btlEngine.Source.Name} " +
                  $"dealt {_btlEngine.LastDamageValue} damage " +
                  $"to {target.Name}";
        }

        WriteMessage(msg);

        if (target.IsAlive == false)
        {
            Thread.Sleep(1000);

            var playerIsDeadMsg = $"{target.Name} died.";
            WriteMessage(playerIsDeadMsg);

            Thread.Sleep(1000);

            var winnerMsg = $"{_btlEngine.Source.Name} won.";
            WriteMessage(winnerMsg);
            return;
        }

        Thread.Sleep(1000);
        _btlEngine.NextTurn();
        WriteMessage($"{_btlEngine.Source.Name}'s turn.");
    }

    private void UpdatePlayerHealthOnConsole(Unit target)
    {
        for (int left = ZidaneHpStartLeft; left < ZidaneHpEndLeft; left++)
        {
            System.Console.SetCursorPosition(left, ZidaneTop);
            System.Console.Write(' ');
        }

        char[] split = target.Hp.ToString().ToCharArray();

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