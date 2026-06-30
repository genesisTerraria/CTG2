using System;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace CTG2.ReeseIntegration;

internal readonly struct ReeseRecordingCommandResult
{
    public ReeseRecordingCommandResult(string message, Color color)
    {
        Message = message;
        Color = color;
    }

    public string Message { get; }
    public Color Color { get; }
}

internal static class ReeseRecordingControl
{
    public static ReeseRecordingCommandResult StopRecording(string reason)
    {
        if (!ModLoader.TryGetMod("Reese", out Mod reese))
        {
            return new ReeseRecordingCommandResult(
                "[Reese] Reese is not loaded, so no recording could be stopped.",
                Color.Red
            );
        }

        try
        {
            object isRecordingResult = reese.Call("IsRecording");

            if (isRecordingResult is not bool isRecording)
            {
                return new ReeseRecordingCommandResult(
                    "[Reese] Could not verify recording status. Reese may not support the IsRecording API.",
                    Color.Red
                );
            }

            if (!isRecording)
            {
                return new ReeseRecordingCommandResult(
                    "[Reese] No recording is currently active.",
                    Color.Yellow
                );
            }

            object stopResult = reese.Call("StopRecordingAndGetFilePath", reason);

            if (stopResult is string savedPath && !string.IsNullOrWhiteSpace(savedPath))
            {
                string replayName = Path.GetFileNameWithoutExtension(savedPath);

                return new ReeseRecordingCommandResult(
                    $"[Reese] Recording stopped successfully: {replayName}",
                    Color.LimeGreen
                );
            }

            return new ReeseRecordingCommandResult(
                "[Reese] Failed to stop recording cleanly. Reese did not return a saved replay path.",
                Color.Red
            );
        }
        catch (Exception exception)
        {
            ModContent.GetInstance<global::CTG2.CTG2>().Logger.Warn($"[Reese compat] Failed to stop Reese recording: {exception}");

            return new ReeseRecordingCommandResult(
                $"[Reese] Failed to stop recording: {exception.Message}",
                Color.Red
            );
        }
    }
}
