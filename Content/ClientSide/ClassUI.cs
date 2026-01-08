using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using ClassesNamespace;
using CTG2.Content.ClientSide;
using CTG2.Content.ServerSide;
using Terraria.GameContent;
using CTG2;

public class ClassUI : UIState
{
    private UIPanel _mainPanel;
    private UIList _classList;
    private UIList _upgradeList;
    // private PlayerPreviewElement _preview;
    private UIText _classNameText;
    private UIText _classSummaryText;
    private UIProgressBar _hpBar;
    private UIProgressBar _mobilityBar;
    private ClassConfig selectedClass;
    private UpgradeConfig selectedUpgrade;
    private string _lastGamemode;

    public override void OnInitialize()
    {
        if (Main.dedServ) return;

        // Main container
        _mainPanel = new UIPanel
        {
            Width = { Percent = 0.65f },
            Height = { Percent = 0.6f },
            HAlign = 0.5f,
            VAlign = 0.5f,
            BackgroundColor = new Color(209, 25, 255, 100)
        };
        Append(_mainPanel);

        AddClassList();
        AddClassInfo();
        AddPlayerPreview();
        AddStatBars();
        AddUpgradeList();

        // initialize last-known gamemode so we can detect changes and refresh UI
        _lastGamemode = GetCurrentGamemode();

        // initial populate
        PopulateClasses();
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        // Auto-refresh the class list when gamemode changes.
        var gm = ModContent.GetInstance<GameManager>();
        string currentMode = GetCurrentGamemode();
        if (gm != null && currentMode != _lastGamemode)
        {
            _lastGamemode = currentMode;
            PopulateClasses();
        }
    }

    private string GetCurrentGamemode()
    {
        var gm = ModContent.GetInstance<GameManager>();
        if (gm == null) return "unknown";

        if (gm.rngConfig) return "rng";
        if (gm.scrimsConfig) return "scrims";
        if (gm.pubsConfig) return "pubs";
        return "none";
    }

