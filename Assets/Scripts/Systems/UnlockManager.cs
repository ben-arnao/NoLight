using UnityEngine;

namespace RogueLike2D.Systems
{
    // Manages unlock tokens and character unlock costs.
    public class UnlockManager : MonoBehaviour
    {
        private const string TokenKey = "unlockTokens";
        private const string UnlockCountKey = "unlockedCharacters";

        public int GetTokenCount() => PlayerPrefs.GetInt(TokenKey, 0);

        public void AddTokens(int amount)
        {
            int tokens = GetTokenCount() + Mathf.Max(0, amount);
            PlayerPrefs.SetInt(TokenKey, tokens);
            PlayerPrefs.Save();
        }

        public int GetUnlockedCharacterCount() => PlayerPrefs.GetInt(UnlockCountKey, 0);

        public int GetCurrentUnlockCost()
        {
            // Base 3 tokens, +1 every 3 unlocks as a simple ramp.
            int unlocked = GetUnlockedCharacterCount();
            return 3 + (unlocked / 3);
        }

        public bool TryUnlockCharacter()
        {
            int tokens = GetTokenCount();
            int cost = GetCurrentUnlockCost();
            if (tokens < cost) return false;

            PlayerPrefs.SetInt(TokenKey, tokens - cost);
            PlayerPrefs.SetInt(UnlockCountKey, GetUnlockedCharacterCount() + 1);
            PlayerPrefs.Save();
            return true;
        }
    }
}
