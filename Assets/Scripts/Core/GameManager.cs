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

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Auto-wire if missing (optional)
            if (!seasonManager) seasonManager = GetComponentInChildren<SeasonManager>();
            if (!stageGenerator) stageGenerator = GetComponentInChildren<StageGenerator>();
            if (!unlockManager) unlockManager = GetComponentInChildren<UnlockManager>();
            if (!battleManager) battleManager = GetComponentInChildren<BattleManager>();
        }

        private void Start()
        {
            // Show the main menu on play (if present in scene).
            var mainMenu = FindObjectOfType<MainMenuUI>();
            if (mainMenu != null)
            {
                mainMenu.ShowMain();
            }

            // TODO: Hook up to main menu to call StartNewRun with selected squad.
        }

        public void StartNewRun(List<CharacterDefinitionSO> chosenSquad)
        {
            // Create new run data
            int seasonId = seasonManager.GetCurrentSeasonId();
            int seed = seasonManager.GetSeasonSeed(seasonId);
            var stages = stageGenerator.GenerateSeasonStages(seed);

            currentRun = new RunData(seasonId, seed, stages, chosenSquad);
            State = RunState.InRun;

            SaveAndIncrementTotalRuns();

            StartNextEncounter();
        }

        public void StartNextEncounter()
        {
            if (currentRun == null)
            {
                Debug.LogWarning("No current run. StartNewRun first.");
                return;
            }

            BattleType next = currentRun.GetNextEncounterType();
            if (next == BattleType.None)
            {
                State = RunState.Completed;
                Debug.Log("Run completed!");
                SaveWin(true);
                // TODO: Show end-of-run summary UI.
                return;
            }

            State = RunState.InBattle;

            // TODO: Build enemy squad for this encounter (placeholder, random).
            var enemySquad = stageGenerator.GenerateEnemiesForEncounter(next, currentRun.Seed, currentRun.StageIndex, currentRun.EncounterIndex);

            // Build player runtime squad from definitions
            var playerRuntimes = CharacterRuntime.BuildRuntimeSquad(currentRun.Squad);
            var enemyRuntimes = CharacterRuntime.BuildRuntimeSquad(enemySquad);

            battleManager.BeginBattle(next, playerRuntimes, enemyRuntimes, OnBattleEnded);
        }

        private void OnBattleEnded(bool playerWon)
        {
            if (playerWon)
            {
                currentRun.AdvanceEncounter();
                if (currentRun.IsRunComplete)
                {
                    State = RunState.Completed;
                    SaveWin(true);
                    // TODO: Trigger seasonal hardcore stage unlock if season complete.
                    Debug.Log("Player won the run!");
                    return;
                }

                State = RunState.InRewards;
                // TODO: Show reward UI and call OnRewardChosen when player picks one.
                // Placeholder: auto-grant 1 token and continue.
                unlockManager.AddTokens(1);
                OnRewardChosen();
            }
            else
            {
                State = RunState.GameOver;
                SaveWin(false);
                Debug.Log("Player lost the run.");
                // TODO: Show game over UI.
            }
        }

        public void OnRewardChosen()
        {
            // TODO: Apply selected reward.
            State = RunState.InRun;
            StartNextEncounter();
        }

        private void SaveAndIncrementTotalRuns()
        {
            int totalRuns = PlayerPrefs.GetInt("totalRuns", 0);
            PlayerPrefs.SetInt("totalRuns", totalRuns + 1);
            PlayerPrefs.Save();
        }

        private void SaveWin(bool won)
        {
            int wonRuns = PlayerPrefs.GetInt("wonRuns", 0);
            if (won)
                PlayerPrefs.SetInt("wonRuns", wonRuns + 1);
            PlayerPrefs.Save();
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
