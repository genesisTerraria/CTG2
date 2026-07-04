using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;
using Microsoft.Xna.Framework;
using System.Linq;
using Terraria.ID;
using ClassesNamespace;
using CTG2.Content.ServerSide;

namespace CTG2.Content.ClientSide;

public class BanUI : UIState
{
    public static bool Visible { get; private set; }
    public static ClassConfig SelectedBan { get; private set; }

    private static BanUI _instance;

    private UIPanel _mainPanel;
    private UIPanel _mainPanel2;
    private UIList _classList;
    private UIList _classList2;
    private UITextPanel<string> _confirmButton;
    private ClassConfig _selectedClass;
    private int _banSelectCooldown; // ticks remaining
    private const int BanSelectCooldownMax = 60; // 60 ticks = 1 second
    private bool _dragging;
    private Vector2 _dragOffset;

    private static readonly Color PanelColor = new Color(63, 82, 151, 180);
    private static readonly Color SelectedColor = Color.Gold;

    public static void ShowBanUI()
    {
        Visible = true;
        SelectedBan = null;
        if (_instance != null)
        {
            _instance._selectedClass = null;
            _instance.PopulateClasses();
            _instance.UpdateConfirmButton();
        }
    }

    public static void HideBanUI()
    {
        Visible = false;
    }

    public override void OnInitialize()
    {
        if (Main.dedServ) return;

        _instance = this;

        _mainPanel = new UIPanel
        {
            Width = { Percent = 0.1f },
            Height = { Percent = 0.4f },
            HAlign = 0.3f,
            VAlign = 0.5f,
            BackgroundColor = PanelColor
        };

        _mainPanel2 = new UIPanel
        {
            Width = { Percent = 0.1f },
            Height = { Percent = 0.4f },
            HAlign = 0.4f,
            VAlign = 0.5f,
            BackgroundColor = PanelColor
        };

        _mainPanel.OnLeftMouseDown += StartDrag;
        _mainPanel.OnLeftMouseUp += StopDrag;
        _mainPanel2.OnLeftMouseDown += StartDrag;
        _mainPanel2.OnLeftMouseUp += StopDrag;

        Append(_mainPanel);
        Append(_mainPanel2);

        AddClassLists();
        AddConfirmButton();

        PopulateClasses();
    }

    private void StartDrag(UIMouseEvent evt, UIElement listeningElement)
    {
        _dragging = true;
        _dragOffset = evt.MousePosition - new Vector2(_mainPanel.Left.Pixels, _mainPanel.Top.Pixels);
    }

    private void StopDrag(UIMouseEvent evt, UIElement listeningElement)
    {
        _dragging = false;
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (_dragging)
        {
            Vector2 mouse = Main.MouseScreen;

            _mainPanel.Left.Set(mouse.X - _dragOffset.X, 0f);
            _mainPanel.Top.Set(mouse.Y - _dragOffset.Y, 0f);
            _mainPanel.Recalculate();

            _mainPanel2.Left.Set(_mainPanel.Left.Pixels, 0f);
            _mainPanel2.Top.Set(_mainPanel.Top.Pixels, 0f);
            _mainPanel2.Recalculate();
        }

        if (_banSelectCooldown > 0)
            _banSelectCooldown--;
    }

    private void AddClassLists()
    {
        var panel = new UIPanel
        {
            Left = { Pixels = 0 },
            Top = { Pixels = 0 },
            Width = { Pixels = 200 },
            Height = { Percent = 1f }
        };
        _classList = new UIList
        {
            Top = { Pixels = 10 },
            Width = { Percent = 1f },
            Height = { Percent = 1f }
        };
        var bar = new UIScrollbar
        {
            Height = { Percent = 1f },
            HAlign = 1f,
            VAlign = 0f
        };
        _classList.SetScrollbar(bar);
        panel.Append(_classList);
        _mainPanel.Append(panel);

        var panel2 = new UIPanel
        {
            Left = { Pixels = 0 },
            Top = { Pixels = 0 },
            Width = { Pixels = 200 },
            // Leave room at the bottom of the right column for the confirm button
            Height = { Percent = 1f, Pixels = -46 }
        };
        _classList2 = new UIList
        {
            Top = { Pixels = 10 },
            Width = { Percent = 1f },
            Height = { Percent = 1f }
        };
        var bar2 = new UIScrollbar
        {
            Height = { Percent = 1f },
            HAlign = 1f,
            VAlign = 0f
        };
        _classList2.SetScrollbar(bar2);
        panel2.Append(_classList2);
        _mainPanel2.Append(panel2);
    }

