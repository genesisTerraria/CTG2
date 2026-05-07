using System;
using Discord;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CTG2.Content.Systems.Client;

[Autoload(Side = ModSide.Client)]
public class CtgDiscordSdk : ModSystem
{
    private const long DiscordClientId = 1501749625013669968L;
    private const LogLevel DiscordSdkLogLevel = LogLevel.Info;

    private Discord.Discord _discord;
    private bool _initAttempted;
    private bool _initFailed;
    private User? _currentUser;
    private bool _alreadyLogged;
    private bool _discordUnsupportedPlatform;
    private On_Main.hook_Update _updateHook;

    public override void Load()
    {
        // TODO: remove this bypass once we ship a macOS discord_game_sdk.dylib
        if (OperatingSystem.IsMacOS())
        {
            _discordUnsupportedPlatform = true;
            Mod.Logger.Info("Discord SDK identity check is temporarily disabled on macOS.");
            return;
        }

        CtgNativeDiscordLibraryLoader.Register(Mod);
        TryInit();
    }

    public override void OnWorldUnload()
    {
        // Reset per-session state so the next world join re-runs the identity check.
        _alreadyLogged = false;
        _currentUser = null;
    }

    public override void Unload()
    {
        if (_updateHook != null)
        {
            On_Main.Update -= _updateHook;
            _updateHook = null;
        }

        _discord?.Dispose();
        _discord = null;
        _currentUser = null;
        _initAttempted = false;
        _initFailed = false;
        _alreadyLogged = false;
        _discordUnsupportedPlatform = false;
    }

    private void TryInit()
    {
        _initAttempted = true;
        _initFailed = false;

        try
        {
            _discord = new Discord.Discord(DiscordClientId, (ulong)CreateFlags.NoRequireDiscord);
        }
        catch (Exception e)
        {
            Mod.Logger.Warn("Discord SDK could not initialize (will retry on world enter)", e);
            _discord = null;
            _initFailed = true;
            return;
        }

        _discord.SetLogHook(DiscordSdkLogLevel, (level, message) =>
        {
            Action<object> log = level switch
            {
                LogLevel.Error => Mod.Logger.Error,
                LogLevel.Warn => Mod.Logger.Warn,
                LogLevel.Info => Mod.Logger.Info,
                LogLevel.Debug => Mod.Logger.Debug,
                _ => null
            };
            log?.Invoke($"[Discord]: {message}");
        });

        _discord.GetUserManager().OnCurrentUserUpdate += OnCurrentUserUpdate;

        if (_updateHook == null)
        {
            _updateHook = (orig, self, time) =>
            {
                orig(self, time);
                try
                {
                    _discord?.RunCallbacks();
                }
                catch (Exception e)
                {
                    Mod.Logger.Error("Discord RunCallbacks failed", e);
                }
            };
            On_Main.Update += _updateHook;
        }
    }

    private void OnCurrentUserUpdate()
    {
        try
        {
            _currentUser = _discord.GetUserManager().GetCurrentUser();
        }
        catch (ResultException)
        {
            _currentUser = null;
        }
    }

    public void LogCurrentUserIdentity()
    {
        if (_discordUnsupportedPlatform)
            return;

        if (_alreadyLogged)
            return;

        if (_initFailed || _discord == null)
        {
            Mod.Logger.Info("Retrying Discord SDK init...");
            TryInit();
        }

        if (_initFailed || _discord == null)
        {
            Mod.Logger.Warn("Discord identity could not be resolved.");
            RequestKickForDiscordFailure();
            return;
        }

        var user = _currentUser;
        if (user == null)
        {
            try
            {
                user = _discord.GetUserManager().GetCurrentUser();
            }
            catch (ResultException)
            {
                Mod.Logger.Warn("Discord identity could not be resolved.");
                RequestKickForDiscordFailure();
                return;
            }
        }

        Mod.Logger.Info($"Discord detected: username={user.Value.Username}, globalName=unavailable, id={user.Value.Id}");
        _alreadyLogged = true;

        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            var packet = ((CTG2)Mod).GetPacket();
            packet.Write((byte)MessageType.LogDiscordIdentity);
            packet.Write(Main.myPlayer);
            packet.Write(user.Value.Id);
            packet.Write(user.Value.Username ?? string.Empty);
            packet.Send();
        }
    }

    private void RequestKickForDiscordFailure()
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            return;

        var packet = ((CTG2)Mod).GetPacket();
        packet.Write((byte)MessageType.KickDiscordIdentityFailed);
        packet.Write(Main.myPlayer);
        packet.Send();
    }
}
