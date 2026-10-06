# InterClubBadminton

A Windows desktop application for creating and managing badminton teams.

## Features

- Add players with their gender and singles, doubles, and mixed-doubles levels.
- Create teams and assemble one-day teams from the available players.
- Load ranking levels and interface translations from XML files.

## Requirements

- Windows
- .NET Framework 4.8
- Visual Studio with the .NET Framework 4.8 targeting pack

## Build and run

1. Open `InterClubBadminton.sln` in Visual Studio.
2. Restore the NuGet packages if prompted.
3. Build the solution, then run the `InterClubBadminton` project.

The ranking data and translations are in `InterClubBadminton/Resources/Points.xml`
and `InterClubBadminton/Translations.xml`. `InterClubBadminton/Resources/Template-Players.xml`
contains an example player record.

## License
[![License status](https://img.shields.io/badge/License-MIT%20License-blue.svg)](https://github.com/fredatgithub/InterClubBadminton#license)
