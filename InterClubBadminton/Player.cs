using System.Collections.Generic;
using Tools;

namespace InterClubBadminton
{
  internal class Player
  {
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public Gender SexGender { get; set; }
    public RankLevel SimpleLevel { get; set; }
    public RankLevel DoubleLevel { get; set; }
    public RankLevel MixedLevel { get; set; }
    public List<TypePlayer> WishList { get; set; }
    public List<TypePlayer> RefusalList { get; set; }
    public int LicenseNumber { get; set; }

    public Player(string firstName = "no first name", string lastName = "no last name",
      Gender sexGender = Gender.Male, RankLevel simpleLevel = RankLevel.NC,
      RankLevel doubleLevel = RankLevel.NC, RankLevel mixedLevel = RankLevel.NC,
      int licenseNumber = 0)
    {
      FirstName = firstName;
      LastName = lastName;
      SexGender = sexGender;
      SimpleLevel = simpleLevel;
      DoubleLevel = doubleLevel;
      MixedLevel = mixedLevel;
      WishList = new List<TypePlayer>();
      RefusalList = new List<TypePlayer>();
      LicenseNumber = licenseNumber;
    }
    
    public override string ToString()
    {
      return FirstName + Punctuation.OneSpace + LastName;
    }
  }
}