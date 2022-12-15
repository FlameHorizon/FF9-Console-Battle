namespace FF9.Console;

public class Game
{
    private int _cursorPositionLeft = 1;
    private int _cursorPositionTop = 1;
    
    public void Start()
    {
        System.Console.CursorVisible = false;
        
        System.Console.WriteLine("-----------");
        System.Console.WriteLine("|>Attack  |");
        System.Console.WriteLine("-----------");
        System.Console.WriteLine("| Steal   |");
        System.Console.WriteLine("-----------");
        System.Console.WriteLine("| Item    |");
        System.Console.WriteLine("-----------");

        
        while (true)
        {
            ConsoleKeyInfo k = System.Console.ReadKey(true);
            if (k.Key == ConsoleKey.B)
            {
                System.Console.WriteLine("Wrote B, exiting program.");
                break;
            }
            else if (k.Key == ConsoleKey.DownArrow)
            {
                // Create down offset.
                (int left, int top) moveOffset = (0, 2);
                
                // Check boundaries.
                if (_cursorPositionTop + moveOffset.top >= 6)
                {
                    continue;
                }
                
                // Clear cursor behind.
                System.Console.SetCursorPosition(_cursorPositionLeft,_cursorPositionTop);
                System.Console.Write(" ");

                (int left, int top) newCursorPosition =
                    (_cursorPositionLeft + moveOffset.left, _cursorPositionTop + moveOffset.top);
                
                // Write new cursor location.
                System.Console.SetCursorPosition(newCursorPosition.left, newCursorPosition.top) ;
                _cursorPositionLeft = newCursorPosition.left;
                _cursorPositionTop = newCursorPosition.top;
                System.Console.Write(">");
                
                
            }
            else if (k.Key == ConsoleKey.UpArrow)
            {
                // Create up offset.
                (int left, int top) moveOffset = (0, -2);
                
                // Check boundaries.
                if (_cursorPositionTop + moveOffset.top <= 0)
                {
                    continue;
                }
                
                // Clear cursor behind.
                System.Console.SetCursorPosition(_cursorPositionLeft,_cursorPositionTop);
                System.Console.Write(" ");

                (int left, int top) newCursorPosition =
                    (_cursorPositionLeft + moveOffset.left, _cursorPositionTop + moveOffset.top);
                
                // Write new cursor location.
                System.Console.SetCursorPosition(newCursorPosition.left, newCursorPosition.top) ;
                _cursorPositionLeft = newCursorPosition.left;
                _cursorPositionTop = newCursorPosition.top;
                System.Console.Write(">");
            }
            else
            {
                System.Console.WriteLine();
            }
        }
    }
}