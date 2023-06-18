using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAeDN.GameData
{
   public readonly record struct PieceMoveResult(
      Piece Piece,
      bool Moved,
      int? NewRelativeFieldIndex = null, 
      Piece? ThrownOut = null)
   {
   }
}
