using System.Collections.Generic;
using System.Linq;
using DirectDashMod.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace DirectDashMod.Items;

public abstract class DashGemBase : ModItem
{
	public const int LIGHT_COL = 128;

	public const int TILESIZE = 16;

	public const float BASE_DASH_DIST = 160f;

	public const float BASE_DASH_SPEED = 9.6f;

	public readonly Color PASSIVE_COL = Color.LightGoldenrodYellow;

	public static Asset<Texture2D> Sheet;

	public virtual float DashDist => 160f;

	public virtual float DashSpeed => 9.6f;

	public virtual int DashCooldown => 300;

	public virtual int DashCount => 1;

	public virtual Color DrawColor => Color.White;

	public virtual Rectangle DrawFrame => new Rectangle(0, 0, 32, 32);

	public virtual float DrawScale => 7f;

	public virtual bool RunDash => false;

	public virtual bool BounceDash => false;

	public virtual bool SafeDash => false;

	public virtual Vector2 BounceBoost => WallJumpPlayer.BASE_CANCEL_BOOST;

	public virtual int ConstDefense => 0;

	public virtual float ConstSpeed => 0f;

	public virtual bool OffensiveDash => false;

	public virtual bool BlackBelt => false;

	public virtual int StaminaBoost => 0;

	public bool HasImage
	{
		get
		{
			if (this.Texture != null)
			{
				return !this.Texture.Equals("CTG2/Imported/DirectDashMod/Images/empty");
			}
			return false;
		}
	}

	public override string Texture => "CTG2/Imported/DirectDashMod/Images/empty";

	public override void Load()
	{
		DashGemBase.Sheet = ModContent.Request<Texture2D>("CTG2/Imported/DirectDashMod/Items/DashRocks");
	}

	public override void SetDefaults()
	{
		base.Item.width = this.DrawFrame.Width;
		base.Item.height = this.DrawFrame.Height;
		base.Item.value = 10000;
		base.Item.accessory = true;
		base.Item.color = this.DrawColor;
		base.Item.rare = 2;
		base.Item.defense = this.ConstDefense;
	}

	public override void UpdateEquip(Player player)
	{
		if (player.TryGetModPlayer<DashPlayer3>(out var dashPly))
		{
			dashPly.acc_dashDist += this.DashDist;
			dashPly.acc_dashSpeed += this.DashSpeed;
			dashPly.acc_dashCountMax += this.DashCount;
			dashPly.acc_dashCooldown += this.DashCooldown;
			dashPly.acc_Color = this.DrawColor;
			dashPly.acc_RunDash = this.RunDash;
			dashPly.acc_SafeDash = this.SafeDash;
			if (this.OffensiveDash)
			{
				dashPly.acc_ShieldDash = true;
			}
		}
		if (player.TryGetModPlayer<WallJumpPlayer>(out var jumpPly))
		{
			jumpPly.acc_CanWallJump = true;
			jumpPly.acc_ChainBounce = jumpPly.acc_ChainBounce || this.BounceDash;
			jumpPly.acc_CancelBoost *= this.BounceBoost;
			if (this.StaminaBoost > 0)
			{
				jumpPly.acc_Stamina += this.StaminaBoost;
			}
		}
		if (this.ConstSpeed > 0f)
		{
			player.moveSpeed += this.ConstSpeed;
		}
		if (this.BlackBelt)
		{
			player.blackBelt = true;
		}
	}

	public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
	{
		Texture2D tex = (this.HasImage ? TextureAssets.Item[base.Item.type].Value : DashGemBase.Sheet.Value);
		spriteBatch.Draw(tex, position, this.DrawFrame, itemColor.MultiplyRGBA(drawColor), 0f, this.DrawFrame.Size() / 2f, scale, SpriteEffects.None, 0f);
		return false;
	}

