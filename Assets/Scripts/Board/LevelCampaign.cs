using UnityEngine;
namespace SaileachStudios.Mirlini.Board {
    [CreateAssetMenu(menuName="Mirlini/Level campaign")]
    public sealed class LevelCampaign : ScriptableObject {
        public LevelData[] Levels = new LevelData[0];
    }
}