    private void AddClassList()
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
            Height = { Percent = 1f },
            ListPadding = 5f
        };
        var bar = new UIScrollbar
        {
            Height = { Percent = 1f },
            HAlign = 1f,
            VAlign = 0f
        };
        _classList.SetScrollbar(bar);
        panel.Append(_classList);
        panel.Append(bar);
        _mainPanel.Append(panel);
    }

    private void AddClassInfo()
    {
        var infoPanel = new UIPanel
        {
            Left = { Percent = 0.25f },
            Top = { Pixels = 10 },
            Width = { Percent = 0.5f },
            Height = { Pixels = 60 },
            BackgroundColor = new Color(50, 50, 70, 200)
        };
        _classNameText = new UIText("Class Name")
        {
            Top = { Pixels = 6 },
            HAlign = 0.5f
        };
        _classSummaryText = new UIText("Class description goes here.")
        {
            Top = { Pixels = 30 },
            HAlign = 0.5f,
            OverflowHidden = true
        };
        infoPanel.Append(_classNameText);
        infoPanel.Append(_classSummaryText);
        _mainPanel.Append(infoPanel);
    }

    private void AddPlayerPreview()
    {   /*
        _preview = new PlayerPreviewElement(width: 100, height: 140) {
            HAlign = 0.5f,
            VAlign = 0.5f
        };
        _mainPanel.Append(_preview); */
    }

    private void AddStatBars()
    {
        var statsPanel = new UIPanel
        {
            Width = { Pixels = 220 },
            Height = { Pixels = 50 },
            HAlign = 0.5f,
            Top = { Percent = 0.75f },
            BackgroundColor = new Color(70, 50, 100, 200)
        };
        // You can implement UIProgressBar yourself or just use UIText placeholders:
        _hpBar = new UIProgressBar()
        {
            Top = { Pixels = 6 },
            Left = { Pixels = 6 },
            Width = { Pixels = 200 },
            Height = { Pixels = 12 }
        };
        _mobilityBar = new UIProgressBar()
        {
            Top = { Pixels = 30 },
            Left = { Pixels = 6 },
            Width = { Pixels = 200 },
            Height = { Pixels = 12 }
        };
        statsPanel.Append(_hpBar);
        statsPanel.Append(_mobilityBar);
        _mainPanel.Append(statsPanel);
    }

    private void AddUpgradeList()
    {
        var panel = new UIPanel
        {
            Left = { Percent = 0.75f },
            Top = { Pixels = 0 },
            Width = { Percent = 0.2f },
            Height = { Percent = 1f },
            BackgroundColor = new Color(50, 70, 50, 200)
        };
        _upgradeList = new UIList
        {
            Top = { Pixels = 10 },
            Width = { Percent = 1f },
            Height = { Percent = 1f },
            ListPadding = 5f
        };
        var bar = new UIScrollbar
        {
            Height = { Percent = 1f },
            HAlign = 1f,
            VAlign = 0f
        };
        _upgradeList.SetScrollbar(bar);
        panel.Append(_upgradeList);
        panel.Append(bar);
        _mainPanel.Append(panel);
    }

    private void PopulateClasses()
    {
        _classList.Clear();
        var gameManager = ModContent.GetInstance<GameManager>();
        foreach (var cls in CTG2.CTG2.config.Classes)
        {   
            if (cls.AbilityID == 18 && !gameManager.rngConfig)
                continue;
            if (cls.AbilityID != 18 && gameManager.rngConfig)
                continue;
            var btn = new UITextPanel<string>(cls.Name)
            {
                Width = { Percent = 1f },
                Height = { Pixels = 30 }
            };
            Color PanelColor = Color.DarkMagenta;
            btn.BackgroundColor = PanelColor;
            // store original for restoring
            Color normal = PanelColor;
            Color hover = normal * 1.5f;
            Color selected = Color.Gold;

            btn.OnMouseOver += (evt, el) => {
                if (btn.BackgroundColor != selected)
                {
                    btn.BackgroundColor = hover;
                }
                
            };
            btn.OnMouseOut += (evt, el) =>
            {
                if (btn.BackgroundColor != selected)
                {
                    btn.BackgroundColor = normal;
                }
            };

            btn.OnLeftClick += (evt, el) => {
                // clear all siblings
                foreach (UITextPanel<string> button in _classList)
                {
                    if (button.BackgroundColor == selected)
                    {
                        button.BackgroundColor = normal;
                    }
                }
                btn.BackgroundColor = selected;
                SelectClass(cls);
            };
            _classList.Add(btn);
        }
        
        

        // default select first (removed for debugging purposes)
        // if (CTG2.CTG2.config.Classes.Any())
        //     SelectClass(CTG2.CTG2.config.Classes[0]);
    }

    private void SelectClass(ClassConfig cfg)
    {
        var playerManager = Main.LocalPlayer.GetModPlayer<PlayerManager>();
        playerManager.currentClass = cfg;
        //playerManager.currentUpgrade = cfg.Upgrades[0]; // Not needed
        
        playerManager.pickedClass = true;
        var mod = ModContent.GetInstance<CTG2.CTG2>();
        ModPacket classPacket = mod.GetPacket();
        classPacket.Write((byte)MessageType.UpdatePickedClass);
        classPacket.Write(Main.LocalPlayer.whoAmI);
        classPacket.Write(true);
        classPacket.Send(toClient: Main.LocalPlayer.whoAmI);

        var classPlayer = Main.LocalPlayer.GetModPlayer<ClassSystem>();
        classPlayer.setClass();
        

        selectedClass = cfg;
        _classNameText.SetText(cfg.Name);
        _classSummaryText.SetText(cfg.Summary);

        // update stats bars
        _hpBar.SetProgress(cfg.HealthFromKills / 100f);        // example
        _mobilityBar.SetProgress(cfg.RespawnTime / 10f);        // example

        // populate upgrades
        _upgradeList.Clear();
        foreach (var up in cfg.Upgrades)
        {
            var btn = new UITextPanel<string>(up.Name)
            {
                Width = { Percent = 1f },
                Height = { Pixels = 30 }
            };
            Color PanelColor = Color.DarkMagenta;
            btn.BackgroundColor = PanelColor;
            // store original for restoring
            Color normal = PanelColor;
            Color hover = normal * 1.5f;
            Color selected = Color.Gold;

            btn.OnMouseOver += (evt, el) => {
                if (btn.BackgroundColor != selected)
                {
                    btn.BackgroundColor = hover;
                }
                
            };
            btn.OnMouseOut += (evt, el) =>
            {
                if (btn.BackgroundColor != selected)
                {
                    btn.BackgroundColor = normal;
                }
            };

            btn.OnLeftClick += (evt, el) => {
                // clear all siblings
                foreach (UITextPanel<string> button in _upgradeList)
                {
                    if (button.BackgroundColor == selected)
                    {
                        button.BackgroundColor = normal;
                    }
                }
                btn.BackgroundColor = selected;
                playerManager.currentUpgrade = up;
            };
            _upgradeList.Add(btn);
        }
    }
}