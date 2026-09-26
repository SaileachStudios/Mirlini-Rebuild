using UnityEngine;
namespace SaileachStudios.Mirlini.Board {
    // Raised world-space marker sits above wall tops without changing the goal trigger.
    // Barred circle means locked; open circle and downward stem identify a valid destination.
    public sealed class GoalPresentation : MonoBehaviour {
        private LineRenderer rim, bar, stem;
        public bool IsUnlocked { get; private set; }
        private void Awake() {
            var backing=WorldFeedback.Line(transform,"Goal contrast",.22f,new Color(.04f,.07f,.09f));
            WorldFeedback.Arc(backing,.66f,1.65f,1);
            rim=WorldFeedback.Line(transform,"Goal beacon",.12f,Color.white);
            WorldFeedback.Arc(rim,.66f,1.68f,1);
            bar=WorldFeedback.Line(transform,"Locked bar",.14f,Color.white);
            bar.positionCount=2;bar.SetPositions(new[]{new Vector3(-.5f,1.7f,-.4f),new Vector3(.5f,1.7f,.4f)});
            stem=WorldFeedback.Line(transform,"Goal location",.07f,Color.white);
            stem.positionCount=2;stem.SetPositions(new[]{new Vector3(0,.1f,0),new Vector3(0,1.5f,0)});
            SetUnlocked(IsUnlocked);
        }
        public void SetUnlocked(bool value) {
            IsUnlocked=value;if(rim==null) return;
            Color color=value?new Color(.25f,1,1):new Color(1,.65f,.18f);
            rim.startColor=rim.endColor=bar.startColor=bar.endColor=stem.startColor=stem.endColor=color;
            bar.enabled=!value;
        }
    }
}
