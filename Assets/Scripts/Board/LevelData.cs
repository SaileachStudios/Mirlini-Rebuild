using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaileachStudios.Mirlini.Board
{
    [CreateAssetMenu(fileName = "NewLevelData", menuName = "Mirlini/Level Data")]
    public class LevelData : ScriptableObject
    {
        public string LevelName;
        public Vector3 MarbleStartPosition;
        public Vector3 HolePosition;
        public bool[] wallInfo = new bool[480]; // Reference which wall positions are active
        public float IdealCompletionTime;
        public LevelType Type;
    }

    public enum LevelType
    {
        Normal,
        Unlock,
        Dark,
        InvisibleWalls
    }
}