using System.Collections.Generic;
using System.Linq;
using RogueLike2D.Characters;

namespace RogueLike2D.Battle
{
    // Simple speed-based turn queue: higher speed acts earlier, ties by order.
    public class TurnOrderSystem
    {
        private readonly List<CharacterRuntime> order = new List<CharacterRuntime>();

        public void Initialize(List<CharacterRuntime> playerTeam, List<CharacterRuntime> enemyTeam)
        {
            order.Clear();
            UpdateOrder(playerTeam, enemyTeam);
        }

        public void UpdateOrder(List<CharacterRuntime> playerTeam, List<CharacterRuntime> enemyTeam)
        {
            order.Clear();
            var all = new List<CharacterRuntime>();
            if (playerTeam != null) all.AddRange(playerTeam);
            if (enemyTeam != null) all.AddRange(enemyTeam);

            // Sort by Speed descending
            order.AddRange(all.OrderByDescending(c => c.Stats.Speed));
        }

        public CharacterRuntime GetNextActor()
        {
            if (order.Count == 0) return null;
            var next = order[0];
            // rotate to end
            order.RemoveAt(0);
            order.Add(next);
            return next;
        }

        public List<CharacterRuntime> PeekUpcomingOrder()
        {
            return new List<CharacterRuntime>(order);
        }
    }
}
