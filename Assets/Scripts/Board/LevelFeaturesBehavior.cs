using System.Collections.Generic;
using SaileachStudios.Mirlini.Marble;
using UnityEngine;
using UnityEngine.Rendering;
namespace SaileachStudios.Mirlini.Board {
    // Authored scene owner; creates only the active level's small feature presentation objects.
    public sealed class LevelFeaturesBehavior : MonoBehaviour {
        public LevelAttempt Attempt { get; private set; }
        public UnlockRingBehavior Ring { get; private set; }
        public Light Spotlight { get; private set; }
        private MarbleBehaviour marble;
        private DarkLightingProfile dark;
        private readonly Dictionary<Light,bool> lightStates=new Dictionary<Light,bool>();
        private AmbientMode ambientMode;
        private Color ambientColor;
        private float ambientIntensity,reflectionIntensity;
        private bool lightingCaptured;
        private Material ringMaterial;
        public void Configure(LevelData data,GameObject[] walls,MarbleBehaviour ball,HoleBehavior goal) {
            RestoreLighting();marble=ball;dark=data.Dark;Attempt=new LevelAttempt(data.Unlock.Enabled);
            for(int i=0;i<walls.Length;i++) {
                var state=data.Walls[i];var reveal=walls[i].GetComponent<RevealWall>();
                if(state==WallState.RevealOnCollision && reveal==null) reveal=walls[i].AddComponent<RevealWall>();
                if(reveal!=null) reveal.Configure(state,i,Attempt,ball);
                else foreach(var renderer in walls[i].GetComponentsInChildren<Renderer>(true)) renderer.enabled=true;
                walls[i].SetActive(state!=WallState.Empty);
            }
            goal.SetIsCorrectHole(Attempt.GoalUnlocked);
            if(data.Unlock.Enabled) {
                if(Ring==null) CreateRing();
                Ring.transform.position=transform.position+data.Unlock.RingPosition;
                Ring.Configure(ball,Attempt,goal);Ring.gameObject.SetActive(true);
            } else if(Ring!=null) Ring.gameObject.SetActive(false);
            if(dark!=null) {
                CaptureLighting();
                if(Spotlight==null) {
                    var go=new GameObject("Marble spotlight");go.transform.SetParent(transform,false);Spotlight=go.AddComponent<Light>();
                }
                Spotlight.type=LightType.Spot;Spotlight.range=dark.Range;Spotlight.spotAngle=dark.SpotAngle;
                Spotlight.innerSpotAngle=4;Spotlight.intensity=dark.Intensity;Spotlight.color=dark.Color;
                Spotlight.shadows=LightShadows.None;Spotlight.enabled=true;FollowMarble();
            } else if(Spotlight!=null) Spotlight.enabled=false;
        }
        private void CreateRing() {
            var go=new GameObject("Unlock ring");go.transform.SetParent(transform,false);
            go.SetActive(false);go.AddComponent<SphereCollider>();Ring=go.AddComponent<UnlockRingBehavior>();
            var line=go.AddComponent<LineRenderer>();line.useWorldSpace=false;line.loop=true;line.positionCount=64;
            line.widthMultiplier=.06f;
            ringMaterial=new Material(Shader.Find("Sprites/Default"));
            ringMaterial.color=Color.yellow;line.sharedMaterial=ringMaterial;
            for(int i=0;i<64;i++) {
                float angle=i*Mathf.PI*2/64;
                line.SetPosition(i,new Vector3(Mathf.Cos(angle)*LevelMechanics.RingRadius,BoardGrid.FloorY+.03f,Mathf.Sin(angle)*LevelMechanics.RingRadius));
            }
        }
        private void CaptureLighting() {
            lightingCaptured=true;ambientMode=RenderSettings.ambientMode;ambientColor=RenderSettings.ambientLight;
            ambientIntensity=RenderSettings.ambientIntensity;reflectionIntensity=RenderSettings.reflectionIntensity;
            foreach(var light in FindObjectsByType<Light>(FindObjectsInactive.Include,FindObjectsSortMode.None)) {
                if(light==Spotlight || light.gameObject.scene!=gameObject.scene) continue;
                lightStates[light]=light.enabled;light.enabled=false;
            }
            RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=Color.black;
            RenderSettings.ambientIntensity=0;RenderSettings.reflectionIntensity=0;
        }
        private void RestoreLighting() {
            if(!lightingCaptured) return;
            foreach(var pair in lightStates) if(pair.Key!=null) pair.Key.enabled=pair.Value;
            lightStates.Clear();RenderSettings.ambientMode=ambientMode;RenderSettings.ambientLight=ambientColor;
            RenderSettings.ambientIntensity=ambientIntensity;RenderSettings.reflectionIntensity=reflectionIntensity;
            lightingCaptured=false;
        }
        private void LateUpdate() { if(dark!=null && Spotlight!=null && marble!=null) FollowMarble(); }
        private void FollowMarble() {
            Spotlight.transform.position=new Vector3(marble.transform.position.x,transform.position.y+dark.Height,marble.transform.position.z);
            Spotlight.transform.rotation=Quaternion.Euler(90,0,0);
        }
        private void OnDisable() { RestoreLighting();if(Spotlight!=null) Spotlight.enabled=false;if(Ring!=null) Ring.gameObject.SetActive(false); }
        private void OnDestroy() { if(ringMaterial!=null) Destroy(ringMaterial); }
    }
}
