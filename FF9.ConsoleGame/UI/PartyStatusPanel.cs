using FF9.ConsoleGame.Battle;

namespace FF9.ConsoleGame.UI;

public class PartyStatusPanel
{
    private readonly BattleEngine _btlEngine;
    private List<Unit> _playerParty;

    public PartyStatusPanel(BattleEngine btlEngine)
    {
        _btlEngine = btlEngine;
    }
    
    public void DrawCharactersInfo()
    {
        _playerParty = _btlEngine.UnitsInBattle.Where(u => u.IsPlayer).ToList();
        Unit first = _playerParty.First();
        Unit second = _playerParty.Skip(1).First();
        Unit third = _playerParty.Skip(2).First();
        Unit forth = _playerParty.Skip(3).First();

        Console.SetCursorPosition(31, 3);
        Console.Write("|--------------------|");
        
        Console.SetCursorPosition(31, 4);
        Console.Write("|Name     |   HP|  MP|");
        
        Console.SetCursorPosition(31, 5);
        Console.Write("| {0} | {1}| {2}|", 
            first.Name.PadRight(7), 
            first.Hp.ToString().PadLeft(4), 
            first.Mp.ToString().PadLeft(3));
        
        Console.SetCursorPosition(31, 6);
        Console.Write("| {0} | {1}| {2}|", 
            second.Name.PadRight(7), 
            second.Hp.ToString().PadLeft(4), 
            second.Mp.ToString().PadLeft(3));
        
        Console.SetCursorPosition(31, 7);
        Console.Write("| {0} | {1}| {2}|", 
            third.Name.PadRight(7), 
            third.Hp.ToString().PadLeft(4), 
            third.Mp.ToString().PadLeft(3));
        
        Console.SetCursorPosition(31, 8);
        Console.Write("| {0} | {1}| {2}|", 
            forth.Name.PadRight(7), 
            forth.Hp.ToString().PadLeft(4), 
            forth.Mp.ToString().PadLeft(3));

    }
}