using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;
using CTG2.Content.Commands.Auth;
using CTG2.Content.ClientSide;

namespace CTG2.Content.ClientSide
{
    public class ModAdminUI : UIState
    {
        private UIPanel _rootPanel;
        private UIList _playerList;
        private UIPanel _detailPanel;
        private UIText _detailTitle;
        private UIText _detailBody;
        private UIList _detailButtonList;

        private int _selectedPlayerIndex = -1;
        private AdminAction _pendingAction = AdminAction.None;
        private ViewMode _currentView = ViewMode.PlayerList;
        private bool _wasVisibleLastUpdate = false;

        private enum ViewMode
        {
            PlayerList,
            ActionMenu,
            ConfirmAction,
            TeamSelection
        }

        private enum AdminAction
        {
            None,
            Kick,
            Ban,
            Mute
        }

        public override void OnInitialize()
        {
            if (Main.dedServ) return;

            _rootPanel = new UIPanel
            {
                Width = { Percent = 0.55f },
                Height = { Percent = 0.65f },
                HAlign = 0.5f,
                VAlign = 0.5f,
                BackgroundColor = new Color(63, 82, 151, 220)
            };

            Append(_rootPanel);

            var leftPanel = new UIPanel
            {
                Width = { Percent = 0.38f },
                Height = { Percent = 1f },
                Left = { Pixels = 10 },
                Top = { Pixels = 10 },
                BackgroundColor = new Color(40, 55, 90, 220)
            };

            _playerList = new UIList
            {
                Width = { Percent = 1f },
                Height = { Percent = 1f },
                Top = { Pixels = 10 }
            };

            var playerScroll = new UIScrollbar
            {
                Height = { Percent = 1f },
                HAlign = 1f,
                VAlign = 0f
            };

            _playerList.SetScrollbar(playerScroll);
            leftPanel.Append(_playerList);
            leftPanel.Append(playerScroll);
            _rootPanel.Append(leftPanel);

            _detailPanel = new UIPanel
            {
                Width = { Percent = 0.58f },
                Height = { Percent = 1f },
                Left = { Percent = 0.42f },
                Top = { Pixels = 10 },
                BackgroundColor = new Color(40, 55, 90, 220)
            };

            _detailTitle = new UIText("")
            {
                Top = { Pixels = 10 },
                Left = { Pixels = 10 },
                Width = { Percent = 0.9f },
                Height = { Pixels = 30 }
            };

            _detailBody = new UIText("")
            {
                Top = { Pixels = 45 },
                Left = { Pixels = 10 },
                Width = { Percent = 0.9f },
                Height = { Pixels = 30 }
            };

            _detailButtonList = new UIList
            {
                Top = { Pixels = 90 },
                Width = { Percent = 1f },
                Height = { Percent = 0.75f },
                ListPadding = 8f
            };

            _detailPanel.Append(_detailTitle);
            _detailPanel.Append(_detailBody);
            _detailPanel.Append(_detailButtonList);
            _rootPanel.Append(_detailPanel);

            var closeButton = CreateButton("Close", ToggleClose, 0.85f);
            closeButton.Top.Set(-6f, 0f);
            closeButton.Left.Set(-6f, 1f);
            closeButton.HAlign = 1f;
            closeButton.VAlign = 0f;
            closeButton.Width.Set(90f, 0f);
            closeButton.Height.Set(30f, 0f);
            _rootPanel.Append(closeButton);

            RefreshPlayerList();
            RefreshDetailPanel();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            bool isVisible = Main.LocalPlayer?.GetModPlayer<PlayerManager>().ShowModUI == true;
            if (isVisible && !_wasVisibleLastUpdate)
            {
                RefreshPlayerList();
                RefreshDetailPanel();
            }

            _wasVisibleLastUpdate = isVisible;
        }

        private void RefreshPlayerList()
        {
            _playerList.Clear();

            for (int i = 0; i < Main.player.Length; i++)
            {
                Player player = Main.player[i];
                if (player == null || !player.active)
                    continue;

                int playerIndex = i;
                Color teamColor = player.team switch
                {
                    3 => new Color(40, 120, 255, 240),
                    1 => new Color(220, 40, 40, 240),
                    _ => new Color(63, 82, 151, 200)
                };

                var playerButton = CreateButton(player.name, () => SelectPlayer(playerIndex), 0.85f, teamColor);
                if (playerIndex == _selectedPlayerIndex)
                {
                    playerButton.BackgroundColor = Color.Gold;
                }
                _playerList.Add(playerButton);
            }

            _playerList.Recalculate();
        }

