using System;
using System.Collections.Generic;
using UnityEngine;
using RogueLike2D.Battle;
using RogueLike2D.Systems;
using RogueLike2D.Stage;
using RogueLike2D.Characters;
using RogueLike2D.ScriptableObjects;
using RogueLike2D.UI;

namespace RogueLike2D.Core
{
    // Controls overall run flow: stages, battles, bosses, rewards.
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Managers")]
        [SerializeField] private SeasonManager seasonManager;
        [SerializeField] private StageGenerator stageGenerator;
        [SerializeField] private UnlockManager unlockManager;
        [SerializeField] private BattleManager battleManager;

        [Header("Current Run")]
        [SerializeField] private RunData currentRun;

        public enum RunState
        {
            None,
            InRun,
            InBattle,
            InRewards,
            Completed,
            GameOver
        }

        public RunState State { get; private set; } = RunState.None;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoBootstrap()
        {
            Debug.Log("[GameManager] AutoBootstrap invoked after scene load");
            if (Instance != null) { Debug.Log("[GameManager] Instance already exists, skipping AutoBootstrap"); return; }
            var existing = UnityEngine.Object.FindObjectOfType<GameManager>();
            if (existing != null) { Debug.Log("[GameManager] Found existing GameManager in scene, skipping AutoBootstrap"); return; }

            var go = new GameObject("GameManager (Auto)");
            go.AddComponent<GameManager>();
            Debug.Log("[GameManager] Auto-created GameManager GameObject");
        }

        private void Awake()
        {
            Debug.Log("[GameManager] Awake");
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[GameManager] Duplicate instance detected, destroying this GameObject");
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[GameManager] Set as singleton Instance and marked DontDestroyOnLoad");

            // Auto-wire if missing (optional)
            if (!seasonManager) { seasonManager = GetComponentInChildren<SeasonManager>(); Debug.Log($"[GameManager] Auto-wired SeasonManager: {(seasonManager ? "found" : "missing")}"); }
            if (!stageGenerator) { stageGenerator = GetComponentInChildren<StageGenerator>(); Debug.Log($"[GameManager] Auto-wired StageGenerator: {(stageGenerator ? "found" : "missing")}"); }
            if (!unlockManager) { unlockManager = GetComponentInChildren<UnlockManager>(); Debug.Log($"[GameManager] Auto-wired UnlockManager: {(unlockManager ? "found" : "missing")}"); }
            if (!battleManager) { battleManager = GetComponentInChildren<BattleManager>(); Debug.Log($"[GameManager] Auto-wired BattleManager: {(battleManager ? "found" : "missing")}"); }
        }

        private void Start()
        {
            Debug.Log("[GameManager] Start");
            // Ensure file logger initialized and write a baseline marker for startup.
            FileLogger.Initialize();
            FileLogger.EnsureBaselineMarkers("GameManager.Start");
            Debug.Log($"[GameManager] Logging to: {FileLogger.GetLogFilePath()}");
            // Show or create the main menu on play.
            var mainMenu = FindObjectOfType<MainMenuUI>();
            if (mainMenu == null)
            {
                Debug.Log("[GameManager] No MainMenuUI found. Creating one.");
                var go = new GameObject("MainMenuUI");
                mainMenu = go.AddComponent<MainMenuUI>();
            }
            if (mainMenu != null)
            {
                Debug.Log("[GameManager] Showing main menu");
                mainMenu.ShowMain();
                InputModuleBootstrap.EnsureCorrectInputModule(UnityEngine.EventSystems.EventSystem.current, "GameManager.Start (post ShowMain)");
            }
            else
            {
                Debug.LogWarning("[GameManager] Failed to obtain MainMenuUI");
            }

            // TODO: Hook up to main menu to call StartNewRun with selected squad.
        }

        public void StartNewRun(List<CharacterDefinitionSO> chosenSquad)
        {
            Debug.Log($"[GameManager] StartNewRun invoked. Squad size: {(chosenSquad != null ? chosenSquad.Count : 0)}");
            // Create new run data
            int seasonId = seasonManager.GetCurrentSeasonId();
            int seed = seasonManager.GetSeasonSeed(seasonId);
            Debug.Log($"[GameManager] Using SeasonId={seasonId}, Seed={seed}");
            var stages = stageGenerator.GenerateSeasonStages(seed);
            Debug.Log($"[GameManager] Generated stages count: {(stages != null ? stages.Count : 0)}");

            currentRun = new RunData(seasonId, seed, stages, chosenSquad);
            State = RunState.InRun;
            Debug.Log("[GameManager] State set to InRun");

            SaveAndIncrementTotalRuns();

            StartNextEncounter();
        }

