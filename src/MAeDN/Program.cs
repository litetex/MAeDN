using MAeDN.GameData;
using System;

namespace MAeDN
{
   internal class Program
   {
      static void Main(string[] args)
      {
         new GameManager(new Game(new GameConfig())).Run();
      }
   }
}