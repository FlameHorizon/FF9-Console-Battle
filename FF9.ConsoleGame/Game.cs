﻿using FF9.ConsoleGame.Battle;
using FF9.ConsoleGame.UI;

namespace FF9.ConsoleGame;

public class Game
{
    private const int MillisecondsTimeout = 500;

    private const int MessageLinePositionTop = 0;
    private const int MessageLinePositionLeft = 0;

    private readonly BattleEngine _btlEngine;
    private readonly CommandPanel _commandPanel;
    private readonly PartyStatusPanel _partyStatusPanel;
    private readonly TargetingPanel _targetingPanel;

    public Game(BattleEngine btlEngine)
    {
        _btlEngine = btlEngine;
        _commandPanel = new CommandPanel(panelPosition: (0, 2));
        _partyStatusPanel = new PartyStatusPanel(_btlEngine, panelPosition: (30, 2));
        _targetingPanel = new TargetingPanel(_btlEngine, panelPosition: (0, 2));
        Console.CursorVisible = false;
    }

    // Now I will be working on targeting system.

    public void Start()
    {
        Console.WriteLine(" ");
        WriteMessage($"{_btlEngine.Source.Name}'s turn.");
        _commandPanel.Draw();

        _partyStatusPanel.Draw();
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
            }
            else if (ConsoleKey.Enter == keyPressed.Key)
            {
                if (_targetingPanel.IsVisible && _targetingPanel.Target != null)
                {
                    _btlEngine.SetTarget(_targetingPanel.Target);
                    HandleAction(_commandPanel.CurrentPlayerAction);
                    _targetingPanel.Hide();
                    _commandPanel.Draw();
                }
                else if (_commandPanel.IsVisible)
                {
                    _commandPanel.Hide();
                    _targetingPanel.Draw();
                }
            }

            else if (ConsoleKey.B == keyPressed.Key)
                HandleBack();

            if (_btlEngine.EnemyDefeated)
            {
                WriteMessage("Enemy party has been defeated.");
                break;
            }
        }
    }

    private void HandleBack()
    {
        if (_commandPanel.IsVisible)
        {
            return;
        }

        if (_targetingPanel.IsVisible)
        {
            _targetingPanel.Hide();
            _commandPanel.Draw();
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

        Thread.Sleep(MillisecondsTimeout);
        _btlEngine.NextTurn();
        _partyStatusPanel.UpdatePlayerTurnIndicator();
        WriteMessage($"{_btlEngine.Source.Name}'s turn.");
    }

    private void ExecuteDefendAction()
    {
        _btlEngine.TurnDefence();

        var msg = $"{_btlEngine.Source.Name} is in defence stance. Incoming damage reduced by 50%.";
        WriteMessage(msg);

        Thread.Sleep(MillisecondsTimeout);
        _btlEngine.NextTurn();
        _partyStatusPanel.UpdatePlayerTurnIndicator();
        WriteMessage($"{_btlEngine.Source.Name}'s turn.");
    }

    private void ExecuteAttackAction()
    {
        Unit target;
        if (_btlEngine.Source.IsPlayer)
        {
            target = _btlEngine.Target;
        }
        else
        {
            // This is how AI makes decision who to target.
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
            Thread.Sleep(MillisecondsTimeout);
            WriteMessage($"{target.Name} died.");
        }

        Thread.Sleep(MillisecondsTimeout);
        _btlEngine.NextTurn();
        _partyStatusPanel.UpdatePlayerTurnIndicator();
        WriteMessage($"{_btlEngine.Source.Name}'s turn.");
    }

    private static bool ArrowKeyPressed(ConsoleKeyInfo keyPressed)
    {
        return new[]
        {
            ConsoleKey.DownArrow,
            ConsoleKey.UpArrow,
            ConsoleKey.RightArrow,
            ConsoleKey.LeftArrow
        }.Contains(keyPressed.Key);
    }

    private void HandleArrowKey(ConsoleKeyInfo keyPressed)
    {
        var keyDirectionMap = new Dictionary<ConsoleKey, CursorMoveDirection>()
        {
            { ConsoleKey.DownArrow, CursorMoveDirection.Down },
            { ConsoleKey.UpArrow, CursorMoveDirection.Up },
            { ConsoleKey.LeftArrow, CursorMoveDirection.Left },
            { ConsoleKey.RightArrow, CursorMoveDirection.Right }
        };

        if (_commandPanel.IsVisible)
        {
            _commandPanel.MoveBattleMenuCursor(keyDirectionMap[keyPressed.Key]);
            _commandPanel.UpdateCurrentPlayerAction();
        }
        else if (_targetingPanel.IsVisible)
            _targetingPanel.MoveCursor(keyDirectionMap[keyPressed.Key]);

        else
            throw new InvalidOperationException("No panel can handle this button press.");
    }

    private static void WriteMessage(string msg)
    {
        ConsoleExtensions.ClearLine(MessageLinePositionTop);
        Console.SetCursorPosition(MessageLinePositionLeft, MessageLinePositionTop);
        Console.Write(msg);
    }
}