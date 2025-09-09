using System;
using System.Collections.Generic;
using UnityEngine;
using RogueLike2D.Core;

namespace RogueLike2D.Systems
{
    // Handles season boundaries, seeds, and archive.
    public class SeasonManager : MonoBehaviour
    {
        private const string SeasonSeedKeyPrefix = "seasonSeed_";
        private const string SeasonArchiveKey = "seasonArchive";

        public int GetCurrentSeasonId()
        {
            // Placeholder: one season per calendar month. Format YYYYMM (e.g., 202509)
            DateTime now = DateTime.UtcNow;
            return now.Year * 100 + now.Month;
        }

        public int GetSeasonSeed(int seasonId)
        {
            string key = SeasonSeedKeyPrefix + seasonId;
            int existing = PlayerPrefs.GetInt(key, int.MinValue);
            if (existing != int.MinValue)
            {
                return existing;
            }

            // Generate deterministic seed from seasonId
            int seed = seasonId * 73856093 ^ unchecked((int)0x9E3779B9);
            PlayerPrefs.SetInt(key, seed);
            ArchiveSeason(seasonId, seed);
            PlayerPrefs.Save();
            return seed;
        }

        public Dictionary<int, int> GetArchivedSeasons()
        {
            // Stored as comma-separated: season:seed;season:seed
            string raw = PlayerPrefs.GetString(SeasonArchiveKey, "");
            var dict = new Dictionary<int, int>();
            if (string.IsNullOrEmpty(raw)) return dict;

            string[] pairs = raw.Split(';');
            foreach (var p in pairs)
            {
                if (string.IsNullOrWhiteSpace(p)) continue;
                var kv = p.Split(':');
                if (kv.Length == 2 && int.TryParse(kv[0], out int s) && int.TryParse(kv[1], out int seed))
                {
                    dict[s] = seed;
                }
            }
            return dict;
        }

        private void ArchiveSeason(int seasonId, int seed)
        {
            var archive = GetArchivedSeasons();
            archive[seasonId] = seed;
            // serialize back
            var parts = new List<string>();
            foreach (var kv in archive)
            {
                parts.Add($"{kv.Key}:{kv.Value}");
            }
            PlayerPrefs.SetString(SeasonArchiveKey, string.Join(";", parts));
        }
    }
}
