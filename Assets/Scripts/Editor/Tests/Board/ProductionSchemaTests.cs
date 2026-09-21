using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using SaileachStudios.Mirlini.Board;
public class ProductionSchemaTests {
    private LevelData Fresh() {
        var level=ScriptableObject.CreateInstance<LevelData>();level.LevelId="test-id";
        level.MarbleStartPosition=new Vector3(-5,0,-5);level.HolePosition=new Vector3(5,0,5);return level;
    }
    [Test] public void EnumArrayAndCombinedFeaturesRoundTrip() {
        var a=Fresh();var b=Fresh();
        try {
            a.Walls[3]=WallState.Solid;a.Walls[5]=WallState.RevealOnCollision;a.Unlock.Enabled=true;
            a.Dark=AssetDatabase.LoadAssetAtPath<DarkLightingProfile>(ProductionLevelImporter.ProfilePath);
            EditorJsonUtility.FromJsonOverwrite(EditorJsonUtility.ToJson(a),b);
            CollectionAssert.AreEqual(a.Walls,b.Walls);Assert.IsTrue(b.Unlock.Enabled);Assert.AreSame(a.Dark,b.Dark);
            Assert.AreEqual(a.LevelId,b.LevelId);Assert.AreEqual(StarCalibrationState.NeedsCalibration,b.StarCalibration);
        } finally {UnityEngine.Object.DestroyImmediate(a);UnityEngine.Object.DestroyImmediate(b);}
    }
    [Test] public void IdentitySurvivesDisplayRenameAndCampaignReorder() {
        var a=Fresh();var b=Fresh();b.LevelId="second";
        try {a.LevelName="renamed";Assert.AreEqual("test-id",a.LevelId);Assert.IsTrue(LevelDataValidation.TryValidateCampaign(new[]{b,a},out _));}
        finally {UnityEngine.Object.DestroyImmediate(a);UnityEngine.Object.DestroyImmediate(b);}
    }
    [Test] public void DuplicateCampaignIdentityRejected() {
        var a=Fresh();var b=Fresh();try {Assert.IsFalse(LevelDataValidation.TryValidateCampaign(new[]{a,b},out _));}
        finally {UnityEngine.Object.DestroyImmediate(a);UnityEngine.Object.DestroyImmediate(b);}
    }
    [TestCase(0)][TestCase(1)][TestCase(2)][TestCase(3)][TestCase(4)][TestCase(5)][TestCase(6)]
    public void InvalidConfigurationRejected(int variant) {
        var level=Fresh();try {
            switch(variant) {
                case 0:level.LevelId="";break;
                case 1:level.Walls=new WallState[479];break;
                case 2:level.Walls[0]=(WallState)99;break;
                case 3:level.MarbleStartPosition=new Vector3(float.NaN,0,0);break;
                case 4:level.HolePosition=Vector3.right*100;break;
                case 5:level.Unlock.Enabled=true;level.Unlock.RingPosition=Vector3.right*100;break;
                case 6:level.StarCalibration=(StarCalibrationState)1;break;
            }
            Assert.IsFalse(LevelDataValidation.TryValidate(level,out _));
        } finally {UnityEngine.Object.DestroyImmediate(level);}
    }
    [Test] public void RevealWallBlocksStartLikeSolid() {
        var level=Fresh();try {
            var p=BoardGrid.ToWorld(BoardGrid.GetEdge(0).Center);level.MarbleStartPosition=new Vector3(p.X,0,p.Z);
            foreach(var state in new[]{WallState.Solid,WallState.RevealOnCollision}) {level.Walls[0]=state;Assert.IsFalse(LevelDataValidation.TryValidate(level,out _));}
        } finally {UnityEngine.Object.DestroyImmediate(level);}
    }
    [Test] public void UnlockRequiresContinuousTwoSecondsAndRetainsSuccess() {
        var attempt=new LevelAttempt(true);
        attempt.OccupyRing(true,true,1.5f);Assert.IsFalse(attempt.GoalUnlocked);
        attempt.OccupyRing(false,true,0);Assert.AreEqual(0,attempt.UnlockProgress);
        attempt.OccupyRing(true,true,1);attempt.OccupyRing(true,false,10);Assert.AreEqual(1,attempt.UnlockProgress);
        attempt.OccupyRing(true,true,1);Assert.IsTrue(attempt.GoalUnlocked);
        attempt.OccupyRing(false,true,0);Assert.IsTrue(attempt.GoalUnlocked);
        Assert.IsFalse(new LevelAttempt(true).GoalUnlocked);
    }
    [Test] public void RevealStateBelongsOnlyToAttempt() {
        var first=new LevelAttempt(false);first.Reveal(4);Assert.IsTrue(first.IsRevealed(4));Assert.IsFalse(new LevelAttempt(false).IsRevealed(4));
    }
    [Test] public void RepresentativeBatchMatchesDeterministicNormalization() {
        var profile=AssetDatabase.LoadAssetAtPath<DarkLightingProfile>(ProductionLevelImporter.ProfilePath);
        var source=ProductionLevelImporter.ReadSource();Assert.AreEqual(11,source.levels.Length);
        foreach(var entry in source.levels) {
            var a=ProductionLevelImporter.Normalize(entry,profile);var b=ProductionLevelImporter.Normalize(entry,profile);
            try {
                Assert.AreEqual(JsonUtility.ToJson(a),JsonUtility.ToJson(b));
                var asset=AssetDatabase.LoadAssetAtPath<LevelData>(ProductionLevelImporter.Output+"/Level "+entry.number.ToString("00")+".asset");
                Assert.AreEqual(JsonUtility.ToJson(a),JsonUtility.ToJson(asset));
                Assert.IsTrue(LevelTraversal.CanReachObjectives(asset));
                Assert.AreEqual(StarCalibrationState.NeedsCalibration,asset.StarCalibration);
                Assert.AreEqual(entry.historicalTime,asset.HistoricalIdealTime);
                Assert.AreEqual(entry.instructions.Keyboard,asset.Instructions.Keyboard);
                Assert.AreEqual(entry.instructions.Touch,asset.Instructions.Touch);
                Assert.AreEqual(entry.instructions.Gyro,asset.Instructions.Gyro);
            } finally {UnityEngine.Object.DestroyImmediate(a);UnityEngine.Object.DestroyImmediate(b);}
        }
    }
    [TestCase(15,14,"Deduplicated edge occurrences: 1")]
    [TestCase(40,98,"snapped segments: 4")]
    [TestCase(50,225,"Deduplicated edge occurrences: 5")]
    public void HistoricalGeometryIsNormalized(int number,int edges,string note) {
        var level=AssetDatabase.LoadAssetAtPath<LevelData>(ProductionLevelImporter.Output+"/Level "+number.ToString("00")+".asset");
        Assert.AreEqual(edges,level.Walls.Count(x=>x!=WallState.Empty));StringAssert.Contains(note,level.MigrationNotes);
    }
    [Test] public void ConflictingSourceStatesFailBeforeAssetWrite() {
        var source=ProductionLevelImporter.ReadSource().levels.Single(x=>x.number==15);
        var original=source.walls[0];source.walls=source.walls.Concat(new[]{new AuditedWall {sourceId="conflict",center=original.center,size=original.size,reveal=false}}).ToArray();
        Assert.Throws<ArgumentException>(()=>ProductionLevelImporter.Normalize(source,null));
    }
    [Test] public void UnreachableGoalDetectedWithoutGameplaySolver() {
        var level=Fresh();try {
            for(int i=0;i<BoardGrid.EdgeCount;i++) {var edge=BoardGrid.GetEdge(i);if(!edge.AlongZ && edge.Center.Z==0) level.Walls[i]=WallState.Solid;}
            Assert.IsFalse(LevelTraversal.CanReachObjectives(level));
        } finally {UnityEngine.Object.DestroyImmediate(level);}
    }
}
