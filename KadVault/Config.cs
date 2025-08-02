namespace KadVault;

public class Config
{
    public bool Debug { get; set; } = false;
    public string SafeOpeningSFXFilePath { get; set; }
    public string AlarmSFXFilePath { get; set; }
    public int VaultSideLegendaryCoinChance { get; set; }
    public int VaultSideRareCoinChance { get; set; }
    public int VaultSideCommonCoinChance { get; set; }
    public string CommonCoinID { get; set; }
    public string RareCoinID { get; set; }
    public string LegendaryCoinID { get; set; }
    public List<string> LegendaryItemsArray { get; set; }
}
