# WorkshopSample
Example project for developing a mod for Sentinels of the Multiverse

For more information please visit the Trello board: https://trello.com/b/vYBMImbg/sotm-workshop

This project is a very rough template for starting work on making a mod with new hero, villain, and/or environment decks.

Getting started:

- Get a copy of the project (probably make your own repo or a fork or something)
- If necessary, update the references for EngineCommon and SentinelsEngine to point to the DLLs in your own copy of the game. Double-click on MyMod->MyMod->References, choose the .Net Assembly tab, and browse to the desired files.
  - Windows: C:\Program Files\Steam\steamapps\common\Sentinels of the Multiverse\Sentinels_Data\Managed
  - Mac: /Users/<USERNAME>/Library/Application Support/Steam/steamapps/common/Sentinels of the Multiverse/Sentinels.app/Contents/Resources/Data/Managed
  - Linux: ~/.local/share/Steam/steamapps/common/Sentinels of the Multiverse/Sentinels_Data/Managed
- You should then be be able to compile, run the unit tests, run the console program, etc.  

Making your mod:

- Feel free to rename the project files, etc etc to whatever you like.
- The Default Namespace setting in the main project (MyMod) must be a unique namespace for your mod. It must match the namespace of your code. (Currently only a single string with no dots is supported).
- The namespace of your deck's code (card controllers, etc) should be YourNameSpace.DeckIdentifier
- Update the Setup.cs in the test project to register your own class & namespace with the engine in the DoSetup method.

More documentation and proper template project to come some day!
