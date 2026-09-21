using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using SaileachStudios.Mirlini.Board;
public class FullCampaignTests {
    [Serializable] public class Snapshot { public AssetSnapshot[] assets; }
    [Serializable] public class AssetSnapshot { public string path,sha256,id,guid; }
    private LevelCampaign Campaign => AssetDatabase.LoadAssetAtPath<LevelCampaign>(ProductionLevelImporter.FullCampaignPath);
    [Test] public void CompleteCampaignHasFiftyUniqueIdentitiesInIntendedOrder() {
        Assert.IsNotNull(Campaign);Assert.AreEqual(50,Campaign.Levels.Length);
        Assert.AreEqual(50,AssetDatabase.FindAssets("t:LevelData",new[]{ProductionLevelImporter.Output}).Length);
        Assert.IsTrue(LevelDataValidation.TryValidateCampaign(Campaign.Levels,out string error),error);
        CollectionAssert.AreEqual(Enumerable.Range(1,50),Campaign.Levels.Select(x=>x.MigrationLevelNumber));
        Assert.AreEqual(50,Campaign.Levels.Select(x=>x.LevelId).Distinct().Count());
    }
    [Test] public void FullMechanicDistributionMatchesIndependentAuditTotals() {
        var levels=Campaign.Levels;
        int[] actual=new int[8];
        foreach(var level in levels) actual[(level.Unlock.Enabled?1:0)|(level.Dark!=null?2:0)|(level.Walls.Contains(WallState.RevealOnCollision)?4:0)]++;
        CollectionAssert.AreEqual(new[]{17,9,8,5,7,4,0,0},actual);
        Assert.AreEqual(18,levels.Count(x=>x.Unlock.Enabled));Assert.AreEqual(13,levels.Count(x=>x.Dark!=null));
        Assert.AreEqual(11,levels.Count(x=>x.Walls.Contains(WallState.RevealOnCollision)));
    }
    [Test] public void AllFiftyNormalizeDeterministicallyAndReachEveryObjective() {
        var source=ProductionLevelImporter.ReadSource(ProductionLevelImporter.FullIntermediate);
        Assert.AreEqual(50,source.levels.Length);
        var profile=AssetDatabase.LoadAssetAtPath<DarkLightingProfile>(ProductionLevelImporter.ProfilePath);
        foreach(var entry in source.levels) {
            var a=ProductionLevelImporter.Normalize(entry,profile);var b=ProductionLevelImporter.Normalize(entry,profile);
            try {
                var asset=Campaign.Levels[entry.number-1];
                Assert.AreEqual(JsonUtility.ToJson(a),JsonUtility.ToJson(b),"Repeat normalization "+entry.number);
                Assert.AreEqual(JsonUtility.ToJson(a),JsonUtility.ToJson(asset),"Saved content "+entry.number);
                Assert.AreEqual("mirlini-"+entry.sourceGuid,asset.LevelId);
                Assert.AreEqual(entry.sourceHash,asset.MigrationSourceHash);Assert.AreEqual(64,asset.MigrationSourceHash.Length);
                Assert.AreEqual(StarCalibrationState.NeedsCalibration,asset.StarCalibration);
                Assert.AreEqual(entry.historicalTime,asset.HistoricalIdealTime);
                Assert.AreEqual(entry.instructions.Keyboard,asset.Instructions.Keyboard);
                Assert.AreEqual(entry.instructions.Touch,asset.Instructions.Touch);Assert.AreEqual(entry.instructions.Gyro,asset.Instructions.Gyro);
                Assert.IsTrue(LevelTraversal.CanReachObjectives(asset),"Start routes "+entry.number);
                if(asset.Unlock.Enabled) {
                    b.MarbleStartPosition=b.Unlock.RingPosition;
                    Assert.IsTrue(LevelTraversal.CanReachObjectives(b),"Ring to goal route "+entry.number);
                }
            } finally {UnityEngine.Object.DestroyImmediate(a);UnityEngine.Object.DestroyImmediate(b);}
        }
    }
    [Test] public void OriginalRepresentativesAndReviewCampaignKeepTheirBytesAndGuids() {
        var baseline=JsonUtility.FromJson<Snapshot>(File.ReadAllText("Docs/Phase3/Verification/RepresentativesBefore.json"));
        using(var sha=SHA256.Create()) foreach(var item in baseline.assets) {
            Assert.AreEqual(item.sha256,BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(item.path))).Replace("-","").ToLowerInvariant(),item.path);
            if(string.IsNullOrEmpty(item.id)) continue;
            Assert.AreEqual(item.guid,AssetDatabase.AssetPathToGUID(item.path));
            Assert.AreEqual(item.id,AssetDatabase.LoadAssetAtPath<LevelData>(item.path).LevelId);
        }
    }
    [Test] public void FullBatchRejectsMissingOrRepeatedCampaignEntries() {
        Assert.Throws<ArgumentException>(()=>ProductionLevelImporter.ValidateFullBatch(Campaign.Levels.Take(49).ToArray()));
        var reordered=Campaign.Levels.ToArray();reordered[49]=reordered[0];
        Assert.Throws<ArgumentException>(()=>ProductionLevelImporter.ValidateFullBatch(reordered));
    }
    [Test] public void UnknownOffGridGeometryIsRejectedWithoutRelaxingTolerance() {
        var source=ProductionLevelImporter.ReadSource(ProductionLevelImporter.FullIntermediate).levels.Single(x=>x.number==2);
        source.walls[0].center+=new Vector3(.1f,0,.1f);
        Assert.Throws<ArgumentException>(()=>ProductionLevelImporter.Normalize(source,null));
    }
}
