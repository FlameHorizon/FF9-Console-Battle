// See https://aka.ms/new-console-template for more information

using FF9.Console;
using FF9.Console.Battle;

var p = new Unit("Zidane", 105, 12, 0, 6);
var o = new Unit("Masked Man", 188, 8, 0, 10);

var btlEngine = new BattleEngine(p, o);

var game = new Game(btlEngine);
game.Start();

