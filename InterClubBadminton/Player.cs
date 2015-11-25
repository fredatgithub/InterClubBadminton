using System.Collections.Generic;

namespace InterClubBadminton
{
  internal class Player
  {
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public Gender SexGender { get; set; }
    public PlayLevel SimpleLevel { get; set; }
    public PlayLevel DoubleLevel { get; set; }
    public PlayLevel MixedLevel { get; set; }
    public List<TypePlayer> WishList { get; set; }
    public List<TypePlayer> RefusalList { get; set; }

    public Player(string firstName = "no first name", string lastName = "no last name",
      Gender sexGender = Gender.Male, PlayLevel simpleLevel = PlayLevel.NC,
      PlayLevel doubleLevel = PlayLevel.NC, PlayLevel mixedLevel = PlayLevel.NC)
    {
      FirstName = firstName;
      LastName = lastName;
      SexGender = sexGender;
      SimpleLevel = simpleLevel;
      DoubleLevel = doubleLevel;
      MixedLevel = mixedLevel;
      WishList = new List<TypePlayer>();
      RefusalList = new List<TypePlayer>();
    }
  }
}