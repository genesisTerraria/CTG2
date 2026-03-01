using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria.GameContent;

// A UITextPanel<string> that changes color on hover or selection
public class SelectableTextPanel : UITextPanel<string>
{
    private readonly Color _normalColor;
    private readonly Color _hoverColor;
    private readonly Color _selectedColor;
    private Color PanelColor =  new Color(255, 255, 255, 200);

    public bool IsSelected { get; set; }

    public SelectableTextPanel(string text) : base(text)
    {
        // capture the default
        _normalColor   = PanelColor;
        _hoverColor    = _normalColor * 1.2f;
        _selectedColor = Color.Gold * 0.9f;
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        // 1) update background color based on state
        if (IsSelected)
            PanelColor = _selectedColor;
        else if (IsMouseHovering)
            PanelColor = _hoverColor;
        else
            PanelColor = _normalColor;

        base.DrawSelf(spriteBatch);
    }
}