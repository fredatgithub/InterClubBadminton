using System.Collections.Generic;

namespace InterClubBadminton
{
  internal class Team
  {
    public string Name { get; set; }
    public List<Player> ListOfPlayers { get; set; }

    public Team(string name = "untitled name")
    {
      Name = name;
      ListOfPlayers = new List<Player>();
    }
  }
}