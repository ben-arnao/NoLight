using System;
using System.Collections.Generic;
using UnityEngine;
using RogueLike2D.Stage;
using RogueLike2D.Characters;
using RogueLike2D.ScriptableObjects;

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

        private CharacterRuntime currentActor;
        private AbilitySO pendingAbility;
        private CharacterRuntime pendingTarget;
        private bool awaitingPlayerInput = false;
        private int turnCounter = 0;

        public event Action<List<CharacterRuntime>> OnTurnOrderUpdated; // Hook for UI
        public event Action OnBattleStarted;
        public event Action<bool> OnBattleFinished;

        // UI prompts
        public event Action<CharacterRuntime, List<AbilitySO>, List<CharacterRuntime>, List<CharacterRuntime>> OnPlayerActionPrompt;
        public event Action<AbilitySO, List<CharacterRuntime>> OnTargetPrompt;
        public event Action<int, CharacterRuntime, List<CharacterRuntime>> OnTurnCounterUpdated;

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

            StartCoroutine(RunBattleLoop());
        }

        private System.Collections.IEnumerator RunBattleLoop()
        {
            int safeGuard = 1000; // prevent infinite loops
            while (safeGuard-- > 0)
            {
                currentActor = turnSystem.GetNextActor();
                if (currentActor == null)
                {
                    Debug.Log("No next actor; battle ends as draw (placeholder).");
                    EndBattle(false);
                    yield break;
                }

                turnCounter++;
                currentActor.BeginTurn();

                // Early victory check for opposing team dead (could happen from start-of-turn effects)
                if (enemyTeam.TrueForAll(e => e == null || !e.IsAlive))
                {
                    EndBattle(true);
                    yield break;
                }
                if (playerTeam.TrueForAll(p => p == null || !p.IsAlive))
                {
                    EndBattle(false);
                    yield break;
                }

                bool isPlayer = playerTeam.Contains(currentActor);
                OnTurnCounterUpdated?.Invoke(turnCounter, currentActor, turnSystem.PeekUpcomingOrder());

                if (isPlayer)
                {
                    awaitingPlayerInput = true;
                    pendingAbility = null;
                    pendingTarget = null;

                    // Present abilities and targets
                    OnPlayerActionPrompt?.Invoke(currentActor, currentActor.EquippedAbilities, enemyTeam, playerTeam);

                    // Wait for ability selection
                    yield return new WaitUntil(() => !awaitingPlayerInput || pendingAbility != null);

                    if (!awaitingPlayerInput)
                    {
                        // Battle externally ended
                        yield break;
                    }

                    // Validate cooldown
                    if (pendingAbility == null || currentActor.IsOnCooldown(pendingAbility.Id))
                    {
                        // skip turn on invalid selection
                        Debug.LogWarning("Invalid or cooldown ability selection; skipping action.");
                    }
                    else
                    {
                        // If ability needs a target, ensure we have one
                        if (pendingAbility.Targeting == AbilityTargeting.SingleEnemy || pendingAbility.Targeting == AbilityTargeting.SingleAlly)
                        {
                            // If no target selected yet, ask now
                            if (pendingTarget == null)
                            {
                                var candidates = pendingAbility.Targeting == AbilityTargeting.SingleEnemy
                                    ? enemyTeam.FindAll(c => c != null && c.IsAlive)
                                    : playerTeam.FindAll(c => c != null && c.IsAlive);

                                OnTargetPrompt?.Invoke(pendingAbility, candidates);
                                yield return new WaitUntil(() => !awaitingPlayerInput || pendingTarget != null);
                            }
                        }
                        else if (pendingAbility.Targeting == AbilityTargeting.Self)
                        {
                            pendingTarget = currentActor;
                        }

                        // Execute
                        var allies = isPlayer ? playerTeam : enemyTeam;
                        var enemies = isPlayer ? enemyTeam : playerTeam;
                        resolver.ResolveAbility(pendingAbility, currentActor, allies, enemies, pendingTarget);
                    }

                    // Clear selection
                    pendingAbility = null;
                    pendingTarget = null;
                    awaitingPlayerInput = false;
                }
                else
                {
                    // Enemy AI
                    resolver.DemonAct(currentActor, enemyTeam, playerTeam);
                    yield return new WaitForSeconds(0.2f);
                }

                currentActor.EndTurn();

                // Remove dead characters (optional)
                playerTeam.RemoveAll(c => c == null || !c.IsAlive);
                enemyTeam.RemoveAll(c => c == null || !c.IsAlive);

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

                yield return null;
            }

            // Fallback
            EndBattle(false);
        }

        public void TryChooseAbility(AbilitySO ability)
        {
            if (!awaitingPlayerInput || currentActor == null) return;
            if (ability == null) return;
            if (currentActor.IsOnCooldown(ability.Id)) return;

            pendingAbility = ability;

            // If target is required, prompt immediately
            if (ability.Targeting == AbilityTargeting.SingleEnemy || ability.Targeting == AbilityTargeting.SingleAlly)
            {
                var candidates = ability.Targeting == AbilityTargeting.SingleEnemy
                    ? enemyTeam.FindAll(c => c != null && c.IsAlive)
                    : playerTeam.FindAll(c => c != null && c.IsAlive);

                OnTargetPrompt?.Invoke(ability, candidates);
            }
            else if (ability.Targeting == AbilityTargeting.Self)
            {
                pendingTarget = currentActor;
            }
        }

        public void TryChooseTarget(CharacterRuntime target)
        {
            if (!awaitingPlayerInput || currentActor == null || pendingAbility == null) return;
            if (target == null || !target.IsAlive) return;

            pendingTarget = target;
        }

        private CharacterRuntime GetRandomAlive(List<CharacterRuntime> list)
        {
            var alive = list.FindAll(c => c != null && c.IsAlive);
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
            OnBattleFinished?.Invoke(playerWon);
            onBattleEnded?.Invoke(playerWon);
        }

        private List<CharacterRuntime> GenerateDummyEnemies()
        {
            // Placeholder: empty enemy list to avoid nulls.
            return new List<CharacterRuntime>();
        }
    }
}
