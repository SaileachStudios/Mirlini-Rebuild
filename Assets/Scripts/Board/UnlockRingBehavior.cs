using SaileachStudios.Mirlini.Marble;
using UnityEngine;
namespace SaileachStudios.Mirlini.Board {
    [RequireComponent(typeof(SphereCollider))]
    public sealed class UnlockRingBehavior : MonoBehaviour {
        private MarbleBehaviour marble;
        private LevelAttempt attempt;
        private HoleBehavior goal;
        private Collider marbleCollider;
        private SphereCollider region;
        private bool occupied;
        public void Configure(MarbleBehaviour ball,LevelAttempt activeAttempt,HoleBehavior hole) {
            marble=ball;marbleCollider=ball.GetComponent<Collider>();attempt=activeAttempt;goal=hole;
            region=GetComponent<SphereCollider>();region.isTrigger=true;region.radius=LevelMechanics.RingRadius;
            occupied=false;
        }
        private void OnTriggerEnter(Collider other) { if(other==marbleCollider) occupied=true; }
        private void OnTriggerStay(Collider other) { if(other==marbleCollider) occupied=true; }
        private void OnTriggerExit(Collider other) {
            if(other!=marbleCollider) return;
            occupied=false;attempt?.OccupyRing(false,false,0);
        }
        private void FixedUpdate() {
            if(attempt==null || marble==null || attempt.GoalUnlocked) return;
            // Trigger bookkeeping plus a geometric check handles teleports/help before OnTriggerExit.
            bool stillInside=occupied && Physics.ComputePenetration(region,transform.position,transform.rotation,
                marbleCollider,marbleCollider.transform.position,marbleCollider.transform.rotation,out _,out _);
            attempt.OccupyRing(stillInside,marble.CanCallForHelp,Time.fixedDeltaTime);
            if(attempt.GoalUnlocked) goal.SetIsCorrectHole(true);
        }
        private void OnDisable() { occupied=false; }
    }
}
