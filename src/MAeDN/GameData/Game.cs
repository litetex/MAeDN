using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace MAeDN.GameData
{
   public class Game
   {
      public GameConfig Config { get; init; }

      public int CurrentPlayerIndex { get; private set; } = 0;

      public List<Player> Players { get; private set; } = new List<Player>();


      public Player CurrentPlayer { get { return Players[CurrentPlayerIndex]; } }

      public int TotalNumberOfFields { get { return Config.GameSize * Config.FieldsPerPlayer; } }

      public int TotalNumberOfFieldsWithFinish { get { return TotalNumberOfFields + Config.PiecesPerPlayer; } }

      public bool IsGameFinished { get { return Players.Count(p => p.State != PlayerState.FINISHED) < 2; } }

      public Game(GameConfig config)
      {
         Config = config;
         Players = Config.PlayerNames.Select(name => new Player(this, name)).ToList();

         // Randomize initial player
         CurrentPlayerIndex = Config.Random.Next(Players.Count);
      }

      public Player? NextPlayer()
      {
         if(IsGameFinished)
         {
            return null;
         }

         do
         {
            CurrentPlayerIndex = (CurrentPlayerIndex + 1) % Config.PlayerCount;
         } while (CurrentPlayer.State == PlayerState.FINISHED);

         return CurrentPlayer;
      }

      public Piece? GetPieceAt(Player callingPlayer, int relativeFieldIndex)
      {
         // If would be finished Check only self
         if(relativeFieldIndex >= TotalNumberOfFields)
         {
            return callingPlayer.Pieces
               .Where(p => p.IsFinished)
               .FirstOrDefault(p => p.RelativeFieldIndex == relativeFieldIndex);
         }

         var totalFieldIndex = callingPlayer.TotalFieldIndex(relativeFieldIndex);
         return Players.SelectMany(p => p.Pieces)
            .FirstOrDefault(p => p.TotalFieldIndex == totalFieldIndex);
      }

      public int RollDice()
      {
         return Config.DiceRolls.ElementAt(Config.Random.Next(Config.DiceRolls.Count));
      }
   }
}
