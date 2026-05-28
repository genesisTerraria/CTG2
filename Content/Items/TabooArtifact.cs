using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using Terraria.Audio;
using System.IO;
using Microsoft.Xna.Framework.Input;
using Terraria.GameInput;

namespace CTG2.Content.Items
{
	[AutoloadEquip(EquipType.Shield)] // Load the spritesheet you create as a shield for the player when it is equipped.
	public class TabooArtifact : ModItem
	{
		public override void SetDefaults() {
			Item.width = 30;
			Item.height = 30;
			Item.rare = ItemRarityID.Red;
			Item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.GetModPlayer<TabooArtifactPlayer>().DashAccessoryEquipped = true;
		}
	}

	public class TabooArtifactPlayer : ModPlayer {
		public bool dashKeybindActive = false; // Uses the hook keybind as the dash keybind

		// The fields related to the dash accessory
		public bool DashAccessoryEquipped;
		public int DashDelay = 0; // frames remaining till we can dash again
		public int DashTimer = 0; // frames remaining in the dash
		public bool recentlyEnded = false;

		int counter = 0;

		public void SendDash(Vector2 velocity, int toWho = -1, int fromWho = -1)
		{
			ModPacket packet = Mod.GetPacket();
			packet.Write((byte)MessageType.TabooArtifact);
			packet.Write((byte)Player.whoAmI);
			packet.WriteVector2(velocity);
			packet.Write(DashDelay);
			packet.Send(toWho, fromWho);
		}

		public void ReceiveDash(Player player, BinaryReader reader)
        {
            Vector2 velocity = reader.ReadVector2();
            int DashDelay = reader.ReadInt32();

            player.velocity = velocity;

            // ensures afterimage + animation
            player.eocDash = DashTimer;
        }

		public override void ResetEffects() {
			// Reset our equipped flag. If the accessory is equipped somewhere, ExampleShield.UpdateAccessory will be called and set the flag before PreUpdateMovement
			if (Player.whoAmI == Main.myPlayer)
			{
				DashAccessoryEquipped = false;

				if (recentlyEnded && DashTimer == 0)
				{
					recentlyEnded = false;
				}

				dashKeybindActive = CTG2.TabooArtifactKeybind.JustPressed;
			}
		}

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (Player.whoAmI == Main.myPlayer && !Player.HasBuff(BuffID.Webbed))
			{
				if (dashKeybindActive && DashDelay == 0 && DashAccessoryEquipped)
				{
					Vector2 mouseOffset = 0.02f * (Main.MouseWorld - Player.Center);
					if (mouseOffset.Length() == 0)
						return;

					float speed = Math.Clamp(mouseOffset.X, -5.5f, 5.5f);
					int direction = Math.Sign(speed);
					speed += 9 * direction;
					Player.velocity.X = speed;

					Main.NewText(speed);

					Point tileAheadUpper = (Player.Center + new Vector2(direction * Player.width / 2f + 2f, Player.gravDir * (-Player.height) / 2f + Player.gravDir * 2f)).ToTileCoordinates();
					Point tileAheadMid = (Player.Center + new Vector2(direction * Player.width / 2f + 2f, 0f)).ToTileCoordinates();

					if (WorldGen.SolidOrSlopedTile(tileAheadUpper.X, tileAheadUpper.Y) || WorldGen.SolidOrSlopedTile(tileAheadMid.X, tileAheadMid.Y))
						Player.velocity.X *= 0.5f;

					DashDelay = (int) (MathF.Abs(speed) / 14.5f * 60);
					DashTimer = DashTimer = (int)(DashDelay / 60f * 30);
					recentlyEnded = true;

					counter = 0;
				}
			}
        }	

		public override void PreUpdateMovement()
		{
			if (Player.whoAmI != Main.myPlayer) return;

			if (DashDelay > 0)
				DashDelay--;

			Player.eocDash = DashTimer;

			if (DashTimer > 0) // If dash is active
			{
				if (counter % 2 == 0) Player.velocity.X *= 0.95f;
				DashTimer--;
				Player.armorEffectDrawShadowEOCShield = true;

				// Send dash packet
				if (Main.netMode == NetmodeID.MultiplayerClient)
				{
					Vector2 velo = Player.velocity;

					SendDash(velo);
				}
			}
			else
			{
				Player.armorEffectDrawShadowEOCShield = false;
			}

			counter++;
		}
	}
}
