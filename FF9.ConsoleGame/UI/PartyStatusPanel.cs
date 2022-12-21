using System.Diagnostics;
using FF9.ConsoleGame.Battle;

namespace FF9.ConsoleGame.UI;

public class PartyStatusPanel
{
    private readonly BattleEngine _btlEngine;
    private readonly List<Unit> _playerParty;

    private readonly (int left, int top) _panelPosition;
    private readonly (int right, int bottom) _panelRightBottomPosition;
    private (int left, int top) _turnIndicatorPosition;

    private readonly int _turnIndicatorLeft;
    
    public PartyStatusPanel(BattleEngine btlEngine, (int left, int top) panelPosition)
    {
        _btlEngine = btlEngine;
        _playerParty = _btlEngine.UnitsInBattle.Where(u => u.IsPlayer).ToList();
        _panelPosition = panelPosition;
        _panelRightBottomPosition = (0, _panelPosition.top + 6);
        _turnIndicatorLeft = _panelPosition.left + 1;
    }

    public void Draw()
    {
        Console.SetCursorPosition(_panelPosition.left, _panelPosition.top);
        Console.Write("|--------------------|");

        Console.SetCursorPosition(_panelPosition.left, _panelPosition.top + 1);
        Console.Write("|Name     |   HP|  MP|");

        int offset = 2;
        foreach (Unit unit in _playerParty)
        {
            Console.SetCursorPosition(_panelPosition.left, _panelPosition.top + offset);
            Console.Write("| {0} | {1}| {2}|",
                unit.Name.PadRight(7),
                unit.Hp.ToString().PadLeft(4),
                unit.Mp.ToString().PadLeft(3));

            offset++;
        }
    }

    public void UpdatePlayerTurnIndicator()
    {
        if (_btlEngine.Source.IsPlayer == false)
        {
            return;
        }

        // Erase previous turn indicator
        if (_turnIndicatorPosition != (0, 0))
        {
            Console.SetCursorPosition(_turnIndicatorPosition.left, _turnIndicatorPosition.top);
            Console.Write(" ");
        }
        
        Console.SetCursorPosition(0, _panelRightBottomPosition.bottom + 1);
        
        string lookFor = " " + _btlEngine.Source.Name;

        List<KernelHelper.COORD> coords = null!;
        const int offset = 2;
        for (var i = 0; i < 4; i++)
        {
            coords = ConsoleExtensions.IndexOfInConsole(lookFor,
                (_panelPosition.left, _panelPosition.top + offset + i));

            if (coords.Any())
                break;
        }
        
        if (coords is null)
        {
            throw new ArgumentNullException();
        }
        

        _turnIndicatorPosition = (_turnIndicatorLeft, coords.First().Y);

        if (coords.First().Y == 0)
        {
            // First observed while using defend action.
            // Might be because before AI was taking its turn...
            Debug.WriteLine($"coords.First().Y is 0. " +
                        $"Might be bug in {nameof(UpdatePlayerTurnIndicator)}");
        }
        
        DrawTurnIndicator(_turnIndicatorLeft, coords.First().Y);
    }

    private static void DrawTurnIndicator(int left, int top)
    {
        Console.SetCursorPosition(left, top);
        Console.Write("*");
    }

    public void UpdatePlayerHealthOnConsole()
    {
        var pos = 1;
        foreach (Unit unit in _playerParty)
        {
            UpdateCurrentHp(unit.Hp, pos);
            pos++;
        }
    }

    private void UpdateCurrentHp(int currentHp, int pos)
    {
        const int firstHpLine = 4;
        int top = firstHpLine + (pos - 1);
        
        // Left coords. are always the same for every character.
        (int start, int end) hpRange = (_panelPosition.left + 12, _panelPosition.left + 12 + 4);

        // Make space for new hp value.
        ConsoleExtensions.ClearRange(hpRange, top);

        // Find correct start position to start writing hp value by looking at length of string.
        int startPos = hpRange.start + 4 - currentHp.ToString().Length;

        // Write actual hp value as a text.
        Console.SetCursorPosition(startPos, top);
        Console.Write(currentHp.ToString());
    }
}