        public void StartNextEncounter()
        {
            Debug.Log("[GameManager] StartNextEncounter");
            if (currentRun == null)
            {
                Debug.LogWarning("[GameManager] No current run. StartNewRun first.");
                return;
            }

            BattleType next = currentRun.GetNextEncounterType();
            Debug.Log($"[GameManager] Next encounter type: {next}");
            if (next == BattleType.None)
            {
                State = RunState.Completed;
                Debug.Log("[GameManager] Run completed!");
                SaveWin(true);
                // TODO: Show end-of-run summary UI.
                return;
            }

            State = RunState.InBattle;
            Debug.Log("[GameManager] State set to InBattle");

            // TODO: Build enemy squad for this encounter (placeholder, random).
            var enemySquad = stageGenerator.GenerateEnemiesForEncounter(next, currentRun.Seed, currentRun.StageIndex, currentRun.EncounterIndex);
            Debug.Log($"[GameManager] Enemy squad defs count: {(enemySquad != null ? enemySquad.Count : 0)}");

            // Build player runtime squad from definitions
            var playerRuntimes = CharacterRuntime.BuildRuntimeSquad(currentRun.Squad);
            var enemyRuntimes = CharacterRuntime.BuildRuntimeSquad(enemySquad);
            Debug.Log($"[GameManager] Player runtimes: {playerRuntimes.Count}, Enemy runtimes: {enemyRuntimes.Count}");

            battleManager.BeginBattle(next, playerRuntimes, enemyRuntimes, OnBattleEnded);
            Debug.Log("[GameManager] BattleManager.BeginBattle called");
        }

        private void OnBattleEnded(bool playerWon)
        {
            Debug.Log($"[GameManager] OnBattleEnded. PlayerWon={playerWon}");
            if (playerWon)
            {
                currentRun.AdvanceEncounter();
                Debug.Log($"[GameManager] Advanced encounter. StageIndex={currentRun.StageIndex}, EncounterIndex={currentRun.EncounterIndex}, IsRunComplete={currentRun.IsRunComplete}");
                if (currentRun.IsRunComplete)
                {
                    State = RunState.Completed;
                    SaveWin(true);
                    // TODO: Trigger seasonal hardcore stage unlock if season complete.
                    Debug.Log("[GameManager] Player won the run!");
                    return;
                }

                State = RunState.InRewards;
                Debug.Log("[GameManager] State set to InRewards; granting 1 token as placeholder reward");
                // TODO: Show reward UI and call OnRewardChosen when player picks one.
                // Placeholder: auto-grant 1 token and continue.
                unlockManager.AddTokens(1);
                OnRewardChosen();
            }
            else
            {
                State = RunState.GameOver;
                SaveWin(false);
                Debug.Log("[GameManager] Player lost the run.");
                // TODO: Show game over UI.
            }
        }

        public void OnRewardChosen()
        {
            Debug.Log("[GameManager] OnRewardChosen");
            // TODO: Apply selected reward.
            State = RunState.InRun;
            Debug.Log("[GameManager] State set to InRun");
            StartNextEncounter();
        }

        private void SaveAndIncrementTotalRuns()
        {
            int totalRuns = PlayerPrefs.GetInt("totalRuns", 0);
            PlayerPrefs.SetInt("totalRuns", totalRuns + 1);
            PlayerPrefs.Save();
            Debug.Log($"[GameManager] TotalRuns incremented to {totalRuns + 1}");
        }

        private void SaveWin(bool won)
        {
            int wonRuns = PlayerPrefs.GetInt("wonRuns", 0);
            if (won)
                PlayerPrefs.SetInt("wonRuns", wonRuns + 1);
            PlayerPrefs.Save();
            Debug.Log($"[GameManager] SaveWin called. Won={won}. WonRuns now={PlayerPrefs.GetInt("wonRuns", 0)}");
        }

        public float GetGlobalRunWinRate()
        {
            int totalRuns = PlayerPrefs.GetInt("totalRuns", 0);
            int wonRuns = PlayerPrefs.GetInt("wonRuns", 0);
            if (totalRuns == 0) return 0f;
            return (float)wonRuns / totalRuns;
        }
    }
}
