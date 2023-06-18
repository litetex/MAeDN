using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAeDN.GameData
{
   public class Player
   {
      public Game Game { get; private set; }

      public string Name { get; init; }

      public HashSet<Piece> Pieces { get; init; }

      public PlayerState State 
      { 
         get
         {
            if (Pieces.All(p => p.IsFinished))
               return PlayerState.FINISHED;

            return Pieces.Any(p => p.IsOnFields) ? PlayerState.ON_FIELDS : PlayerState.HOME;
         } 
      }

      public HashSet<Piece> MoveablePieces 
      { 
         get 
         { 
            return Pieces
               .Where(p => !p.IsHome)
               .Where(p => p.RelativeFieldIndexSafe != Game.TotalNumberOfFieldsWithFinish - 1)
               .ToHashSet(); 
         } 
      }

      public HashSet<Piece> PiecesAtHome
      {
         get
         {
            return Pieces
               .Where(p => p.IsHome)
               .ToHashSet();
         }
      }

      public Player(Game game, string name)
      {
         Game = game;
         Name = name;
         Pieces = Enumerable.Range(0, Game.Config.PiecesPerPlayer)
            .Select(i => new Piece(this, i + 1))
            .ToHashSet();
      }

      public int TotalFieldIndex(int relativeFieldIndex)
      {
         return (relativeFieldIndex + (Game.Players.IndexOf(this) * Game.Config.FieldsPerPlayer)) % Game.TotalNumberOfFields;
      }
   }
}
