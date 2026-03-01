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
	public class ArcherDash : ModItem
	{
		public override void SetDefaults() {
			Item.width = 30;
			Item.height = 30;
			Item.rare = ItemRarityID.Red;
			Item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.GetModPlayer<ArcherDashPlayer>().DashAccessoryEquipped = true;
		}
	}

	public class ArcherDashPlayer : ModPlayer {
		public const int DashCooldown = 150; // Time (frames) between starting dashes. If this is shorter than DashDuration you can start a new dash before an old one has finished
		public const int DashDuration = 18; // Duration of the dash afterimage effect in frames

		public float DashVelocity = 14f; // The initial velocity.  10 velocity is about 37.5 tiles/second or 50 mph

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
			packet.Write((byte)MessageType.ArcherDash);
			packet.Write((byte)Player.whoAmI);
			packet.WriteVector2(velocity);
			packet.Write(DashTimer);
			packet.Write(DashDelay);
			packet.Send(toWho, fromWho);
		}


		public void RecieveDash(Player player, BinaryReader reader)
        {
            Vector2 velocity = reader.ReadVector2();
            int DashTimer = reader.ReadInt32();
            int DashDelay = reader.ReadInt32();

            player.velocity = velocity;
            player.gravity = 0f;

            // ensures afterimage + animation
            player.eocDash = DashTimer;
        }


		public override void ResetEffects() {
			// Reset our equipped flag. If the accessory is equipped somewhere, ExampleShield.UpdateAccessory will be called and set the flag before PreUpdateMovement
			if (Player.whoAmI == Main.myPlayer)
			{
				DashAccessoryEquipped = false;

				if (recentlyEnded && DashTimer == 0) {
					Vector2 newVelocity = Player.velocity;
					if (newVelocity != Vector2.Zero) {
						newVelocity.Normalize();
						newVelocity *= 4f;
						Player.gravity = 0.4f;
						Player.velocity = newVelocity;
					}
					recentlyEnded = false;
					DashVelocity = 14f;
				}


				dashKeybindActive = CTG2.ArcherDashKeybind.JustPressed;

				if (DashDelay == 0 && lastDashDelay != 0) SoundEngine.PlaySound(SoundID.Item35, Player.Center);

				lastDashDelay = DashDelay;
			}
		}


		public override void PreUpdateMovement() {

			if (Player.whoAmI == Main.myPlayer )
			{
				Vector2 newVelocity = Player.velocity;

				if (dashKeybindActive && DashDelay == 0 && DashAccessoryEquipped)
				{
					Vector2 direction = Main.MouseWorld - Player.Center;
					if (direction.Length() == 0)
						return;

					direction.Normalize();
					Vector2 dashVelocity = direction * DashVelocity;

					Player.velocity = dashVelocity;
					Player.gravity = 0f;
					DashDelay = DashCooldown;
					DashTimer = DashDuration;
					recentlyEnded = true;
				}

				if (DashDelay > 0) DashDelay--;

				Player.eocDash = DashTimer;

				if (DashTimer > 0) // If dash is active
				{
					if (DashTimer < 10) {
						DashVelocity -= 1f;
						if (Player.velocity != Vector2.Zero) {
							Vector2 decVelocity = Player.velocity;
							decVelocity.Normalize();
							decVelocity *= DashVelocity;
							Player.velocity = decVelocity;
						}
					}

					// Afterimage effect
					Player.armorEffectDrawShadowEOCShield = true;
					DashTimer--;

					// Send dash packet
					if (Main.netMode == NetmodeID.MultiplayerClient)
					{
						Vector2 velo = Player.velocity;
						velo.Normalize();
						velo *= DashVelocity;

						SendDash(velo);
					}
				}
				else
					Player.armorEffectDrawShadowEOCShield = false;
			}
		}
	}
}
