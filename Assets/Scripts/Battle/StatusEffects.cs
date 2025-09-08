using System.Collections.Generic;
using UnityEngine;

namespace RogueLike2D.Battle
{
    public enum StatusType
    {
        Poison,
        Stun,
        Bleed,
        Burn,
        Freeze
    }

    // Base status effect placeholder
    [System.Serializable]
    public class StatusEffect
    {
        public StatusType Type;
        public int Duration; // in turns
        public int Stacks;

        public StatusEffect(StatusType type, int duration, int stacks = 1)
        {
            Type = type;
            Duration = duration;
            Stacks = stacks;
        }
    }

    // Container attached at runtime to an actor (skeleton placeholder).
    public class StatusContainer
    {
        private readonly List<StatusEffect> statuses = new List<StatusEffect>();

        public void Add(StatusEffect effect)
        {
            // TODO: merge stacks if same type
            statuses.Add(effect);
        }

        public void TickStart()
        {
            // TODO: Apply ongoing start-of-turn effects (e.g., stun blocking action)
            Decay();
        }

        public void TickEnd()
        {
            // TODO: Apply end-of-turn effects (e.g., poison damage)
            Decay();
        }

        private void Decay()
        {
            for (int i = statuses.Count - 1; i >= 0; i--)
            {
                statuses[i].Duration--;
                if (statuses[i].Duration <= 0)
                    statuses.RemoveAt(i);
            }
        }
    }
}
