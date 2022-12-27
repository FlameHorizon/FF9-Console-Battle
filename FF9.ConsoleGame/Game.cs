using FF9.ConsoleGame.Battle;
using FF9.ConsoleGame.Items;
using FF9.ConsoleGame.UI;
using BattleEngine = FF9.ConsoleGame.Battle.BattleEngine;

namespace FF9.ConsoleGame;

public class Game
{
    private const int MillisecondsTimeout = 750;

    private const int MessageLinePositionTop = 0;
    private const int MessageLinePositionLeft = 0;

    private readonly BattleEngine _btlEngine;
    private readonly CommandPanel _commandPanel;
    private readonly PartyStatusPanel _partyStatusPanel;
    private readonly TargetingPanel _targetingPanel;
    private readonly ItemPanel _itemPanel;

    public Game(BattleEngine btlEngine)
    {
        _btlEngine = btlEngine;
        _commandPanel = new CommandPanel(panelPosition: (0, 2));
        _partyStatusPanel = new PartyStatusPanel(_btlEngine, panelPosition: (30, 2));
        _targetingPanel = new TargetingPanel(_btlEngine, panelPosition: (0, 2));
        _itemPanel = new ItemPanel(_btlEngine, panelPosition: (0, 2));
        Console.CursorVisible = false;
    }

    public void Start()
    {
        Console.WriteLine(" ");
        WriteMessage($"{_btlEngine.Source.Name}'s turn.");
        _commandPanel.Draw();

        _partyStatusPanel.Draw();
        _partyStatusPanel.UpdatePlayerTurnIndicator();

        while (true)
        {
            while (_btlEngine.IsTurnAi)
            {
                BattleAction action = _btlEngine.AiAction();
                HandleAction(action);

                if (_btlEngine.PlayerDefeated == false)
                    continue;

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
                else if (_itemPanel.IsVisible && _itemPanel.Item != null && _targetingPanel.Target != null)
                {
                    _btlEngine.SetItem(_itemPanel.Item);
                    HandleAction(_commandPanel.CurrentPlayerAction);
                    _targetingPanel.Hide();
                    _commandPanel.Draw();
                }
                else if (_itemPanel.IsVisible && _itemPanel.Item != null)
                {
                    _itemPanel.Hide();
                    _targetingPanel.Draw();
                }

                else if (_commandPanel.IsVisible)
                {
                    if (_commandPanel.CurrentPlayerAction == BattleAction.Defend)
                    {
                        _commandPanel.Hide();
                        HandleAction(_commandPanel.CurrentPlayerAction);
                        _commandPanel.Draw();
                    }
                    else if (_commandPanel.CurrentPlayerAction == BattleAction.UseItem)
                    {
                        _commandPanel.Hide();
                        _itemPanel.Draw();
                    }
                    else
                    {
                        _commandPanel.Hide();
                        _targetingPanel.Draw();
                    }
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
            return;

        if (_targetingPanel.IsVisible)
        {
            _targetingPanel.Hide();
            _commandPanel.Draw();
        }

        if (_itemPanel.IsVisible)
        {
            _itemPanel.Hide();
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
                ExecuteUseItemAction();
                break;
            case BattleAction.Change:
                break;
            case null:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action));
        }
    }

    private void ExecuteStealAction()
    {
        _btlEngine.TurnSteal();

        Item? stolenItem = _btlEngine.LastStolenItem;

        string msg = stolenItem is not null
            ? $"{_btlEngine.Source.Name} stole {stolenItem.Name} from {_btlEngine.Target?.Name}"
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
            target = _btlEngine.Target ?? throw new InvalidOperationException();
        }
        else
        {
            // This is how AI makes decision who to target.
            IEnumerable<Unit> list = _btlEngine.PlayerUnits.Where(u => u.IsAlive).ToList();
            int rand = Random.Shared.Next(1, list.Count() + 1);
            target = list.Skip(rand - 1).First();
        }

        _btlEngine.TurnAttack(target);
        _partyStatusPanel.UpdatePlayerHealth();

        string msg = _btlEngine.LastDamageValue == 0
            ? $"{_btlEngine.Source.Name} missed attack."
            : $"{_btlEngine.Source.Name} dealt {_btlEngine.LastDamageValue} " +
              $"damage to {target.Name}";

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

    private void ExecuteUseItemAction()
    {
        Unit target;
        if (_btlEngine.Source.IsPlayer)
            target = _btlEngine.Target ?? throw new InvalidOperationException();

        else
            throw new InvalidOperationException("AI can't use items.");

        if (_itemPanel.Item is null)
            return;

        _btlEngine.SetItem(_itemPanel.Item);
        _btlEngine.TurnUseItem(target);

        _partyStatusPanel.UpdatePlayerHealth();

        string msg = $"{_btlEngine.Source.Name} used item " +
                     $"{_btlEngine.LastUsedItem?.Name} on {_btlEngine.Target.Name}";

        WriteMessage(msg);
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
        var keyDirectionMap = new Dictionary<ConsoleKey, CursorMoveDirection>
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

        else if (_itemPanel.IsVisible)
            _itemPanel.MoveCursor(keyDirectionMap[keyPressed.Key]);

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