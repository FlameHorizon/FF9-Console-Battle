// See https://aka.ms/new-console-template for more information

using FF9.Console;
using FF9.Console.Battle;

var p = InitialUnit.Warrior("Whey", isPlayer:true);

var o = InitialUnit.Thief("Goblin");

var btlEngine = new BattleEngine(p, o);

var game = new Game(btlEngine);
game.Start();