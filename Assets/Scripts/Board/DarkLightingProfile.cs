using UnityEngine;
namespace SaileachStudios.Mirlini.Board {
    [CreateAssetMenu(menuName="Mirlini/Dark lighting profile")]
    public sealed class DarkLightingProfile : ScriptableObject {
        public float Height = 6f * BoardGrid.CellSize;
        public float Range = 21.45f * BoardGrid.CellSize;
        public float SpotAngle = 46.6f;
        public float Intensity = 3f;
        public Color Color = Color.white;
        public bool IsValid => BoardGrid.IsFinite(Height) && Height > 0 &&
            BoardGrid.IsFinite(Range) && Range > Height && BoardGrid.IsFinite(SpotAngle) &&
            SpotAngle > 0 && SpotAngle < 179 && BoardGrid.IsFinite(Intensity) && Intensity > 0 &&
            BoardGrid.IsFinite(Color.r) && BoardGrid.IsFinite(Color.g) && BoardGrid.IsFinite(Color.b) && BoardGrid.IsFinite(Color.a);
    }
}
