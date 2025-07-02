using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaileachStudios.Mirlini.Board
{
    public class LevelManager : MonoBehaviour
    {
        [Header("Game Objects")]
        [SerializeField] private GameObject marble;
        [SerializeField] private GameObject hole;

        [Header("Levels")]
        [SerializeField] private LevelData[] levels;
        [SerializeField] private GameObject[] walls;

        private void Start() {
            if(marble == null) {
                Debug.LogError("Marble not set");
            }
            if(hole == null) {
                Debug.LogError("Hole note set");
            }
            if (levels == null || levels.Length == 0) {
                Debug.LogError("Levels not set.");
            }
            if (walls == null || walls.Length == 0) {
                Debug.LogError("Walls not set");
            }
            SetupLevel(0);
        }

        public void SetupLevel(int levelIndex) {
            Debug.Log("Setting up level");
            var marblePosition = levels[levelIndex].MarbleStartPosition;
            var holePosition = levels[levelIndex].HolePosition;
            var wallInfo = levels[levelIndex].wallInfo;

            marble.transform.position = marblePosition;
            hole.transform.position = holePosition;
            for (int index = 0; index < wallInfo.Length; index++) {
                walls[index].SetActive(wallInfo[index]);
            }
        }
    }
}