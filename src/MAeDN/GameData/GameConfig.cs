using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAeDN.GameData
{
   public class GameConfig
   {
      public Random Random { get; set; } = new Random(1);

      public HashSet<string> PlayerNames { get; set; } = new HashSet<string> { "Alice", "Bob" };

      public int FieldsPerPlayer { get; set; } = 10;

      public int PiecesPerPlayer { get; set; } = 4;

      // Max of DiceRoles needs to be < FieldsPerPlayer!
      public HashSet<int> DiceRolls { get; set; } = Enumerable.Range(1, 6).ToHashSet();

      public int MinGetOutOfHomeDiceRoll { get { return DiceRolls.Max(); } }

      public int CountOfGetOutOfHomeDiceRolls { get; set; } = 3;

      public int CanRollDiceAgainDiceRoll { get { return DiceRolls.Max(); } }

      public int PlayerCount { get { return PlayerNames.Count; } }

      public int GameSize { get { return Math.Max(PlayerCount, 4); } }
   }
}