	public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
	{
		Texture2D tex = (this.HasImage ? TextureAssets.Item[base.Item.type].Value : DashGemBase.Sheet.Value);
		spriteBatch.Draw(tex, base.Item.position - Main.screenPosition, this.DrawFrame, lightColor.MultiplyRGB(this.DrawColor).MultiplyRGB(alphaColor), rotation, Vector2.Zero, scale, SpriteEffects.None, 0f);
		return false;
	}

	public override void ModifyTooltips(List<TooltipLine> tooltips)
	{
		int indx = tooltips.FindIndex((TooltipLine tool) => tool.IsModifier || tool.IsModifierBad || tool.Text.Equals("Material"));
		if (indx == -1)
		{
			indx = tooltips.Count();
		}
		int dval;
		if ((dval = (int)((this.DashDist - 160f) / 16f)) != 0)
		{
			tooltips.Insert(indx, new TooltipLine(base.Mod, "DashGem_Distance", $"{((dval > 0) ? "+" : "")}{dval} Dash Distance"));
		}
		if ((dval = (int)((this.DashSpeed - 9.6f) / 9.6f * 100f)) != 0)
		{
			tooltips.Insert(indx, new TooltipLine(base.Mod, "DashGem_Speed", $"{((dval > 0) ? "+" : "")}{dval}% Dash Speed"));
		}
		if (this.RunDash)
		{
			tooltips.Insert(indx, new TooltipLine(base.Mod, "DashGem_Run", "+Running dash")
			{
				OverrideColor = this.PASSIVE_COL
			});
		}
		if (this.BounceDash)
		{
			tooltips.Insert(indx, new TooltipLine(base.Mod, "DashGem_Bounce", "+Bounce Reset")
			{
				OverrideColor = this.PASSIVE_COL
			});
		}
		if (this.SafeDash)
		{
			tooltips.Insert(indx, new TooltipLine(base.Mod, "DashGem_Safe", "+Invincible Dash")
			{
				OverrideColor = this.PASSIVE_COL
			});
		}
		if (this.OffensiveDash)
		{
			tooltips.Insert(indx, new TooltipLine(base.Mod, "DashGem_Offense", "+Offensive Dash")
			{
				OverrideColor = this.PASSIVE_COL
			});
		}
		if (this.StaminaBoost > 0)
		{
			tooltips.Insert(indx, new TooltipLine(base.Mod, "DashGem_Stamina", "+Stamina Boost")
			{
				OverrideColor = this.PASSIVE_COL
			});
		}
		if (this.BlackBelt)
		{
			tooltips.Insert(indx, new TooltipLine(base.Mod, "DashGem_Blackbelt", "+Blackbelt")
			{
				OverrideColor = this.PASSIVE_COL
			});
		}
		if (this.DashCount == 1)
		{
			tooltips.Insert(indx, new TooltipLine(base.Mod, "DashGem_Uses", $"Allows you to dash once every {this.DashCooldown / 60} seconds"));
		}
		else if (this.DashCount == 2)
		{
			tooltips.Insert(indx, new TooltipLine(base.Mod, "DashGem_Uses", $"Allows you to dash twice every {this.DashCooldown / 60} seconds"));
		}
		else
		{
			tooltips.Insert(indx, new TooltipLine(base.Mod, "DashGem_Uses", $"Allows you to dash {this.DashCount} times every {this.DashCooldown / 60} seconds"));
		}
		if (this.ConstSpeed > 0f)
		{
			tooltips.Insert(indx, new TooltipLine(base.Mod, "DashGem_Speed", $"{(int)(this.ConstSpeed * 100f)}% increased move speed"));
		}
	}

	public override bool CanAccessoryBeEquippedWith(Item equippedItem, Item incomingItem, Player player)
	{
		if (incomingItem.ModItem != null && equippedItem.ModItem != null && incomingItem.ModItem.GetType().IsSubclassOf(typeof(DashGemBase)) && equippedItem.ModItem.GetType().IsSubclassOf(typeof(DashGemBase)))
		{
			return false;
		}
		return true;
	}
}
