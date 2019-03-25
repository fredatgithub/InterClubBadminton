using System;
using System.Collections.Generic;

namespace InterClubBadminton
{
  internal class OneDayTeam
  {
    public List<Player> Players { get; set; }
    public Player SimpleMan1 { get; set; }
    public Player SimpleMan2 { get; set; }
    public Player SimpleMan3 { get; set; }
    public Player SimpleWoman { get; set; }
    public List<Player> SimpleDoubleMen { get; set; }
    public List<Player> SimpleDoubleWomen { get; set; }
    public List<Player> SimpleDoubleMixed { get; set; }
    public DateTime DateToPlay { get; set; }

    public OneDayTeam()
    {
      Players = new List<Player>();
    }

    public void Add(Player player)
    {
      Players.Add(player);
    }
  }
}