        private void SelectPlayer(int playerIndex)
        {
            _selectedPlayerIndex = playerIndex;
            _currentView = ViewMode.ActionMenu;
            _pendingAction = AdminAction.None;
            RefreshPlayerList();
            RefreshDetailPanel();
        }

        private void RefreshDetailPanel()
        {
            _detailButtonList.Clear();

            bool playerSelected = _selectedPlayerIndex >= 0 && _selectedPlayerIndex < Main.player.Length && Main.player[_selectedPlayerIndex]?.active == true;

            if (!playerSelected)
            {
                _detailTitle.SetText(string.Empty);
                _detailBody.SetText(string.Empty);
                _currentView = ViewMode.PlayerList;
                return;
            }

            Player target = Main.player[_selectedPlayerIndex];
            bool isAdmin = target.GetModPlayer<AuthPlayer>().IsAdmin;

            if (_currentView == ViewMode.ConfirmAction)
            {
                _detailTitle.SetText($"Are you sure you want to {_pendingAction.ToString().ToLowerInvariant()}?");
                _detailBody.SetText("Press Yes or No.");
            }
            else
            {
                _detailTitle.SetText(isAdmin ? "Admin" : "Player");
                _detailBody.SetText("Choose an action below.");
            }

            switch (_currentView)
            {
                case ViewMode.PlayerList:
                    break;

                case ViewMode.ActionMenu:
                    AddActionButton("Kick", BeginConfirmAction, AdminAction.Kick);
                    AddActionButton("Ban", BeginConfirmAction, AdminAction.Ban);
                    AddActionButton("Mute", BeginConfirmAction, AdminAction.Mute);
                    _detailButtonList.Add(CreateButton("Ping", SendPing, 0.85f));
                    _detailButtonList.Add(CreateButton("Whois", SendWhois, 0.85f));
                    AddActionButton("Set Team", OpenTeamSelection, AdminAction.None);
                    break;

                case ViewMode.ConfirmAction:
                    if (_pendingAction == AdminAction.None)
                    {
                        _detailBody.SetText("No action selected.");
                        _currentView = ViewMode.ActionMenu;
                        RefreshDetailPanel();
                        return;
                    }

                    _detailButtonList.Add(CreateButton("Yes", ConfirmAction, 0.85f));
                    _detailButtonList.Add(CreateButton("No", CancelConfirm, 0.85f));
                    break;

                case ViewMode.TeamSelection:
                    _detailBody.SetText("Choose the team to assign the selected player.");
                    _detailButtonList.Add(CreateButton("Blue Team", () => SendTeamChange(3), 0.85f));
                    _detailButtonList.Add(CreateButton("Red Team", () => SendTeamChange(1), 0.85f));
                    _detailButtonList.Add(CreateButton("Back", ReturnToActionMenu, 0.85f));
                    break;
            }

            _detailButtonList.Recalculate();
            _detailPanel.Recalculate();
        }

        private void AddActionButton(string label, Action<AdminAction> action, AdminAction actionType)
        {
            _detailButtonList.Add(CreateButton(label, () => action(actionType), 0.85f));
        }

        private void BeginConfirmAction(AdminAction action)
        {
            if (!HasSelectedPlayer())
                return;

            if (action == AdminAction.Kick || action == AdminAction.Ban || action == AdminAction.Mute)
            {
                Player target = Main.player[_selectedPlayerIndex];
                if (target != null && target.GetModPlayer<AuthPlayer>().IsAdmin)
                {
                    _detailBody.SetText("You cannot perform this action on another admin.");
                    return;
                }
            }

            _pendingAction = action;
            _currentView = ViewMode.ConfirmAction;
            RefreshDetailPanel();
        }

        private void ConfirmAction()
        {
            if (!HasSelectedPlayer())
                return;

            switch (_pendingAction)
            {
                case AdminAction.Kick:
                    SendKick();
                    break;
                case AdminAction.Ban:
                    SendBan();
                    break;
                case AdminAction.Mute:
                    SendMute();
                    break;
            }

            CloseMenu();
        }

