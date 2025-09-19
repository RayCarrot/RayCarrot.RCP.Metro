#nullable disable
namespace RayCarrot.RCP.Metro;

public enum GAM_EngineMode : byte
{
    Invalid = 0,
    StartingProgram = 1,                  // first init
    StoppingProgram = 2,                  // last desinit
    EnterGame = 3,                        // init game loop
    QuitGame = 4,                         // desinit game loop
    EnterLevel = 5,                       // init level loop
    ChangeLevel = 6,                      // desinit level loop
    DeadLoop = 7,                         // init dead loop
    PlayerDead = 8,                       // desinit dead loop
    Playing = 9,                          // playing game

    // Not in R2
    EnterMenu = 10,                        // Init the menu
    Menu = 11,                             // In the start menu
    QuitMenuToEnterGame = 12,              // Go to the game from the menu
    EnterMenuWhenPlaying = 13,             // Init the menu when playing
    EnterMenuWhenPlayingWithoutPause = 14, // Init the menu by AI
    MenuWhenPlaying = 15,                  // In the menu when playing
    MenuWhenPlayingWithoutPause = 16,      // In the menu when playing without pause
    ResumeGameFromMenu = 17,               // Return to the game from the menu
}