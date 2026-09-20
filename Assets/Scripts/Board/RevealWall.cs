using SaileachStudios.Mirlini.Marble;
using UnityEngine;
namespace SaileachStudios.Mirlini.Board {
    public sealed class RevealWall : MonoBehaviour {
        private Renderer[] visuals;
        private MarbleBehaviour marble;
        private LevelAttempt attempt;
        private int edge;
        private bool reveal;
        public void Configure(WallState state,int index,LevelAttempt activeAttempt,MarbleBehaviour ball) {
            visuals=GetComponentsInChildren<Renderer>(true);marble=ball;attempt=activeAttempt;edge=index;
            reveal=state==WallState.RevealOnCollision;
            foreach(var visual in visuals) visual.enabled=!reveal || attempt.IsRevealed(edge);
        }
        private void OnCollisionEnter(Collision collision) { Reveal(collision.collider); }
        private void OnCollisionStay(Collision collision) { Reveal(collision.collider); }
        private void Reveal(Collider other) {
            if(!reveal || marble==null || !marble.CanCallForHelp || other.GetComponentInParent<MarbleBehaviour>()!=marble) return;
            if(attempt.IsRevealed(edge)) return;
            attempt.Reveal(edge);
            foreach(var visual in visuals) visual.enabled=true;
        }
    }
}
