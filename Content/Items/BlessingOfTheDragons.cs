using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using Terraria.Audio;
using System.IO;

namespace CTG2.Content.Items
{
	[AutoloadEquip(EquipType.Shield)] // Load the spritesheet you create as a shield for the player when it is equipped.
	public class BlessingOfTheDragons : ModItem
	{
		public override void SetDefaults() {
			Item.width = 30;
			Item.height = 30;
			Item.rare = ItemRarityID.Red;
			Item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.GetModPlayer<BlessingOfTheDragonsPlayer>().DashAccessoryEquipped = true;
		}
	}

	public class BlessingOfTheDragonsPlayer : ModPlayer {
		public const int DashCooldown = 80; // Time (frames) between starting dashes. If this is shorter than DashDuration you can start a new dash before an old one has finished
		public const int DashDuration = 10; // Duration of the dash afterimage effect in frames

		public float DashVelocity = 10f; // The initial velocity.  10 velocity is about 37.5 tiles/second or 50 mph

		public bool dashKeybindActive = false; // Uses the hook keybind as the dash keybind

		// The fields related to the dash accessory
		public bool DashAccessoryEquipped;
		private int lastDashDelay = 0;
		public int DashDelay = 0; // frames remaining till we can dash again
		public int DashTimer = 0; // frames remaining in the dash

		public bool recentlyEnded = false;


		public void SendDash(Vector2 velocity, int toWho = -1, int fromWho = -1)
		{
			ModPacket packet = Mod.GetPacket();
			packet.Write((byte)MessageType.BlessingOfTheDragons);
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


				dashKeybindActive = CTG2.BlessingOfTheDragonsKeybind.JustPressed;

				lastDashDelay = DashDelay;
			}
		}


		public override void PreUpdateMovement() {

			if (Player.whoAmI == Main.myPlayer && !Player.HasBuff(BuffID.Webbed))
			{
				Vector2 newVelocity = Player.velocity;

				if (dashKeybindActive && DashDelay == 0 && DashAccessoryEquipped)
				{
					Vector2 direction = Main.MouseWorld - Player.Center;
					if (direction.Length() == 0)
						return;

					direction.Normalize();
					float horizontalReduction = Math.Abs(direction.X) * 0.25f; 
					float speedMultiplier = 1f - horizontalReduction;

					// Apply the multiplier to the base DashVelocity
					Vector2 dashVelocity = direction * (DashVelocity * speedMultiplier);
					Player.velocity = dashVelocity;
					DashDelay = DashCooldown;
					DashTimer = DashDuration;
					recentlyEnded = true;
				}

				if (DashDelay > 0) DashDelay--;

				Player.eocDash = DashTimer;

				if (DashTimer > 0) // If dash is active
				{
					// Afterimage effect
					Player.armorEffectDrawShadowEOCShield = true;
					DashTimer--;

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
			}
		}
	}
}
