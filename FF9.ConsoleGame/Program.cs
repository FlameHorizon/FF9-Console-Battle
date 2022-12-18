// See https://aka.ms/new-console-template for more information

using FF9.ConsoleGame;
using FF9.ConsoleGame.Battle;
using FF9.ConsoleGame.UI;

IEnumerable<Unit> playerParty = new[] { InitialUnit.Warrior("Zidane", isPlayer: true) };
IEnumerable<Unit> enemyParty = new[] { InitialUnit.Warrior("Goblin", isPlayer: false) };

var btlEngine = new BattleEngine(playerParty, enemyParty);

var game = new Game(btlEngine);
game.Start();