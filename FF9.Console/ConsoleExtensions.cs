namespace FF9.Console;

public static class ConsoleExtensions
{
    public static void ClearLine(int top)
    {
        int currLeft = System.Console.CursorLeft;
        int currTop = System.Console.CursorTop;
        
        System.Console.SetCursorPosition(0, top);
        System.Console.Write(new string(' ', System.Console.WindowWidth));
        System.Console.SetCursorPosition(currLeft, currTop);
    }
}