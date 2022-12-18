// See https://aka.ms/new-console-template for more information

using FF9.ConsoleGame;
using FF9.ConsoleGame.Battle;
using FF9.ConsoleGame.UI;

var p = InitialUnit.Warrior("Whey", isPlayer:true);

var o = InitialUnit.Thief("Goblin");

var btlEngine = new BattleEngine(p, o);

var game = new Game(btlEngine);
game.Start();