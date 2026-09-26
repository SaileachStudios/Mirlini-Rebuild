using SaileachStudios.Mirlini.Marble;
using UnityEngine;
namespace SaileachStudios.Mirlini.Board {
    public sealed class RevealWall : MonoBehaviour {
        private Renderer[] visuals;
        private MarbleBehaviour marble;
        private LevelAttempt attempt;
        private int edge;
        private bool reveal;
        private float highlight;
        private LineRenderer discoveryOutline;
        private MaterialPropertyBlock tint;
        private Color[] baseColors;
        public bool IsHighlighting => highlight>0;
        private static readonly int ColorId=Shader.PropertyToID("_Color");
        public void Configure(WallState state,int index,LevelAttempt activeAttempt,MarbleBehaviour ball) {
            visuals=System.Array.FindAll(GetComponentsInChildren<Renderer>(true),r=>!(r is LineRenderer));marble=ball;attempt=activeAttempt;edge=index;
            if(discoveryOutline!=null) discoveryOutline.enabled=false;
            highlight=0;tint ??= new MaterialPropertyBlock();
            baseColors=new Color[visuals.Length];
            for(int i=0;i<visuals.Length;i++) {
                baseColors[i]=visuals[i].sharedMaterial!=null && visuals[i].sharedMaterial.HasProperty(ColorId)?visuals[i].sharedMaterial.GetColor(ColorId):Color.white;
                visuals[i].SetPropertyBlock(null);
            }
            enabled=state==WallState.RevealOnCollision;
            reveal=state==WallState.RevealOnCollision;
            foreach(var visual in visuals) visual.enabled=!reveal || attempt.IsRevealed(edge);
        }
        private void Update() {
            if(highlight<=0) return;
            highlight=Mathf.Max(0,highlight-Time.deltaTime);ApplyHighlight();
        }
        private void ApplyHighlight() {
            if(discoveryOutline!=null) {
                discoveryOutline.enabled=highlight>0;
                discoveryOutline.startColor=discoveryOutline.endColor=new Color(1,.75f,.25f,highlight/.35f);
            }
            for(int i=0;i<visuals.Length;i++) {
                if(highlight<=0) { visuals[i].SetPropertyBlock(null);continue; }
                tint.Clear();tint.SetColor(ColorId,Color.Lerp(baseColors[i],new Color(1,.75f,.25f),highlight/.35f));
                visuals[i].SetPropertyBlock(tint);
            }
        }
        private void OnDisable() {
            if(discoveryOutline!=null) discoveryOutline.enabled=false;
            highlight=0;if(visuals!=null) foreach(var visual in visuals) if(visual!=null) visual.SetPropertyBlock(null);
        }
        private void OnCollisionEnter(Collision collision) { Reveal(collision.collider); }
        private void OnCollisionStay(Collision collision) { Reveal(collision.collider); }
        private void Reveal(Collider other) {
            if(!reveal || marble==null || !marble.CanCallForHelp || other.GetComponentInParent<MarbleBehaviour>()!=marble) return;
            if(attempt.IsRevealed(edge)) return;
            attempt.Reveal(edge);
            foreach(var visual in visuals) visual.enabled=true;
            if(discoveryOutline==null) {
                discoveryOutline=WorldFeedback.Line(transform,"Discovery outline",.08f,new Color(1,.75f,.25f));
                discoveryOutline.loop=true;discoveryOutline.positionCount=4;
                discoveryOutline.SetPositions(new[]{new Vector3(-.5f,.51f,-.5f),new Vector3(-.5f,.51f,.5f),
                    new Vector3(.5f,.51f,.5f),new Vector3(.5f,.51f,-.5f)});
            }
            highlight=.35f;ApplyHighlight();
        }
    }
}
