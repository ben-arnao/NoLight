# RogueLike2D Prototype - Local Development
Author: Julia

This Unity project is a small turn-based roguelike prototype. Follow these steps to open and run it locally and see the Main Menu on Play.

Prerequisites
- Unity Hub installed.
- Unity Editor version as specified by ProjectSettings/ProjectVersion.txt (Unity Hub will select it automatically when opening).
- Optional: Git LFS installed if your repo uses large assets.

First-time setup
1) Clone the repository.
2) Open Unity Hub, click “Open” (or “Add”), and select the repository root (the folder containing Assets/, ProjectSettings/, Packages/).
3) Let Unity import the project with the version indicated by ProjectVersion.txt.

Run in Editor
1) Ensure the initial scene is configured:
   - Open File > Build Settings...
   - Under “Scenes In Build,” add your initial scene (the one containing the Main Menu Canvas) and move it to index 0.
   - Open that scene in the Editor by double-clicking it.
2) Press Play.
   - You should see the Main Menu. The UI controller now forces the Main Menu to be shown at startup.
3) Start a run (when ready):
   - The GameManager coordinates run flow. Hook a UI button to GameManager.StartNewRun(...) with your chosen squad when that UI is implemented.

Scene requirements
- A Camera in the scene (tagged MainCamera).
- An EventSystem GameObject so UI can receive input.
- A Canvas with a MainMenuUI component, with references set to:
  - mainPanel (the Main Menu root panel)
  - rosterPanel (the roster selection panel)
  - collectionPanel (the collection panel)

Troubleshooting a blank screen
- Verify the initial scene is open and added at index 0 in Build Settings.
- Ensure there is a Camera and an EventSystem in the scene.
- Select the Canvas with MainMenuUI and confirm:
  - The script reference is not missing.
  - mainPanel, rosterPanel, and collectionPanel fields are assigned.
- Check the Console for errors related to missing references or components.

Notes for contributors
- Namespaces: RogueLike2D.Core, RogueLike2D.UI, RogueLike2D.Battle, etc.
- GameManager is a DontDestroyOnLoad singleton. It now tries to show the main menu on Start if a MainMenuUI exists in the scene.
- MainMenuUI shows the main panel automatically on Start to avoid blank screens.

Debug logs
- Location:
  - Unity Editor: <project_root>/Logs/debug.log
  - Player builds: <persistentDataPath>/Logs/debug.log
    - On Windows this is typically C:\Users\<YourUser>\AppData\LocalLow\<CompanyName>\<ProductName>\Logs\debug.log
- Behavior:
  - Logs are cleared on each app start (fresh log per session).
  - All Unity Console messages are mirrored to this file with timestamps and frame counts; errors include stack traces.
  - Startup, menu initialization, and UI interactions (Start Run, View Collection, Exit) are explicitly logged.
- Troubleshooting:
  - If the main menu appears unresponsive, click "Exit" and check the log for lines starting with [MainMenuUI] and [GameManager].
  - Confirm that "Application.Quit()" or "EditorApplication.isPlaying = false" was invoked when pressing Exit.
