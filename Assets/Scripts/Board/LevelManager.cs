using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaileachStudios.Mirlini.Marble;
using SaileachStudios.Mirlini.Core;

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
        private int currentLevelIndex = 0;
        private bool isSubscribedToGameEvents = false;
        private bool isAdvancingLevel = false;

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

            TrySubscribeToGameEvents();
            SetupLevel(0);
        }

        private void OnEnable() {
            TrySubscribeToGameEvents();
        }

        private void OnDisable() {
            UnsubscribeFromGameEvents();
        }

        public void SetupLevel(int levelIndex) {
            if (levels == null || levelIndex < 0 || levelIndex >= levels.Length) {
                Debug.LogError($"Invalid level index {levelIndex}");
                return;
            }

            currentLevelIndex = levelIndex;
            isAdvancingLevel = false;
            Debug.Log("Setting up level: " + levelIndex);
            var marblePosition = levels[levelIndex].MarbleStartPosition;
            var holePosition = levels[levelIndex].HolePosition;
            var wallInfo = levels[levelIndex].wallInfo;

            marble.transform.position = marblePosition;
            hole.transform.position = holePosition;

            HoleBehavior holeBehavior = hole.GetComponent<HoleBehavior>();
            if (holeBehavior == null) {
                Debug.LogError("HoleBehavior not found on hole object.");
                return;
            }

            holeBehavior.SetIsCorrectHole(true);

            if (wallInfo == null) {
                Debug.LogError($"Level '{levels[levelIndex].name}' has no wall data.");
                return;
            }

            if (walls.Length != wallInfo.Length) {
                Debug.LogError($"Wall setup mismatch. Scene has {walls.Length} walls, but level '{levels[levelIndex].name}' has {wallInfo.Length} entries.");
            }

            int configuredWallCount = Mathf.Min(walls.Length, wallInfo.Length);
            for (int index = 0; index < configuredWallCount; index++) {
                walls[index].SetActive(wallInfo[index]);
            }

            for (int index = configuredWallCount; index < walls.Length; index++) {
                walls[index].SetActive(false);
            }

            MarbleBehaviour marbleBehaviour = marble.GetComponent<MarbleBehaviour>();
            if (marbleBehaviour == null) {
                Debug.LogError("MarbleBehaviour not found on marble object.");
                return;
            }

            marbleBehaviour.StartPlaying();
        }

        public bool LoadNextLevel() {
            int nextLevelIndex = currentLevelIndex + 1;
            if (levels == null || nextLevelIndex >= levels.Length) {
                return false;
            }

            SetupLevel(nextLevelIndex);
            return true;
        }

        private void OnLevelCompleted() {
            if (isAdvancingLevel) {
                return;
            }

            isAdvancingLevel = true;
            if (!LoadNextLevel()) {
                Debug.Log("No additional levels are configured.");
                isAdvancingLevel = false;
            }
        }

        private void TrySubscribeToGameEvents() {
            if (isSubscribedToGameEvents || GameManagerBehavior.Instance == null) {
                return;
            }

            GameManagerBehavior.Instance.Events.OnLevelCompleted += OnLevelCompleted;
            isSubscribedToGameEvents = true;
        }

        private void UnsubscribeFromGameEvents() {
            if (!isSubscribedToGameEvents || GameManagerBehavior.Instance == null) {
                return;
            }

            GameManagerBehavior.Instance.Events.OnLevelCompleted -= OnLevelCompleted;
            isSubscribedToGameEvents = false;
        }
    }
}
