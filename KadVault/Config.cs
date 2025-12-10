namespace KadVault;

public sealed class Config
{
    public bool Debug { get; set; }
    public bool disableStatsTracking { get; set; } = false;
    public string SafeOpeningSFX { get; set; }
    public string AlarmSFX { get; set; }
    public int VaultSideLegendaryCoinChance { get; set; }
    public int VaultSideRareCoinChance { get; set; }
    public int VaultSideCommonCoinChance { get; set; }
    public string CommonCoinID { get; set; }
    public string RareCoinID { get; set; }
    public string LegendaryCoinID { get; set; }
    public List<string> LegendaryItemsArray { get; set; }
    public List<string> UtilityItemsArray { get; set; }
    public float OpeningXP { get; set; } = 500;
}