        private void CancelConfirm()
        {
            _pendingAction = AdminAction.None;
            _currentView = ViewMode.ActionMenu;
            RefreshDetailPanel();
        }

        private void OpenTeamSelection(AdminAction _)
        {
            _currentView = ViewMode.TeamSelection;
            RefreshDetailPanel();
        }

        private void ReturnToActionMenu()
        {
            _currentView = ViewMode.ActionMenu;
            RefreshDetailPanel();
        }

        private void SendKick()
        {
            if (!HasSelectedPlayer())
                return;

            var packet = ModContent.GetInstance<CTG2>().GetPacket();
            packet.Write((byte)MessageType.RequestKickPlayer);
            packet.Write(Main.player[_selectedPlayerIndex].name);
            packet.Send();
        }

        private void SendBan()
        {
            if (!HasSelectedPlayer())
                return;

            var packet = ModContent.GetInstance<CTG2>().GetPacket();
            packet.Write((byte)MessageType.RequestBanPlayer);
            packet.Write(Main.player[_selectedPlayerIndex].name);
            packet.Send();
        }

        private void SendMute()
        {
            if (!HasSelectedPlayer())
                return;

            var packet = ModContent.GetInstance<CTG2>().GetPacket();
            packet.Write((byte)MessageType.RequestMute);
            packet.Write(_selectedPlayerIndex);
            packet.Send();
        }

        private void SendPing()
        {
            if (!HasSelectedPlayer())
                return;

            var packet = ModContent.GetInstance<CTG2>().GetPacket();
            packet.Write((byte)MessageType.RequestPlayerPing);
            packet.Write(Main.player[_selectedPlayerIndex].whoAmI);
            packet.Send();
        }

        private void SendWhois()
        {
            if (!HasSelectedPlayer())
                return;

            var packet = ModContent.GetInstance<CTG2>().GetPacket();
            packet.Write((byte)MessageType.RequestWhois);
            packet.Write(Main.player[_selectedPlayerIndex].whoAmI);
            packet.Send();
        }

        private void SendTeamChange(int team)
        {
            if (!HasSelectedPlayer())
                return;

            Main.player[_selectedPlayerIndex].team = team;

            var packet = ModContent.GetInstance<CTG2>().GetPacket();
            packet.Write((byte)MessageType.RequestTeamChange);
            packet.Write(_selectedPlayerIndex);
            packet.Write(team);
            packet.Send();

            _selectedPlayerIndex = -1;
            _currentView = ViewMode.PlayerList;
            _pendingAction = AdminAction.None;
            RefreshPlayerList();
            RefreshDetailPanel();
        }

        private bool HasSelectedPlayer()
        {
            return _selectedPlayerIndex >= 0 && _selectedPlayerIndex < Main.player.Length && Main.player[_selectedPlayerIndex]?.active == true;
        }

        private void ToggleClose()
        {
            Main.LocalPlayer.GetModPlayer<PlayerManager>().ShowModUI = false;
        }

        private void CloseMenu()
        {
            _selectedPlayerIndex = -1;
            _pendingAction = AdminAction.None;
            _currentView = ViewMode.PlayerList;
            Main.LocalPlayer.GetModPlayer<PlayerManager>().ShowModUI = false;
        }

        private UITextPanel<string> CreateButton(string text, Action onClick, float textScale = 1f, Color? baseColor = null)
        {
            Color normal = baseColor ?? new Color(63, 82, 151, 200);
            var button = new UITextPanel<string>(text, textScale, true)
            {
                Width = { Percent = 1f },
                Height = { Pixels = 30 },
                BackgroundColor = normal,
                BorderColor = Color.White
            };

            Color hover = normal * 1.5f;

            button.OnMouseOver += (evt, element) =>
            {
                if (button.BackgroundColor != Color.Gold)
                    button.BackgroundColor = hover;
            };

            button.OnMouseOut += (evt, element) =>
            {
                if (button.BackgroundColor != Color.Gold)
                    button.BackgroundColor = normal;
            };

            button.OnLeftClick += (evt, element) => onClick?.Invoke();
            return button;
        }
    }
}
