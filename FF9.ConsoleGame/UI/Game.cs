﻿using System.ComponentModel;
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

    private readonly IEnumerable<Unit> _playerParty;
    private readonly BattleEngine _btlEngine;
    private readonly CommandPanel _commandPanel;
    private readonly PartyStatusPanel _partyStatusPanel;

    public Game(BattleEngine btlEngine)
    {
        _btlEngine = btlEngine;
        _commandPanel = new CommandPanel(panelLeftTopPosition: (0, 2));
        _partyStatusPanel = new PartyStatusPanel(_btlEngine, panelLefTopPosition: (31, 3));
        _playerParty = _btlEngine.UnitsInBattle.Where(u => u.IsPlayer);
        Console.CursorVisible = false;
    }

    public void Start()
    {
        Console.WriteLine(" ");
        WriteMessage($"{_btlEngine.Source.Name}'s turn.");
        _commandPanel.DrawBattleMenu();
        _commandPanel.SetBattleMenuCursor(_battleMenuCursorPositionLeft, _battleMenuCursorPositionTop);

        _partyStatusPanel.DrawCharactersInfo();
        _partyStatusPanel.UpdatePlayerTurnIndicator();

        while (true)
        {
            if (_btlEngine.IsTurnAi)
            {
                BattleAction action = _btlEngine.AiAction();
                HandleAction(action);
            }

            if (_btlEngine.PlayerDefeated)
            {
                WriteMessage("Player party has been defeated.");
                break;
            }

            ConsoleKeyInfo keyPressed = Console.ReadKey(true);

            if (ArrowKeyPressed(keyPressed))
            {
                HandleArrowKey(keyPressed);
                UpdateCurrentPlayerAction();
            }
            else if (ConsoleKey.Enter == keyPressed.Key)
            {
                HandleAction(_currentPlayerAction);
            }

            if (_btlEngine.EnemyDefeated)
            {
                WriteMessage("Enemy party has been defeated.");
                break;
            }
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
            ? $"{_btlEngine.Source.Name} stole {stolenItem.Name} from {_btlEngine.Target.Name}"
            : "Couldn't steal an item";

        WriteMessage(msg);

        Thread.Sleep(1000);
        _btlEngine.NextTurn();
        WriteMessage($"{_btlEngine.Source.Name}'s turn.");
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
        Unit target;
        if (_btlEngine.Source.IsPlayer)
        {
            target = _btlEngine.UnitsInBattle.First(u => u.IsPlayer == false);
        }
        else
        {
            IEnumerable<Unit> list = _btlEngine.UnitsInBattle.Where(u => u.IsPlayer);
            int rand = Random.Shared.Next(1, list.Count() + 1);
            target = list.Skip(rand - 1).First();
        }

        _btlEngine.TurnAttack(target);

        if (target.IsPlayer)
            _partyStatusPanel.UpdatePlayerHealthOnConsole();

        string msg = _btlEngine.LastDamageValue == 0
            ? $"{_btlEngine.Source.Name} missed attack."
            : $"{_btlEngine.Source.Name} dealt {_btlEngine.LastDamageValue} damage to {target.Name}";

        WriteMessage(msg);

        if (target.IsAlive == false)
        {
            Thread.Sleep(1000);
            WriteMessage($"{target.Name} died.");
            Thread.Sleep(1000);
            return;
        }

        Thread.Sleep(1000);
        _btlEngine.NextTurn();
        _partyStatusPanel.UpdatePlayerTurnIndicator();
        WriteMessage($"{_btlEngine.Source.Name}'s turn.");
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