using FF9.ConsoleGame.Battle;

namespace FF9.ConsoleGame;

public class ItemPanel
{
    private readonly BattleEngine _battleEngine;
    private readonly (int left, int top) _panelPosition;

    public ItemPanel(BattleEngine battleEngine, (int left, int top) panelPosition)
    {
        _battleEngine = battleEngine;
        _panelPosition = panelPosition;
    }
    
    public void Draw()
    {
        throw new NotImplementedException();
    }

    public void Hide()
    {
        throw new NotImplementedException();
    }

    public bool IsVisible { get; private set; }
    public Item? Item { get; private set; }
}