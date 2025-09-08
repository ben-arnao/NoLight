using UnityEngine;
using UnityEngine.UI;

namespace RogueLike2D.UI
{
    // Placeholder collection UI. Populate with real content later.
    public class CollectionUI : MonoBehaviour
    {
        [SerializeField] private Text collectionText;

        private void OnEnable()
        {
            if (collectionText)
            {
                collectionText.text = "- Heroes: Warrior\n- Abilities: Whirlwind, Meditate, Marked Strike, Power Strike\n- Items: (none)\n- Consumables: (none)";
            }
        }
    }
}
