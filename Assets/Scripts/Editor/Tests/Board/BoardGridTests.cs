using System;
using System.Collections.Generic;
using NUnit.Framework;
using SaileachStudios.Mirlini.Board;

public class BoardGridTests
{
    [Test]
    public void EveryEdgeIsUniqueAndRoundTrips() {
        var seen = new HashSet<string>();
        for (int i=0;i<BoardGrid.EdgeCount;i++) {
            var edge=BoardGrid.GetEdge(i);
            Assert.IsTrue(seen.Add($"{edge.Center.X}:{edge.Center.Z}:{edge.AlongZ}"));
            Assert.IsTrue(BoardGrid.TryGetEdgeIndex(edge.Center,edge.AlongZ,out int result));
            Assert.AreEqual(i,result);
        }
        Assert.AreEqual(480,seen.Count);
    }
    [Test]
    public void IndexLandmarksMatchAuthoredOrdering() {
        var first=BoardGrid.GetEdge(0); var secondRow=BoardGrid.GetEdge(15);var last=BoardGrid.GetEdge(479);
        Assert.AreEqual(7,first.Center.X);Assert.AreEqual(7.5f,first.Center.Z);Assert.IsTrue(first.AlongZ);
        Assert.AreEqual(7.5f,secondRow.Center.X);Assert.AreEqual(7,secondRow.Center.Z);Assert.IsFalse(secondRow.AlongZ);
        Assert.AreEqual(-7,last.Center.X);Assert.AreEqual(-7.5f,last.Center.Z);
    }
    [Test]
    public void InvalidIndicesAndOffGridWallsAreRejected() {
        Assert.Throws<ArgumentOutOfRangeException>(()=>BoardGrid.GetEdge(-1));
        Assert.Throws<ArgumentOutOfRangeException>(()=>BoardGrid.GetEdge(480));
        Assert.IsFalse(BoardGrid.TryGetEdgeIndex(new BoardPoint(0,6.05f),true,out _));
        Assert.IsFalse(BoardGrid.TryGetEdgeIndex(new BoardPoint(8,0.5f),true,out _));
    }
    [Test]
    public void HalfUnitPlacementIncludesIntegerAndHalfIntegerPositions() {
        var p=BoardGrid.SnapPlacement(new BoardPoint(1.24f,-1.26f));
        Assert.AreEqual(1,p.X);Assert.AreEqual(-1.5f,p.Z);
        var roundTrip=BoardGrid.ToLogical(BoardGrid.ToWorld(p));
        Assert.AreEqual(p.X,roundTrip.X,0.00001f);Assert.AreEqual(p.Z,roundTrip.Z,0.00001f);
    }
    [Test]
    public void WallSnapReturnsNearestValidEdgeWithoutChangingOrientation() {
        Assert.IsTrue(BoardGrid.TrySnapEdge(new BoardPoint(6.97f,7.48f),true,out int index));
        Assert.AreEqual(0,index);
        Assert.IsTrue(BoardGrid.TrySnapEdge(new BoardPoint(99,99),false,out index));
        Assert.AreEqual(15,index);
        Assert.IsFalse(BoardGrid.TrySnapEdge(new BoardPoint(float.NaN,0),true,out _));
    }
    [Test]
    public void BoundaryInnerFacesAndWallClearanceAreConsistent() {
        Assert.AreEqual(BoardGrid.HalfExtent,BoardGrid.GetBoundary(0).MinX);
        Assert.AreEqual(-BoardGrid.HalfExtent,BoardGrid.GetBoundary(2).MaxX);
        Assert.IsTrue(BoardGrid.IsInside(new BoardPoint(BoardGrid.HalfExtent-.5f,0),.5f));
        Assert.IsFalse(BoardGrid.IsInside(new BoardPoint(BoardGrid.HalfExtent-.49f,0),.5f));
        // Adjacent parallel walls leave a 1.4-unit corridor, enough for a diameter-one marble.
        Assert.AreEqual(1.4f,BoardGrid.CellSize-BoardGrid.WallThickness,.00001f);
    }
}
