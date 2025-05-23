using Exiled.API.Interfaces;
using MapEditorReborn.Commands.ModifyingCommands.Position;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public int VaultRandomRareCoinChance { get; set; }
        public int VaultRandomCommonChance { get; set; }
        public uint CommonCoinID { get; set; }
        public uint RareCoinID { get; set; }
        public uint LegendaryCoinID { get; set; }

        public List<uint> LegendaryItemsArray { get; set; }

        public uint CommonCoinSpawnID { get; set; }
        public uint RareCoinSpawnID { get; set; }
        public uint LegendaryCoinSpawnID { get; set; }

        public bool LegendaryOnlyEvent { get; set; }

    }

    
}
