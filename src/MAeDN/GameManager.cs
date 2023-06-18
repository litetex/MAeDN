using MAeDN.GameData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAeDN
{
   public class GameManager
   {
      static readonly ConsoleColor[] AVAILABLE_COLORS =
      {
         ConsoleColor.Blue,
         ConsoleColor.Green,
         ConsoleColor.Red,
         ConsoleColor.Yellow,
         ConsoleColor.Cyan,
         ConsoleColor.White
      };

      Game Game { get; init; }

      Dictionary<Player, ConsoleColor> PlayerColors { get; init; }

      public GameManager(Game game)
      {
         Game = game;
         PlayerColors = Game.Players
            .ToDictionary(
               p => p,
               p => AVAILABLE_COLORS[Game.Players.IndexOf(p)]);
      }

      public void Run()
      {
         int turn = 1;
         var currentPlayer = Game.CurrentPlayer;
         do
         {
            DisplayCurrentGameStateOnTurn(turn);

            if(currentPlayer.State == PlayerState.HOME)
            {
               TryMoveOutOfHome(currentPlayer);
            }

            int lastRoll = -1;
            do
            {
               if (lastRoll != -1)
                  DisplayCurrentGameState();

               var potentialMoveablePieces = currentPlayer.MoveablePieces;
               if (potentialMoveablePieces.Any())
               {
                  lastRoll = RollAndTryMovePiece(currentPlayer, potentialMoveablePieces);
               }
            } while (lastRoll == Game.Config.CanRollDiceAgainDiceRoll);

            if(currentPlayer.State == PlayerState.FINISHED)
            {
               Console.Write("Congratulations ");
               ConsoleWritePlayerColor(currentPlayer, currentPlayer.Name);
               Console.Write($" you finished as {Game.Players.Count(p => p.State == PlayerState.FINISHED)}");
            }

            currentPlayer = Game.NextPlayer();
            turn++;

            Console.WriteLine();

         } while (currentPlayer != null);
      }

      private void TryMoveOutOfHome(Player currentPlayer)
      {
         for (int i = 0; i < Game.Config.CountOfGetOutOfHomeDiceRolls; i++)
         {
            int roll = Game.RollDice();
            Console.WriteLine($"Trying to get out of home ({i + 1}/{Game.Config.CountOfGetOutOfHomeDiceRolls})... Rolled a {roll}");

            if (roll >= Game.Config.MinGetOutOfHomeDiceRoll)
            {
               currentPlayer.PiecesAtHome.First()
                  .Move(roll, false);
               break;
            }
         }
      }

      private int RollAndTryMovePiece(Player currentPlayer, HashSet<Piece> potentialMoveablePieces)
      {
         var roll = currentPlayer.Game.RollDice();

         Console.WriteLine("Rolled a " + roll);

         var moveablePieces = potentialMoveablePieces
            .Select(p => p.Move(roll, true))
            .Where(r => r.Moved)
            .OrderByDescending(r => r.Piece.RelativeFieldIndexSafe)
            .ToList();

         var hasMoveablePiecesOnField = moveablePieces.Count > 0;

         var canMoveOutOfHouse = roll >= currentPlayer.Game.Config.MinGetOutOfHomeDiceRoll
            && currentPlayer.PiecesAtHome.Count > 0
            && currentPlayer.PiecesAtHome.First().Move(roll, true).Moved;

         if (canMoveOutOfHouse && hasMoveablePiecesOnField)
         {
            Console.WriteLine($"You could move one of {moveablePieces.Count}x pieces. Or would you like to move a piece out of the house? (y/n)");

            string choice = "";
            while(!new string[] { "y", "n" }.Contains(choice))
            {
               choice = Console.ReadLine()?.ToLower() ?? "";
            }
            if("y".Equals(choice))
            {
               MovePieceOutOfHouse(currentPlayer, roll);
               return roll;
            }
         }
         else if(canMoveOutOfHouse)
         {
            Console.WriteLine("No piece on field can be moved but a piece can be moved out of the house");
            MovePieceOutOfHouse(currentPlayer, roll);
            return roll;
         }
         else if (!hasMoveablePiecesOnField)
         {
            Console.WriteLine("No piece can be moved");
            return roll;
         }

         Piece pieceToMove =
            moveablePieces.Count == 1
            ? moveablePieces.First().Piece
            : SelectPieceToMove(moveablePieces);

         pieceToMove.Move(roll, false);

         return roll;
      }

      private Piece SelectPieceToMove(List<PieceMoveResult> options)
      {
         Console.WriteLine("The following pieces can be moved:");
         foreach(PieceMoveResult option in options)
         {
            var piece = option.Piece;
            var fieldsToFinish = piece.Player.Game.TotalNumberOfFields - piece.RelativeFieldIndexSafe;
            ConsoleWritePlayerColor(piece.Player, piece.Identifier);
            Console.Write(" | ");
            if (fieldsToFinish > 0)
            {
               Console.Write($"{fieldsToFinish,2} fields to finish");
            }
            else
            {
               Console.Write("Inside finish");
            }

            var thrownOut = option.ThrownOut;
            if (thrownOut != null)
            {
               Console.Write(" | Would throw out ");
               ConsoleWritePlayerColor(thrownOut.Player, $"{thrownOut.Identifier} of {thrownOut.Player.Name}");
            }

            Console.WriteLine();
         }

         Console.WriteLine("Please select piece to move");

         var validSelectOptions = options.Select(o => o.Piece.Identifier).ToHashSet();

         string selected = "";
         while (!validSelectOptions.Contains(selected))
         {
            selected = Console.ReadLine()?.ToUpper() ?? "";
         }

         return options
            .Select(o => o.Piece)
            .First(p => p.Identifier.Equals(selected));
      }

      private void MovePieceOutOfHouse(Player currentPlayer, int roll)
      {
         currentPlayer.PiecesAtHome.First().Move(roll, false);
      }

      private void DisplayCurrentGameStateOnTurn(int turn)
      {
         Console.Write("======== ");
         ConsoleWritePlayerColor(Game.CurrentPlayer, $"TURN {turn}");
         Console.WriteLine(" ========");

         Console.Write("Currently playing: ");
         ConsoleWritePlayerColor(Game.CurrentPlayer, Game.CurrentPlayer.Name);
         Console.WriteLine();

         DisplayCurrentGameState();
      }

      private void DisplayCurrentGameState()
      {
         var allPiecesOnFields = Game.Players
            .SelectMany(p => p.Pieces)
            .Where(p => p.IsOnFields)
            .ToDictionary(p => p.TotalFieldIndexSafe);

         for (int i = 0; i < Game.Config.GameSize; i++)
         {
            var player = Game.Players.ElementAtOrDefault(i);

            if(player != null)
            {
               var piecesAtHome = player.Pieces.Where(p => p.IsHome);
               ConsoleWritePlayerColor(player, new String('_', Game.Config.PiecesPerPlayer - piecesAtHome.Count()));
               ConsoleWritePlayerColor(player, string.Join("", piecesAtHome.Select(p => p.Identifier).OrderBy(p => p).Select(i => i.ToString())));
            }
            else
            {
               Console.Write(new String(' ', Game.Config.PiecesPerPlayer));
            }

            Console.Write(" | ");

            var startIndex = i * Game.Config.FieldsPerPlayer;
            var endIndexExclusive = (i + 1) * Game.Config.FieldsPerPlayer;

            for (int j = startIndex; j < endIndexExclusive; j++)
            {
               if (allPiecesOnFields.TryGetValue(j, out var piece))
               {
                  ConsoleWritePlayerColor(piece.Player, piece.Identifier.ToString());
                  continue;
               }

               Console.Write("_");
            }

            Console.Write(" | ");

            var finishPlayer = Game.Players
               .ElementAtOrDefault((i + 1 + Game.Config.GameSize) % Game.Config.GameSize);
            var piecesOfFinishPlayer = finishPlayer?.Pieces
               .Where(p => p.IsFinished)
               .ToDictionary(p => p.RelativeFieldIndexSafe - Game.TotalNumberOfFields);

            for (int j = 0; j < Game.Config.PiecesPerPlayer; j++)
            {
               if (piecesOfFinishPlayer?.TryGetValue(j, out var piece) ?? false)
               {
                  ConsoleWritePlayerColor(piece.Player, piece.Identifier.ToString());
                  continue;
               }

               if (finishPlayer != null)
                  ConsoleWritePlayerColor(finishPlayer, "_");
               else
                  Console.Write(" ");
            }

            Console.WriteLine();
         }
      }

      private void ConsoleWritePlayerColor(Player player, String text)
      {
         var prevColor = Console.ForegroundColor;
         Console.ForegroundColor = PlayerColors[player];

         Console.Write(text);

         Console.ForegroundColor = prevColor;
      }
   }
}
