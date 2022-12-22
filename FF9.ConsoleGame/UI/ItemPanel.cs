using System.Runtime.CompilerServices;
using FF9.ConsoleGame.Battle;

namespace FF9.ConsoleGame.UI;

public class ItemPanel
{
    private readonly BattleEngine _battleEngine;
    private readonly (int left, int top) _panelPosition;
    private readonly int _panelPositionRight;
    private readonly (int left, int top) _initialCursorPosition;
    
    private (int left, int top) _cursorPosition;

    public ItemPanel(BattleEngine battleEngine, (int left, int top) panelPosition)
    {
        _battleEngine = battleEngine;
        _panelPosition = panelPosition;
        _panelPositionRight = _panelPosition.left + 21;
        _cursorPosition = (_panelPosition.left + 1, _panelPosition.top + 2);
        _initialCursorPosition = _cursorPosition;
    }

    public bool IsVisible { get; private set; }
    public Item? Item { get; private set; }
    
    public void Draw()
    {
        Console.SetCursorPosition(_panelPosition.left, _panelPosition.top);
        Console.Write("|-------------------|");

        Console.SetCursorPosition(_panelPosition.left, _panelPosition.top + 1);
        Console.Write("|Item         |  Num|");

        DrawItems();

        SetCursorPositionAtInitial();
        UpdateItem();
        IsVisible = true;
    }

    private void DrawItems()
    {
        var offset = 2;
        foreach (Item item in _battleEngine.PlayerInventory.Take(4))
        {
            if (item.Count <= 0)
                continue;
            
            Console.SetCursorPosition(_panelPosition.left, _panelPosition.top + offset);
            Console.Write("| {0}  {1}|         ", item.Name.PadRight(12), item.Count.ToString().PadLeft(4));    
            
            offset++;
        }
    }

    private void SetCursorPositionAtInitial()
    {
        SetCursorPosition(_initialCursorPosition.left, _initialCursorPosition.top);
    }

    private void SetCursorPosition(int left, int top)
    {
        Console.SetCursorPosition(left, top);
        Console.Write(">");
    }

    public void MoveCursor(CursorMoveDirection direction)
    {
        if (direction is CursorMoveDirection.Left or CursorMoveDirection.Right)
            return;
        
        var directionOffsetMap = new Dictionary<CursorMoveDirection, (int left, int top)>
        {
            { CursorMoveDirection.Down, (0, 1) },
            { CursorMoveDirection.Up, (0, -1) },
            { CursorMoveDirection.Left, (-14, 0) },
            { CursorMoveDirection.Right, (14, 0) }
        };

        MoveCursor(directionOffsetMap[direction]);
        UpdateItem();
    }

    private void MoveCursor((int left, int top) offset)
    {
        // Check boundaries.
        if (IsWithinBoundaries(offset) == false)
            return;

        // Clear cursor behind.
        Console.SetCursorPosition(_cursorPosition.left, _cursorPosition.top);
        Console.Write(" ");

        _cursorPosition =  (_cursorPosition.left + offset.left, 
            _cursorPosition.top + offset.top);

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

    private void UpdateItem()
    {
        string line = ConsoleExtensions.GetText(0, _cursorPosition.top);
        
        string itemName = line.Substring(1,12)
            .Replace(">", string.Empty)
            .Trim();

        Item = string.IsNullOrEmpty(itemName)
            ? null
            : _battleEngine.PlayerInventory
                .FirstOrDefault(i => i.Name == itemName);
    }
    
    public void Hide()
    {
        ConsoleExtensions.ClearRange((_panelPosition.left, _panelPositionRight), _panelPosition.top);
        ConsoleExtensions.ClearRange((_panelPosition.left, _panelPositionRight), _panelPosition.top + 1);
        int characterRows = Math.Max(
            _battleEngine.PlayerUnits.Count(),
            _battleEngine.EnemyUnits.Count()
        );

        for (var i = 0; i < characterRows; i++)
        {
            ConsoleExtensions.ClearRange(
                (_panelPosition.left, _panelPositionRight), 
                _panelPosition.top + 2 + i);
        }

        _cursorPosition = _initialCursorPosition;
        IsVisible = false;
    }
}