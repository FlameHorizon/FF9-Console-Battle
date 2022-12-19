using FF9.ConsoleGame.Battle;

namespace FF9.ConsoleGame.UI;

public class PartyStatusPanel
{
    private readonly BattleEngine _btlEngine;
    private readonly List<Unit> _playerParty;
    
    private readonly (int left, int top) _panelLefTopPosition;
    private readonly (int right, int bottom) _panelRightBottomPosition;
    private (int left, int top) _currentTurnIndicatorPosition;
    
    private readonly int _turnIndicatorLeft;

    public PartyStatusPanel(BattleEngine btlEngine) : this (btlEngine, (31, 3))
    { }

    public PartyStatusPanel(BattleEngine btlEngine, (int left, int top) panelLefTopPosition)
    {
        _btlEngine = btlEngine;
        _playerParty = _btlEngine.UnitsInBattle.Where(u => u.IsPlayer).ToList();
        _panelLefTopPosition = panelLefTopPosition;
        _panelRightBottomPosition = (0, _panelLefTopPosition.top + 6);
        _turnIndicatorLeft = _panelLefTopPosition.left + 1;
    }
    
    public void DrawCharactersInfo()
    {
        Unit first = _playerParty.First();
        Unit second = _playerParty.Skip(1).First();
        Unit third = _playerParty.Skip(2).First();
        Unit forth = _playerParty.Skip(3).First();

        Console.SetCursorPosition(_panelLefTopPosition.left, _panelLefTopPosition.top);
        Console.Write("|--------------------|");

        Console.SetCursorPosition(_panelLefTopPosition.left, _panelLefTopPosition.top + 1);
        Console.Write("|Name     |   HP|  MP|");
        
        Console.SetCursorPosition(_panelLefTopPosition.left, _panelLefTopPosition.top + 2);
        Console.Write("| {0} | {1}| {2}|", 
            first.Name.PadRight(7), 
            first.Hp.ToString().PadLeft(4), 
            first.Mp.ToString().PadLeft(3));
        
        Console.SetCursorPosition(_panelLefTopPosition.left, _panelLefTopPosition.top + 3);
        Console.Write("| {0} | {1}| {2}|", 
            second.Name.PadRight(7), 
            second.Hp.ToString().PadLeft(4), 
            second.Mp.ToString().PadLeft(3));
        
        Console.SetCursorPosition(_panelLefTopPosition.left, _panelLefTopPosition.top + 4);
        Console.Write("| {0} | {1}| {2}|", 
            third.Name.PadRight(7), 
            third.Hp.ToString().PadLeft(4), 
            third.Mp.ToString().PadLeft(3));
        
        Console.SetCursorPosition(_panelLefTopPosition.left, _panelLefTopPosition.top + 5);
        Console.Write("| {0} | {1}| {2}|", 
            forth.Name.PadRight(7), 
            forth.Hp.ToString().PadLeft(4), 
            forth.Mp.ToString().PadLeft(3));

    }

    public void UpdatePlayerTurnIndicator()
    {
        if (_btlEngine.Source.IsPlayer == false)
        {
            return;
        }

        // Erase previous turn indicator
        if (_currentTurnIndicatorPosition != (0, 0))
        {
            Console.SetCursorPosition(_currentTurnIndicatorPosition.left, _currentTurnIndicatorPosition.top);
            Console.Write(" ");
        }

        Console.SetCursorPosition(0, _panelRightBottomPosition.bottom + 1);
        // Find top position of a player, which is now taking turn on the console.
        string lookFor = " " + _btlEngine.Source.Name;
        var coords = ConsoleExtensions.IndexOfInConsole(lookFor);

        _currentTurnIndicatorPosition = (_turnIndicatorLeft, coords.First().Y);
        DrawTurnIndicator(_turnIndicatorLeft, coords.First().Y);
    }
    
    private static void DrawTurnIndicator(int left, int top)
    {
        Console.SetCursorPosition(left, top);
        Console.Write("*");
    }
}