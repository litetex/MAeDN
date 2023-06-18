using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace MAeDN.GameData
{
   public class Piece
   {
      public Player Player { get; private set; }

      private int Number { get; set; }

      public string Identifier { get { return ((char)(Number + 64)).ToString(); } }

      public int? RelativeFieldIndex { get; private set; }

      public int RelativeFieldIndexSafe { get { return RelativeFieldIndex ?? -1; } }

      public bool IsHome { get { return RelativeFieldIndex == null; } }

      public bool IsFinished { get { return RelativeFieldIndex >= Player.Game.TotalNumberOfFields; } }

      public bool IsOnFields { get { return !IsFinished && !IsHome; } }

      public int? TotalFieldIndex
      {
         get
         {
            return !IsOnFields ? null : Player.TotalFieldIndex(RelativeFieldIndexSafe);
         }
      }

      public int TotalFieldIndexSafe { get { return TotalFieldIndex ?? -1; } }

      public Piece(Player player, int number)
      {
         Player = player;
         Number = number;
      }

      public bool ThrowOut(Player player, bool simulateOnly)
      {
         if(Player == player)
            return false;

         if(!simulateOnly)
            RelativeFieldIndex = null;

         return true;
      }

      public PieceMoveResult Move(int numberOfFields, bool simulateOnly)
      {
         var game = Player.Game;
         if (IsHome)
            return TryToMoveOutOfHome(numberOfFields, simulateOnly);

         var newRelativeFieldIndex = RelativeFieldIndexSafe + numberOfFields;
         // Check if is in range
         if(newRelativeFieldIndex >= game.TotalNumberOfFieldsWithFinish)
            return new PieceMoveResult(this, false, RelativeFieldIndex);

         var pieceToThrowOut = game.GetPieceAt(Player, newRelativeFieldIndex);
         var canMove = pieceToThrowOut?.ThrowOut(Player, simulateOnly) ?? true;
         if (canMove)
         {
            if(!simulateOnly)
               RelativeFieldIndex = newRelativeFieldIndex;
         }
         return new PieceMoveResult(this, canMove, newRelativeFieldIndex, pieceToThrowOut);
      }

      private PieceMoveResult TryToMoveOutOfHome(int numberOfFields, bool simulateOnly)
      {
         var game = Player.Game;
         // Was numberOfFields high enough?
         if (numberOfFields >= game.Config.MinGetOutOfHomeDiceRoll)
         {
            // Can move out of the home?
            var pieceToThrowOut = game.GetPieceAt(Player, 0);
            if (pieceToThrowOut?.ThrowOut(Player, simulateOnly) ?? true)
            {
               if (!simulateOnly)
                  RelativeFieldIndex = 0;

               return new PieceMoveResult(this, true, 0, pieceToThrowOut);
            }
         }
         return new PieceMoveResult(this, false);
      }
   }
}
