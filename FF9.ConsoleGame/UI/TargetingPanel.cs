﻿using FF9.ConsoleGame.Battle;

namespace FF9.ConsoleGame.UI;

public class TargetingPanel
{
    private readonly (int left, int top) _panelPosition;
    private readonly int _panelPositionRight;
    private readonly BattleEngine _btlEngine;

    private readonly (int left, int top) _initialCursorPosition;
    private (int left, int top) _cursorPosition;
    
    public TargetingPanel(BattleEngine btlEngine, (int left, int top) panelPosition)
    {
        _btlEngine = btlEngine;
        _panelPosition = panelPosition;
        _panelPositionRight = _panelPosition.left + 23;
        _cursorPosition = (_panelPosition.left + 1, _panelPosition.top + 2);
        _initialCursorPosition = _cursorPosition;
    }
    
    public bool IsVisible { get; private set; }
    public Unit? Target { get; private set; }

    public void Draw()
    {
        Console.SetCursorPosition(_panelPosition.left, _panelPosition.top);
        Console.Write("|---------------------|");
        Console.SetCursorPosition(_panelPosition.left, _panelPosition.top + 1);
        Console.Write("|Enemy        | Player|");

        int offset = 2;
        foreach (Unit unit in _btlEngine.UnitsInBattle.Where(u => u.IsPlayer))
        {
            Console.SetCursorPosition(_panelPosition.left, _panelPosition.top + offset);
            Console.Write("|             | {0}|", unit.Name.PadRight(6));
            offset++;
        }
        
        offset = 2;
        foreach (Unit unit in _btlEngine.UnitsInBattle.Where(u => u.IsPlayer == false))
        {
            Console.SetCursorPosition(_panelPosition.left + 2, _panelPosition.top + offset);
            Console.Write("{0}", unit.Name.PadRight(11));
            offset++;
        }
        
        Console.SetCursorPosition(_panelPosition.left + 2, _panelPosition.top + 2);
        Console.Write("{0}", "Masked Man".PadRight(11));
        
        SetCursorAtInitialPosition();
        UpdateTarget();
        IsVisible = true;
    }

    private void SetCursorAtInitialPosition()
    {
        SetCursorAtInitialPosition(_cursorPosition.left, _cursorPosition.top);
    }
    
    private void SetCursorAtInitialPosition(int left, int top)
    {
        Console.SetCursorPosition(left, top);
        Console.Write(">");
    }

    public void Hide()
    {
        ConsoleExtensions.ClearRange((_panelPosition.left, _panelPositionRight), _panelPosition.top);
        ConsoleExtensions.ClearRange((_panelPosition.left, _panelPositionRight), _panelPosition.top + 1);
        ConsoleExtensions.ClearRange((_panelPosition.left, _panelPositionRight), _panelPosition.top + 2);

        _cursorPosition = _initialCursorPosition;
        IsVisible = false;
        Target = null;
    }

    public void MoveCursor(CursorMoveDirection direction)
    {
        var directionOffsetMap = new Dictionary<CursorMoveDirection, (int left, int top)>()
        {
            { CursorMoveDirection.Down, (0, 1) },
            { CursorMoveDirection.Up, (0, -1) },
            { CursorMoveDirection.Left, (-14, 0) },
            { CursorMoveDirection.Right, (14, 0) },
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

        _cursorPosition =
            (_cursorPosition.left + offset.left, _cursorPosition.top + offset.top);

        // Write new cursor location.
        Console.SetCursorPosition(_cursorPosition.left, _cursorPosition.top);
        Console.Write(">");
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
            : _btlEngine.UnitsInBattle.First(u => u.Name == targetName);
    }
}