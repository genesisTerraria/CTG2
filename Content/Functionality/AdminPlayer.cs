using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using CTG2;

namespace CTG2.Content
{
    public class AdminPlayer : ModPlayer
    {
        public bool IsAdmin = false;

        public override void OnEnterWorld()
        {

            IsAdmin = false; // Reset on join
        }

        public override void SaveData(TagCompound tag)
        {
            tag["IsAdmin"] = IsAdmin;
        }

        public override void LoadData(TagCompound tag)
        {
            IsAdmin = tag.GetBool("IsAdmin");
        }
    }
}
