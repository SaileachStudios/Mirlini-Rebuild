using System.Collections.Generic;
using UnityEngine;
using SaileachStudios.Mirlini.Core;
using SaileachStudios.Mirlini.Marble;

namespace SaileachStudios.Mirlini.Board
{
    [RequireComponent(typeof(LevelEndFlowController))]
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private GameObject marble;
        [SerializeField] private GameObject hole;
        [SerializeField] private LevelData[] levels;
        [SerializeField] private GameObject[] walls;
        [Tooltip("East, North, West, South; unit cube colliders.")]
        [SerializeField] private GameObject[] boundaryWalls;
        [SerializeField] private Transform floor;
        [Min(0.001f)] [SerializeField] private float helpSafetyMargin = 0.05f;
        [Min(0f)] [SerializeField] private float maxHelpDistance = BoardGrid.CellSize * 2f;
        private int currentLevelIndex = -1;
        public int CurrentLevelIndex => currentLevelIndex;

        private void Start() { SetupLevel(0); }
        public void SetupLevel(int levelIndex) { TrySetupLevel(levelIndex); }
        public bool TrySetupLevel(int levelIndex) {
            if (!TryValidateSetup(levelIndex, out string error)) { Debug.LogError(error, this); return false; }
            var owner = GameManagerBehavior.Instance;
            var ball = marble.GetComponent<MarbleBehaviour>();
            var goal = hole.GetComponent<HoleBehavior>();
            // Validation completes before any geometry, state or index is changed.
            ApplyGeometry();
            for (int i = 0; i < walls.Length; i++) walls[i].SetActive(levels[levelIndex].wallInfo[i]);
            marble.transform.position = transform.position + levels[levelIndex].MarbleStartPosition;
            hole.transform.position = transform.position + levels[levelIndex].HolePosition;
            ball.Bind(owner); goal.Bind(owner);
            goal.SetIsCorrectHole(true);
            ball.StartPlaying(); // validated live object initialized in Awake, never Start-order dependent
            currentLevelIndex = levelIndex;
            return true;
        }
        public bool TryValidateSetup(int levelIndex, out string error) {
            error = null;
            if (levels == null || levelIndex < 0 || levelIndex >= levels.Length) { error = "Invalid level index or missing level list."; return false; }
            if (GameManagerBehavior.Instance == null || !GameManagerBehavior.Instance.isActiveAndEnabled) { error = "An active GameManagerBehavior is required."; return false; }
            var flow = GetComponent<LevelEndFlowController>();
            if (flow == null || !flow.enabled) { error = "Authored LevelEndFlowController is missing or disabled."; return false; }
            if (marble == null || hole == null || marble == hole) { error = "Marble and goal references must be distinct and assigned."; return false; }
            var ball = marble.GetComponent<MarbleBehaviour>(); var goal = hole.GetComponent<HoleBehavior>();
            if (ball == null || marble.GetComponent<Rigidbody>() == null || marble.GetComponent<SphereCollider>() == null || !ball.CanStartPlaying) {
                error = "Marble is missing its physics/behavior or is not ready for level setup."; return false;
            }
            var sphere = marble.GetComponent<SphereCollider>();
            if (!sphere.enabled || sphere.isTrigger || sphere.center != Vector3.zero || ball.CollisionRadius <= 0f || !BoardGrid.IsFinite(ball.CollisionRadius)) {
                error = "Marble needs an enabled, centered, non-trigger sphere with a positive playable radius."; return false;
            }
            if (marble.GetComponent<Rigidbody>().isKinematic && ball.CurrentState == BallState.Playing) {
                error = "Playable marble cannot be kinematic."; return false;
            }
            if (goal == null || !goal.isActiveAndEnabled || !goal.IsConfigured || hole.GetComponent<Collider>() == null || !hole.GetComponent<Collider>().enabled || !hole.GetComponent<Collider>().isTrigger) {
                error = "Goal requires active HoleBehavior, two indicators and its trigger collider."; return false;
            }
            if (!LevelDataValidation.TryValidate(levels[levelIndex], out error, ball.CollisionRadius)) return false;
            var goalCollider = hole.GetComponent<Collider>();
            // Setup may run before the next physics step. Match native bounds to current transforms
            // before subtracting the transform position to calculate the local goal footprint.
            Physics.SyncTransforms();
            Bounds goalBounds = goalCollider.bounds;
            float goalRadius = Mathf.Max(goalBounds.extents.x, goalBounds.extents.z);
            Vector3 goalPoint = levels[levelIndex].HolePosition + goalBounds.center - hole.transform.position;
            if (!BoardGrid.IsInside(new BoardPoint(goalPoint.x,goalPoint.z), goalRadius) ||
                Vector2.Distance(new Vector2(levels[levelIndex].MarbleStartPosition.x, levels[levelIndex].MarbleStartPosition.z), new Vector2(goalPoint.x,goalPoint.z)) < ball.CollisionRadius + goalRadius) {
                error = "Goal footprint must fit inside the board and remain separate from the marble start."; return false;
            }
            for (int i=0;i<BoardGrid.EdgeCount;i++) if (levels[levelIndex].wallInfo[i] &&
                BoardGrid.GetWallRectangle(i).Expanded(goalRadius).ContainsInterior(new BoardPoint(goalPoint.x,goalPoint.z))) {
                error = $"Goal footprint overlaps wall edge {i}."; return false;
            }
            return TryValidateGeometry(out error);
        }
        public bool TryValidateGeometry(out string error) {
            error = null;
            if (transform.rotation != Quaternion.identity || (transform.lossyScale-Vector3.one).sqrMagnitude > 0.000001f) {
                error = "Board origin must have identity rotation and unit scale."; return false;
            }
            if (walls == null || walls.Length != BoardGrid.EdgeCount || boundaryWalls == null || boundaryWalls.Length != 4 || floor == null || !floor.gameObject.activeInHierarchy || floor.GetComponent<Collider>() == null || !floor.GetComponent<Collider>().enabled) {
                error = "Board requires 480 internal walls, four ordered boundaries and a floor."; return false;
            }
            foreach (GameObject boundary in boundaryWalls) if (boundary == null || !boundary.activeInHierarchy) { error = "Board boundaries must be active."; return false; }
            var seen = new HashSet<GameObject> { marble, hole, gameObject, floor.gameObject };
            foreach (var array in new[] { walls, boundaryWalls }) foreach (GameObject wall in array) {
                if (wall == null || !seen.Add(wall)) { error = "Wall references must be assigned and unique."; return false; }
                var box = wall.GetComponent<BoxCollider>();
                if (box == null || !box.enabled || box.isTrigger || box.center != Vector3.zero || box.size != Vector3.one ||
                    (wall.transform.parent != null && (wall.transform.parent.lossyScale-Vector3.one).sqrMagnitude > 0.000001f)) {
                    error = "Walls need enabled non-trigger unit box colliders and unscaled parents."; return false;
                }
            }
            if (floor.parent != null && (floor.parent.lossyScale-Vector3.one).sqrMagnitude > 0.000001f) { error = "Floor parent must have unit scale."; return false; }
            return true;
        }
        // Used by editor authoring too; call validation before mutation.
        public void ApplyGeometry() {
            for (int i=0; i<walls.Length; i++) BoardGeometryPlacement.ApplyWall(walls[i].transform, i, transform.position);
            for (int i=0; i<boundaryWalls.Length; i++) BoardGeometryPlacement.ApplyBoundary(boundaryWalls[i].transform, i, transform.position);
            BoardGeometryPlacement.ApplyFloor(floor, transform.position);
        }
        public bool LoadNextLevel() {
            int next = currentLevelIndex + 1;
            return levels != null && next < levels.Length && TrySetupLevel(next);
        }
        public bool TryCallForHelp() {
            var ball = marble != null ? marble.GetComponent<MarbleBehaviour>() : null;
            if (currentLevelIndex < 0 || ball == null || !ball.CanCallForHelp || !TryValidateGeometry(out _) ||
                !BoardGrid.IsFinite(helpSafetyMargin) || helpSafetyMargin <= 0) return false;
            var sphere = marble.GetComponent<SphereCollider>();
            Vector3 center = sphere.transform.TransformPoint(sphere.center), origin = transform.position;
            var obstacles = new List<BoardRectangle>();
            // Live collider bounds include hidden but solid walls. No reveal/goal state is changed.
            Physics.SyncTransforms();
            foreach (GameObject wall in walls) if (wall.activeInHierarchy) AddObstacle(wall.GetComponent<Collider>(), origin, obstacles);
            foreach (GameObject boundary in boundaryWalls) AddObstacle(boundary.GetComponent<Collider>(), origin, obstacles);
            // Exclude the entire goal footprint so help never relocates the marble into a hole.
            foreach (Collider collider in hole.GetComponentsInChildren<Collider>()) AddObstacle(collider, origin, obstacles);
            if (!HelpPositionPlanner.TryFind(new BoardPoint(center.x-origin.x, center.z-origin.z), ball.CollisionRadius,
                helpSafetyMargin, maxHelpDistance, obstacles, out BoardPoint destination)) return false;
            Vector3 delta = new Vector3(destination.X-(center.x-origin.x),
                Mathf.Max(0f, origin.y + BoardGrid.FloorY + ball.CollisionRadius - center.y), destination.Z-(center.z-origin.z));
            return ball.ApplyHelpPosition(marble.transform.position + delta);
        }
        private static void AddObstacle(Collider collider, Vector3 origin, List<BoardRectangle> obstacles) {
            if (collider == null || !collider.enabled) return;
            Bounds b = collider.bounds;
            obstacles.Add(new BoardRectangle(b.min.x-origin.x,b.min.z-origin.z,b.max.x-origin.x,b.max.z-origin.z));
        }
    }
}
