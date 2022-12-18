using System.ComponentModel;
using FF9.ConsoleGame.Battle;

namespace FF9.ConsoleGame.UI;

public class Game
{
    private const int MessageLinePositionTop = 0;
    private const int MessageLinePositionLeft = 0;

    private const int SpaceBetweenMessageLineAndBattleMenu = 2;

    private const int BattleMenuPositionLeft = 0;
    private const int BattleMenuPositionTop = MessageLinePositionTop + SpaceBetweenMessageLineAndBattleMenu;

    private int _battleMenuCursorPositionLeft = 1;
    private int _battleMenuCursorPositionTop = BattleMenuPositionTop + 1;
    private BattleAction? _currentPlayerAction = BattleAction.Attack;

    private const int ZidaneHpStartLeft = 42;
    private const int ZidaneHpEndLeft = 46;
    private const int ZidaneTop = 3 + SpaceBetweenMessageLineAndBattleMenu;

    private const string AttackLabel = "Attack";
    private const string StealLabel = "Steal";
    private const string ItemLabel = "Item";
    private const string DefendLabel = "Defend";
    private const string EmptyLabel = "";
    private const string ChangeLabel = "Change";

    private readonly Dictionary<string, BattleAction> _battleMenuPlayerAction = new()
    {
        { AttackLabel, BattleAction.Attack },
        { StealLabel, BattleAction.Steal },
        { DefendLabel, BattleAction.Defend },
        { ItemLabel, BattleAction.UseItem },
        { ChangeLabel, BattleAction.Change }
    };

    private readonly BattleEngine _btlEngine;

    public Game(BattleEngine btlEngine)
    {
        _btlEngine = btlEngine;
        Console.CursorVisible = false;
    }

    public void Start()
    {
        Console.WriteLine(" ");
        WriteMessage($"{_btlEngine.Source.Name}'s turn.");

        Unit playerUnit = _btlEngine.UnitsInBattle.First(u => u.IsPlayer);
        string playerNameWithPadding = playerUnit.Name.PadRight(7);
        string playerHpWithPadding = playerUnit.Hp.ToString().PadLeft(3);

        // Start drawing menu three lines below first line.
        Console.SetCursorPosition(0, 2);
        Console.WriteLine("|---------|---------|          ");
        Console.WriteLine("| {0}| {1}|          |------------------|", AttackLabel.PadRight(8), DefendLabel.PadRight(8));
        Console.WriteLine("|---------|---------|          |Name     |  HP| MP|");
        Console.WriteLine("| {0}| {1}|          | {2} | {3}|  0|", StealLabel.PadRight(8), EmptyLabel.PadRight(8) ,playerNameWithPadding,
            playerHpWithPadding);
        Console.WriteLine("|---------|---------|          |                  |");
        Console.WriteLine("| {0}| {1}|          |                  |", ItemLabel.PadRight(8), ChangeLabel.PadRight(8));
        Console.WriteLine("|---------|---------|          |                  |");

        SetBattleMenuCursor(_battleMenuCursorPositionLeft, _battleMenuCursorPositionTop);

        while (true)
        {
            if (_btlEngine.IsTurnAi)
            {
                BattleAction action = _btlEngine.AiAction();
                HandleAction(action);
                continue;
            }
            
            ConsoleKeyInfo keyPressed = Console.ReadKey(true);

            if (ArrowKeyPressed(keyPressed))
            {
                HandleArrowKey(keyPressed);
                UpdateCurrentPlayerAction();
            }
            else if (ConsoleKey.Enter == keyPressed.Key)
                HandleAction(_currentPlayerAction);

            if (_btlEngine.EnemyDefeated)
                break;
        }
    }

    private void HandleAction(BattleAction? action)
    {
        switch (action)
        {
            case BattleAction.Attack:
                ExecuteAttackAction();
                break;
            case BattleAction.Defend:
                ExecuteDefendAction();
                break;
            case BattleAction.Steal:
                ExecuteStealAction();
                break;
            case BattleAction.UseItem:
                break;
            case BattleAction.Change:
                break;
            case null:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void ExecuteStealAction()
    {
        _btlEngine.TurnSteal();

        Item? stolenItem = _btlEngine.LastStolenItem;

        string msg = stolenItem is not null 
            ? $"{_btlEngine.Source} stole {stolenItem.Name} from {_btlEngine.Target}" 
            : "Couldn't steal an item";
        
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
            Console.SetCursorPosition(left, ZidaneTop);
            Console.Write(' ');
        }

        char[] split = target.Hp.ToString().ToCharArray();

        var i = 0;
        // Clear current value of Zidane's health.
        for (int left = ZidaneHpStartLeft + (4 - split.Length); left < ZidaneHpEndLeft; left++)
        {
            Console.SetCursorPosition(left, ZidaneTop);
            Console.Write(split[i]);

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
        string line = ConsoleExtensions.GetText(0, _battleMenuCursorPositionTop);

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
        Console.SetCursorPosition(left, top);
        Console.Write(">");
    }

    private static void WriteMessage(string msg)
    {
        ConsoleExtensions.ClearLine(MessageLinePositionTop);
        Console.SetCursorPosition(MessageLinePositionLeft, MessageLinePositionTop);
        Console.Write(msg);
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
        Console.SetCursorPosition(_battleMenuCursorPositionLeft, _battleMenuCursorPositionTop);
        Console.Write(" ");

        (int left, int top) newCursorPosition =
            (_battleMenuCursorPositionLeft + offset.left, _battleMenuCursorPositionTop + offset.top);

        // Write new cursor location.
        Console.SetCursorPosition(newCursorPosition.left, newCursorPosition.top);
        _battleMenuCursorPositionLeft = newCursorPosition.left;
        _battleMenuCursorPositionTop = newCursorPosition.top;
        Console.Write(">");
    }

    private bool IsWithinBoundaries((int left, int top) offset)
    {
        return _battleMenuCursorPositionTop + offset.top >= BattleMenuPositionTop + 1
               && _battleMenuCursorPositionTop + offset.top <= BattleMenuPositionTop + 5
               && _battleMenuCursorPositionLeft + offset.left <= BattleMenuPositionLeft + 11
               && _battleMenuCursorPositionLeft + offset.left >= BattleMenuPositionLeft + 0;
    }
}