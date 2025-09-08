using System;
using System.Collections.Generic;

namespace RogueLike2D.Utils
{
    // Utility for weighted random choices (placeholder).
    public static class WeightedRandom
    {
        public static T Choose<T>(List<(T item, float weight)> items, Random rng)
        {
            float total = 0f;
            foreach (var i in items) total += i.weight;
            if (total <= 0f) return items[0].item;

            float r = (float)(rng.NextDouble() * total);
            foreach (var i in items)
            {
                if (r < i.weight) return i.item;
                r -= i.weight;
            }
            return items[items.Count - 1].item;
        }
    }
}
