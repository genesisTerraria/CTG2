using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace CTG2.Content.ClientSide;

public class ClientConfig
{   
    [JsonPropertyName("blue-class-select")]
    public int[] BlueSelect { get; set; }
    
    [JsonPropertyName("blue-base")]
    public int[] BlueBase { get; set; }
    
    [JsonPropertyName("blue-gem")]
    public int[] BlueGem { get; set; }
    
    [JsonPropertyName("red-class-select")]
    public int[] RedSelect { get; set; }
    
    [JsonPropertyName("red-base")]
    public int[] RedBase { get; set; }
    
    [JsonPropertyName("red-gem")]
    public int[] RedGem { get; set; }
    
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