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
  - A baseline line is guaranteed to be written on startup and when the Main Menu is shown:
    - [BASELINE] Application alive - GameManager.Start
    - [BASELINE] Application alive - MainMenuUI.ShowMain
  - All Unity Console messages are mirrored to this file with timestamps and frame counts; errors include stack traces.
  - Startup, menu initialization, and UI interactions (Start Run, View Collection, Exit) are explicitly logged.
  - The log target path is printed at startup (e.g., "[GameManager] Logging to: <path>").
- Troubleshooting:
  - If the main menu appears unresponsive, click "Exit" and check the log for lines starting with [MainMenuUI] and [GameManager].
  - Confirm that "Application.Quit()" or "EditorApplication.isPlaying = false" was invoked when pressing Exit.

Running tests
- Open Unity Editor and go to Window > General > Test Runner.
- Select the EditMode tab and run FileLoggerTests. You should see the test pass and a debug.log containing a [BASELINE] line.

Troubleshooting: UI input freezes or Exit button does nothing
- Symptom: Console spam like
  - InvalidOperationException: You are trying to read Input using the UnityEngine.Input class, but you have switched active Input handling to Input System package in Player Settings.
- Cause: The scene uses the legacy StandaloneInputModule while Project Settings > Player > Active Input Handling is set to "Input System Package (New)".
- Fix applied by this repo:
  - Assets/Editor/EnsureInputSettings.cs automatically sets Active Input Handling to "Both" when the Editor opens. This makes the legacy StandaloneInputModule work and unfreezes the UI.
  - At runtime, Assets/Scripts/UI/InputModuleBootstrap.cs ensures the correct EventSystem input module based on active input handling (adds InputSystemUIInputModule when only the new Input System is enabled; otherwise ensures StandaloneInputModule). If the new Input System module has no actions asset assigned, a default set is now provided so the UI can respond to input immediately.
  - You can also run it manually via menu: Tools > Roguelike2D > Fix Input Handling (Set to Both).
- If you still see issues:
  - Ensure your scene has an EventSystem GameObject with a StandaloneInputModule component (or migrate to InputSystemUIInputModule and provide a UI actions asset).
  - Close and reopen the project to ensure the setting was applied.

CI/command line builds (no CLI override for Active Input Handling)
- There is no CLI switch to override Active Input Handling during a build; it comes from Project Settings and determines compiler defines (ENABLE_INPUT_SYSTEM, ENABLE_LEGACY_INPUT_MANAGER).
- Windows quick build: run Tools\build_windows.bat. It invokes RogueLike2D.Editor.BuildScript.PerformWindowsBuild and writes build_log.txt to the repo root.
- This repo includes a pre-build guard script (Assets/Editor/InputHandlingGuard.cs) that warns if the setting and your runtime UI path are likely mismatched. To make it fail the build on mismatch, add the scripting define symbol ROGUELIKE2D_FAIL_ON_INPUT_MISMATCH in Project Settings > Player > Other Settings > Scripting Define Symbols for your target.
- Runtime EventSystem module selection:
  - If ONLY the new Input System is enabled, use InputSystemUIInputModule. The included InputModuleBootstrap ensures this automatically at runtime and, if no actions asset is assigned, generates one via `InputSystemUIInputModule.CreateDefaultActions()` so the UI can respond immediately.
  - Otherwise (Both or Old), StandaloneInputModule will be ensured.