    private void AddConfirmButton()
    {
        _confirmButton = new UITextPanel<string>("Confirm Ban")
        {
            Width = { Percent = 1f },
            Height = { Pixels = 36 },
            HAlign = 1f,
            VAlign = 1f,
            BackgroundColor = new Color(120, 40, 40, 200)
        };

        _confirmButton.OnMouseOver += (evt, el) =>
        {
            if (_selectedClass != null)
                _confirmButton.BackgroundColor = new Color(180, 60, 60, 220);
        };
        _confirmButton.OnMouseOut += (evt, el) => UpdateConfirmButton();

        _confirmButton.OnLeftClick += (evt, el) =>
        {
            if (_selectedClass == null)
                return;

            ConfirmBan(_selectedClass);
        };

        _mainPanel2.Append(_confirmButton);
    }

    private void ConfirmBan(ClassConfig cfg)
    {
        SelectedBan = cfg;

        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            // Tell the server which class to ban; it resolves the team from the sender's whoAmI.
            ModPacket packet = ModContent.GetInstance<CTG2>().GetPacket();
            packet.Write((byte)MessageType.SubmitClassBan);
            packet.Write(cfg.AbilityID);
            packet.Send();
        }
        else
        {
            // Singleplayer (/showbanui testing): apply the ban to the local player's own team
            // so the grey-out is visible in the class UI.
            if (Main.LocalPlayer.team == 3)
                GameInfo.blueTeamBannedClassID = cfg.AbilityID;
            else
                GameInfo.redTeamBannedClassID = cfg.AbilityID;
        }

        Main.NewText($"You banned {cfg.Name} for the opposing team.", Color.OrangeRed);

        // Ban UI is purely clientside, so hide it directly instead of round-tripping a packet.
        HideBanUI();
    }

    private void UpdateConfirmButton()
    {
        // Dim the button until a class has been selected
        _confirmButton.BackgroundColor = _selectedClass != null
            ? new Color(150, 45, 45, 220)
            : new Color(120, 40, 40, 120);
    }

    private void PopulateClasses()
    {
        _classList.Clear();
        _classList2.Clear();
        var gameManager = ModContent.GetInstance<GameManager>();
        for (int id = 1; id <= 9; id++)
        {
            var cls = CTG2.config.Classes.FirstOrDefault(c => c.AbilityID == id);
            if (cls == null)
                continue;

            // RNG CTG: only RngMan (column 2) is allowed; keep first column empty like classes 10–19 loop.
            if (gameManager != null && gameManager.rngConfig)
                continue;

            _classList.Add(MakeClassButton(cls));
        }

        for (int id = 10; id <= 19; id++)
        {
            var cls = CTG2.config.Classes.FirstOrDefault(c => c.AbilityID == id);
            if (cls == null)
                continue;

            if (cls.AbilityID == 18 && !gameManager.rngConfig)
                continue;
            if (cls.AbilityID != 18 && gameManager.rngConfig)
                continue;

            _classList2.Add(MakeClassButton(cls));
        }
    }

    private UITextPanel<string> MakeClassButton(ClassConfig cls)
    {
        var btn = new UITextPanel<string>(cls.Name)
        {
            Width = { Percent = 1f },
            Height = { Pixels = 30 }
        };
        Color normal = PanelColor;
        Color hover = normal * 1.5f;
        btn.BackgroundColor = normal;

        btn.OnMouseOver += (evt, el) =>
        {
            if (btn.BackgroundColor != SelectedColor)
                btn.BackgroundColor = hover;
        };
        btn.OnMouseOut += (evt, el) =>
        {
            if (btn.BackgroundColor != SelectedColor)
                btn.BackgroundColor = normal;
        };

        btn.OnLeftClick += (evt, el) =>
        {
            if (_banSelectCooldown > 0)
                return;

            foreach (UITextPanel<string> button in _classList)
            {
                if (button.BackgroundColor == SelectedColor)
                    button.BackgroundColor = normal;
            }

            foreach (UITextPanel<string> button in _classList2)
            {
                if (button.BackgroundColor == SelectedColor)
                    button.BackgroundColor = normal;
            }

            btn.BackgroundColor = SelectedColor;
            _selectedClass = cls;
            UpdateConfirmButton();

            _banSelectCooldown = BanSelectCooldownMax;
        };

        return btn;
    }
}
