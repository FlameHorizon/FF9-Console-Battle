// See https://aka.ms/new-console-template for more information

using Spectre.Console;

// Ask for the user's favorite fruit
string? fruit = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("What's your [green]favorite fruit[/]?")
        .PageSize(10)
        .MoreChoicesText("[grey](Move up and down to reveal more fruits)[/]")
        .AddChoices("Apple", "Apricot", "Avocado", "Banana", "Blackcurrant", 
            "Blueberry", "Cherry", "Cloudberry", "Cocunut"));

// Echo the fruit back to the terminal
AnsiConsole.WriteLine($"I agree. {fruit} is tasty!");