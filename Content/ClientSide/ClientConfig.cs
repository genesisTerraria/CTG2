using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CTG2.Content.ClientSide;

public class ClientConfig
{   
    [JsonPropertyName("blue-class-select-1")]
    public int[] BlueSelect { get; set; }
    [JsonPropertyName("blue-class-select-2")]
    public int[] BlueSelect2 { get; set; }
    [JsonPropertyName("blue-class-select-3")]
    public int[] BlueSelect3 { get; set; }
    [JsonPropertyName("blue-class-select-4")]
    public int[] BlueSelect4 { get; set; }
    
    [JsonPropertyName("blue-base-1")]
    public int[] BlueBase { get; set; }
    [JsonPropertyName("blue-base-2")]
    public int[] BlueBase2 { get; set; }
    [JsonPropertyName("blue-base-3")]
    public int[] BlueBase3 { get; set; }
    [JsonPropertyName("blue-base-4")]
    public int[] BlueBase4 { get; set; }
    
    [JsonPropertyName("blue-gem-1")]
    public int[] BlueGem { get; set; }
    [JsonPropertyName("blue-gem-2")]
    public int[] BlueGem2 { get; set; }
    [JsonPropertyName("blue-gem-3")]
    public int[] BlueGem3 { get; set; }
    [JsonPropertyName("blue-gem-4")]
    public int[] BlueGem4 { get; set; }
    
    [JsonPropertyName("red-class-select-1")]
    public int[] RedSelect { get; set; }
    [JsonPropertyName("red-class-select-2")]
    public int[] RedSelect2 { get; set; }
    [JsonPropertyName("red-class-select-3")]
    public int[] RedSelect3 { get; set; }
    [JsonPropertyName("red-class-select-4")]
    public int[] RedSelect4 { get; set; }
    
    [JsonPropertyName("red-base-1")]
    public int[] RedBase { get; set; }
    [JsonPropertyName("red-base-2")]
    public int[] RedBase2 { get; set; }
    [JsonPropertyName("red-base-3")]
    public int[] RedBase3 { get; set; }
    [JsonPropertyName("red-base-4")]
    public int[] RedBase4 { get; set; }
    
    [JsonPropertyName("red-gem-1")]
    public int[] RedGem { get; set; }
    [JsonPropertyName("red-gem-2")]
    public int[] RedGem2 { get; set; }
    [JsonPropertyName("red-gem-3")]
    public int[] RedGem3 { get; set; }
    [JsonPropertyName("red-gem-4")]
    public int[] RedGem4 { get; set; }
    
    [JsonPropertyName("lobby")]
    public int[] Lobby { get; set; }
    
    [JsonPropertyName("map-paste")]
    public int[] MapPaste { get; set; }
    
    [JsonPropertyName("classes")]
    public List<ClassConfig> Classes { get; set; }
}

public class ClassConfig
{    
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("summary")]
    public string Summary { get; set; } = "";

    [JsonPropertyName("description")]
    public string Description { get; set; } = "";

    [JsonPropertyName("inventory")]
    public string Inventory { get; set; } = "";

    [JsonPropertyName("icon")]
    public int Icon { get; set; }

    [JsonPropertyName("ability-cooldown")]
    public int AbilityCooldown { get; set; }

    [JsonPropertyName("ability-2-cooldown")]
    public int Ability2Cooldown { get; set; }

    [JsonPropertyName("ability-3-cooldown")]
    public int Ability3Cooldown { get; set; }

    [JsonPropertyName("health-from-kills")]
    public int HealthFromKills { get; set; }

    [JsonPropertyName("respawn-time")]
    public int RespawnTime { get; set; }
    
    [JsonPropertyName("buffs")]
    public List<int> Buffs { get; set; }
    
    [JsonPropertyName("ability-id")]
    public int AbilityID { get; set; }

    [JsonPropertyName("upgrades")]
    public List<UpgradeConfig> Upgrades { get; set; }
}

public class UpgradeConfig
{
    [JsonPropertyName("id")]
    public string Id { get; set; } 

    [JsonPropertyName("name")]
    public string Name { get; set; } 

    [JsonPropertyName("icon")]
    public int Icon { get; set; }

    [JsonPropertyName("value")]
    public int Value { get; set; }
}