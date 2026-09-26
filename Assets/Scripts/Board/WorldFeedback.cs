using UnityEngine;
using UnityEngine.Rendering;
using SaileachStudios.Mirlini.Core;
namespace SaileachStudios.Mirlini.Board {
    internal static class WorldFeedback {
        public static LineRenderer Line(Transform parent,string name,float width,Color color) {
            var child=new GameObject(name);child.transform.SetParent(parent,false);
            var line=child.AddComponent<LineRenderer>();line.useWorldSpace=false;
            line.sharedMaterial=GameplayFeel.Load().WorldFeedbackMaterial;
            line.widthMultiplier=width;line.startColor=line.endColor=color;
            line.shadowCastingMode=ShadowCastingMode.Off;line.receiveShadows=false;line.numCapVertices=2;
            return line;
        }
        public static void Arc(LineRenderer line,float radius,float height,float fraction) {
            const int segments=64;
            line.loop=false;int count=Mathf.Max(2,Mathf.CeilToInt(segments*fraction)+1);line.positionCount=count;
            for(int i=0;i<count;i++) {
                float a=(.25f+fraction*i/(count-1))*Mathf.PI*2;
                line.SetPosition(i,new Vector3(Mathf.Cos(a)*radius,height,Mathf.Sin(a)*radius));
            }
            line.enabled=fraction>0;
        }
    }
}
