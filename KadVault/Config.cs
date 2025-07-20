using Exiled.API.Interfaces;
using System.Collections.Generic;

namespace KadVault
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; }
        public bool Debug { get; set; }
        public string SafeOpeningSFXFilePath { get; set; }
        public string AlarmSFXFilePath { get; set; }
        public int VaultMainLegendaryCoinChance { get; set; }
        public int VaultSideLegendaryCoinChance { get; set; }
        public int VaultSideRareCoinChance { get; set; }
        public int VaultSideCommonCoinChance { get; set; }
        public string CommonCoinID { get; set; }
        public string RareCoinID { get; set; }
        public string LegendaryCoinID { get; set; }

        public List<string> LegendaryItemsArray { get; set; }

        public bool LegendaryOnlyEvent { get; set; }

        public bool customItemSideSpawns { get; set; }

    }

    
}
