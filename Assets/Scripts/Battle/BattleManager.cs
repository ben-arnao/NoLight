using System;
using System.Collections.Generic;
using UnityEngine;
using RogueLike2D.Stage;
using RogueLike2D.Characters;

namespace RogueLike2D.Battle
{
    // Coordinates turn-based battle flow.
    public class BattleManager : MonoBehaviour
    {
        private List<CharacterRuntime> playerTeam;
        private List<CharacterRuntime> enemyTeam;
        private TurnOrderSystem turnSystem;
        private ActionResolver resolver;

        private BattleType battleType;
        private Action<bool> onBattleEnded;

        public event Action<List<CharacterRuntime>> OnTurnOrderUpdated; // Hook for UI
        public event Action OnBattleStarted;
        public event Action OnBattleFinished;

        public void BeginBattle(BattleType type, List<CharacterRuntime> player, List<CharacterRuntime> enemies, Action<bool> onEnded)
        {
            battleType = type;
            onBattleEnded = onEnded;

            playerTeam = player ?? new List<CharacterRuntime>();
            enemyTeam = enemies ?? GenerateDummyEnemies();

            turnSystem = new TurnOrderSystem();
            resolver = new ActionResolver();

            // Seed initial turn order
            turnSystem.Initialize(playerTeam, enemyTeam);

            OnBattleStarted?.Invoke();
            UpdateUIOrder();

            // Auto-run placeholder loop (single pass per character) for skeleton.
            // In a real game, you'd hook this into UI input for player actions.
            StartCoroutine(RunAutoBattleCoroutine());
        }

        private System.Collections.IEnumerator RunAutoBattleCoroutine()
        {
            int safeGuard = 200; // prevent infinite loops
            while (safeGuard-- > 0)
            {
                var next = turnSystem.GetNextActor();
                if (next == null)
                {
                    Debug.Log("No next actor; battle ends as draw (placeholder).");
                    EndBattle(false);
                    yield break;
                }

                next.BeginTurn();

                // Simple AI: attack random opposing target
                bool isPlayer = playerTeam.Contains(next);
                var targets = isPlayer ? enemyTeam : playerTeam;
                var anyAlive = targets.Exists(t => t.IsAlive);
                if (!anyAlive)
                {
                    EndBattle(isPlayer); // last actor's team wins
                    yield break;
                }

                var target = GetRandomAlive(targets);
                resolver.BasicAttack(next, target);
                next.EndTurn();

                // Remove dead characters (optional)
                playerTeam.RemoveAll(c => !c.IsAlive);
                enemyTeam.RemoveAll(c => !c.IsAlive);

                if (playerTeam.Count == 0)
                {
                    EndBattle(false);
                    yield break;
                }
                if (enemyTeam.Count == 0)
                {
                    EndBattle(true);
                    yield break;
                }

                turnSystem.UpdateOrder(playerTeam, enemyTeam);
                UpdateUIOrder();

                yield return new WaitForSeconds(0.2f); // slow down auto loop
            }

            // Fallback
            EndBattle(false);
        }

        private CharacterRuntime GetRandomAlive(List<CharacterRuntime> list)
        {
            var alive = list.FindAll(c => c.IsAlive);
            if (alive.Count == 0) return null;
            int idx = UnityEngine.Random.Range(0, alive.Count);
            return alive[idx];
        }

        private void UpdateUIOrder()
        {
            OnTurnOrderUpdated?.Invoke(turnSystem.PeekUpcomingOrder());
        }

        private void EndBattle(bool playerWon)
        {
            StopAllCoroutines();
            OnBattleFinished?.Invoke();
            onBattleEnded?.Invoke(playerWon);
        }

        private List<CharacterRuntime> GenerateDummyEnemies()
        {
            // Placeholder: empty enemy list to avoid nulls.
            return new List<CharacterRuntime>();
        }
    }
}
