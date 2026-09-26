using UnityEngine;
namespace SaileachStudios.Mirlini.Board {
    // Reads attempt state; never controls dwell, pause, or completion rules.
    public sealed class UnlockRingPresentation : MonoBehaviour {
        private LevelAttempt attempt;
        private LineRenderer progress, outline, tick;
        public float Progress01 { get; private set; }
        public bool Completed { get; private set; }
        public void Bind(LevelAttempt value) { attempt=value;Refresh(); }
        private void Awake() {
            var backing=WorldFeedback.Line(transform,"Ring contrast",.25f,new Color(.06f,.08f,.1f));
            WorldFeedback.Arc(backing,LevelMechanics.RingRadius,BoardGrid.FloorY+.06f,1);
            outline=WorldFeedback.Line(transform,"Ring boundary",.13f,new Color(1,.75f,.15f));
            WorldFeedback.Arc(outline,LevelMechanics.RingRadius,BoardGrid.FloorY+.08f,1);
            progress=WorldFeedback.Line(transform,"Dwell progress",.2f,new Color(.25f,1,1));
            tick=WorldFeedback.Line(transform,"Unlocked check",.16f,new Color(.25f,1,1));
            tick.positionCount=3;tick.SetPositions(new[]{new Vector3(-.6f,BoardGrid.FloorY+.12f,0),
                new Vector3(-.15f,BoardGrid.FloorY+.12f,-.45f),new Vector3(.65f,BoardGrid.FloorY+.12f,.55f)});
            Refresh();
        }
        private void LateUpdate() { Refresh(); }
        public void Refresh() {
            Progress01=attempt==null?0:Mathf.Clamp01(attempt.UnlockProgress/LevelMechanics.UnlockSeconds);
            Completed=attempt!=null && attempt.GoalUnlocked;
            if(progress==null) return;
            WorldFeedback.Arc(progress,LevelMechanics.RingRadius-.28f,BoardGrid.FloorY+.1f,Progress01);
            tick.enabled=Completed;
            outline.startColor=outline.endColor=Completed?new Color(.25f,1,1):new Color(1,.75f,.15f);
        }
        private void OnDisable() { attempt=null;Progress01=0;Completed=false; }
    }
}
