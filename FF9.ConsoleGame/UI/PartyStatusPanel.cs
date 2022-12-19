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

    public PartyStatusPanel(BattleEngine btlEngine) : this(btlEngine, (31, 3))
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
        Console.SetCursorPosition(_panelLefTopPosition.left, _panelLefTopPosition.top);
        Console.Write("|--------------------|");

        Console.SetCursorPosition(_panelLefTopPosition.left, _panelLefTopPosition.top + 1);
        Console.Write("|Name     |   HP|  MP|");

        int offset = 2;
        foreach (Unit unit in _playerParty)
        {
            Console.SetCursorPosition(_panelLefTopPosition.left, _panelLefTopPosition.top + offset);
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

    public void UpdatePlayerHealthOnConsole(Unit? first, Unit? second, Unit? third, Unit? forth)
    {
        const int spaceBetweenMessageLineAndBattleMenu = 2;
        const int spaceBetweenEachCharacter = 1;
        
        const int firstTop = 3 + spaceBetweenMessageLineAndBattleMenu;
        const int secondTop = firstTop + spaceBetweenEachCharacter;
        const int thirdTop = secondTop + spaceBetweenEachCharacter;
        const int forthTop = thirdTop + spaceBetweenEachCharacter;
        
        if (first is not null)
            UpdateCurrentHp(first.Hp, firstTop);
        
        if (second is not null)
            UpdateCurrentHp(second.Hp, secondTop);
        
        if (third is not null)
            UpdateCurrentHp(third.Hp, thirdTop);
        
        if (forth is not null)
            UpdateCurrentHp(forth.Hp, forthTop);
    }

    private static void UpdateCurrentHp(int currentHp, int top)
    {
        // Left coords. are always the same for every character.
        (int start, int end) hpRange = (43, 47);
        
        // Make space for new hp value.
        ClearRange(hpRange, top);
        
        // Find correct start position to start writing hp value by looking at length of string.
        int startPos = hpRange.start + 4 - currentHp.ToString().Length;
        
        // Write actual hp value as a text.
        Console.SetCursorPosition(startPos, top);
        Console.Write(currentHp.ToString());
    }

    private static void ClearRange((int start, int end) leftRange, int top)
    {
        for (int left = leftRange.start; left < leftRange.end; left++)
        {
            Console.SetCursorPosition(left, top);
            Console.Write(' ');
        }
    }
}