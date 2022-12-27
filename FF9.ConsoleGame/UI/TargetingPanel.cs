using FF9.ConsoleGame.Battle;
using System.Diagnostics;

namespace FF9.ConsoleGame.UI;

public class TargetingPanel
{
    private readonly (int left, int top) _panelPosition;
    private readonly int _panelPositionRight;
    private readonly BattleEngine _btlEngine;
    private readonly ConsoleColor _defaultConsoleColor;

    private readonly (int left, int top) _firstEnemyUnitPosition;
    private readonly (int left, int top) _firstPlayerUnitPosition;
    private (int left, int top) _cursorPosition;

    public TargetingPanel(BattleEngine btlEngine, (int left, int top) panelPosition)
    {
        _btlEngine = btlEngine;
        _panelPosition = panelPosition;
        _panelPositionRight = _panelPosition.left + 23;
        _firstEnemyUnitPosition = (_panelPosition.left + 1, _panelPosition.top + 2);
        _firstPlayerUnitPosition = (_panelPosition.left + 15, _panelPosition.top + 2);
        _defaultConsoleColor = Console.ForegroundColor;
    }

    public bool IsVisible { get; private set; }
    public Unit? Target { get; private set; }

    public void Draw(bool focusOnPlayer)
    {
        Console.SetCursorPosition(_panelPosition.left, _panelPosition.top);
        Console.Write("|---------------------|");
        Console.SetCursorPosition(_panelPosition.left, _panelPosition.top + 1);
        Console.Write("|Enemy        | Player|");

        DrawPlayers();
        DrawEnemies();

        if (focusOnPlayer)
            SetCursorPosition(_firstPlayerUnitPosition.left, _firstPlayerUnitPosition.top);

        else
            SetCursorPosition(_firstEnemyUnitPosition.left, _firstEnemyUnitPosition.top);

        UpdateTarget();
        IsVisible = true;
    }

    private void DrawEnemies()
    {
        var offset = 2;
        IEnumerable<Unit> aliveEnemyUnits =
            _btlEngine.EnemyUnits.Where(u => u.IsAlive);

        foreach (Unit unit in aliveEnemyUnits)
        {
            Console.SetCursorPosition(_panelPosition.left + 2,
                _panelPosition.top + offset);

            Console.Write("{0}", unit.Name.PadRight(11));
            offset++;
        }
    }


    private void DrawPlayers()
    {
        var offset = 2;
        foreach (Unit unit in _btlEngine.PlayerUnits)
        {
            Console.SetCursorPosition(_panelPosition.left, _panelPosition.top + offset);
            Console.Write("|             |       |");

            Console.ForegroundColor = unit.IsAlive == false
                ? ConsoleColor.DarkGray
                : _defaultConsoleColor;

            Console.SetCursorPosition(_panelPosition.left + 16,
                _panelPosition.top + offset);

            Console.Write(unit.Name.PadRight(6));
            Console.ForegroundColor = _defaultConsoleColor;

            offset++;
        }
    }

    private void SetCursorPosition(int left, int top)
    {
        _cursorPosition = (left, top);
        Console.SetCursorPosition(left, top);
        Console.Write(">");
    }

    public void Hide()
    {
        ConsoleExtensions.ClearRange((_panelPosition.left, _panelPositionRight), _panelPosition.top);
        ConsoleExtensions.ClearRange((_panelPosition.left, _panelPositionRight), _panelPosition.top + 1);
        int characterRows = Math.Max(
            _btlEngine.PlayerUnits.Count(),
            _btlEngine.EnemyUnits.Count()
        );

        for (var i = 0; i < characterRows; i++)
        {
            ConsoleExtensions.ClearRange(
                (_panelPosition.left, _panelPositionRight),
                _panelPosition.top + 2 + i);
        }

        _cursorPosition = _firstEnemyUnitPosition;
        IsVisible = false;
        Target = null;
    }

    public void MoveCursor(CursorMoveDirection direction)
    {
        var directionOffsetMap = new Dictionary<CursorMoveDirection, (int left, int top)>
        {
            { CursorMoveDirection.Down, (0, 1) },
            { CursorMoveDirection.Up, (0, -1) },
            { CursorMoveDirection.Left, (-14, 0) },
            { CursorMoveDirection.Right, (14, 0) }
        };

        MoveCursor(directionOffsetMap[direction]);
        UpdateTarget();
    }

    private void MoveCursor((int left, int top) offset)
    {
        // Check boundaries.
        if (IsWithinBoundaries(offset) == false)
            return;

        // Clear cursor behind.
        Console.SetCursorPosition(_cursorPosition.left, _cursorPosition.top);
        Console.Write(" ");

        SetCursorPosition(
            _cursorPosition.left + offset.left,
            _cursorPosition.top + offset.top);
    }

    private bool IsWithinBoundaries((int left, int top) offset)
    {
        return _cursorPosition.top + offset.top >= _panelPosition.top + 2
               && _cursorPosition.top + offset.top <= _panelPosition.top + 5
               && _cursorPosition.left + offset.left <= _panelPosition.left + 15
               && _cursorPosition.left + offset.left >= _panelPosition.left + 0;
    }

    private void UpdateTarget()
    {
        string line = ConsoleExtensions.GetText(0, _cursorPosition.top);

        string targetName = line.Split("|")
            .First(x => x.Contains('>'))
            .Replace(">", string.Empty)
            .Trim();

        Target = string.IsNullOrEmpty(targetName)
            ? null
            : _btlEngine.UnitsInBattle.FirstOrDefault(
                u => u.Name == targetName);
    }
}