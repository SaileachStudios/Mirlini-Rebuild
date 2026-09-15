using NUnit.Framework;
using SaileachStudios.Mirlini.Board;

public class HelpPositionPlannerTests
{
    private static readonly BoardRectangle[] Wall = { new BoardRectangle(-.2f,-3,.2f,3) };
    [Test]
    public void SafePositionDoesNotMove() {
        Assert.IsTrue(HelpPositionPlanner.TryFind(new BoardPoint(2,0),.5f,.05f,3,Wall,out var p));
        Assert.AreEqual(2,p.X);Assert.AreEqual(0,p.Z);
    }
    [Test]
    public void ObstructedPositionMovesOnlyToNearestClearanceFace() {
        Assert.IsTrue(HelpPositionPlanner.TryFind(new BoardPoint(.4f,0),.5f,.05f,3,Wall,out var p));
        Assert.AreEqual(.75f,p.X,.00001f);Assert.AreEqual(0,p.Z);
    }
    [Test]
    public void TiesAreDeterministicAndIndependentOfObstacleOrder() {
        var a=new[]{Wall[0],new BoardRectangle(4,4,5,5)};
        var b=new[]{a[1],a[0]};
        Assert.IsTrue(HelpPositionPlanner.TryFind(new BoardPoint(0,0),.5f,.05f,3,a,out var p));
        Assert.IsTrue(HelpPositionPlanner.TryFind(new BoardPoint(0,0),.5f,.05f,3,b,out var q));
        Assert.AreEqual(-.75f,p.X,.00001f);Assert.AreEqual(p.X,q.X);Assert.AreEqual(p.Z,q.Z);
    }
    [Test]
    public void CornerAndBoundaryBothConstrainSolution() {
        var obstacles=new[]{new BoardRectangle(13,-1,13.4f,1)};
        Assert.IsTrue(HelpPositionPlanner.TryFind(new BoardPoint(14.1f,0),.5f,.05f,3,obstacles,out var p));
        Assert.IsTrue(BoardGrid.IsInside(p,.55f));
        Assert.IsFalse(obstacles[0].Expanded(.55f).ContainsInterior(p));
    }
    [Test]
    public void NoNearbySpaceFailsWithoutReturningStartPosition() {
        Assert.IsFalse(HelpPositionPlanner.TryFind(new BoardPoint(0,0),.5f,.05f,.2f,Wall,out var p));
        Assert.AreEqual(0,p.X);Assert.AreEqual(0,p.Z);
    }
    [Test]
    public void FullyBlockedBoardFails() {
        Assert.IsFalse(HelpPositionPlanner.TryFind(new BoardPoint(0,0),.5f,.05f,100,
            new[]{new BoardRectangle(-20,-20,20,20)},out _));
    }
    [Test]
    public void LargerMarbleRequiresLargerClearance() {
        HelpPositionPlanner.TryFind(new BoardPoint(.4f,0),1f,.1f,3,Wall,out var p);
        Assert.AreEqual(1.3f,p.X,.00001f);
    }
    [Test]
    public void InvalidInputsAreRejected() {
        Assert.IsFalse(HelpPositionPlanner.TryFind(new BoardPoint(float.NaN,0),.5f,.05f,3,Wall,out _));
        Assert.IsFalse(HelpPositionPlanner.TryFind(new BoardPoint(0,0),-.5f,.05f,3,Wall,out _));
        Assert.IsFalse(HelpPositionPlanner.TryFind(new BoardPoint(0,0),.5f,-.05f,3,Wall,out _));
    }
}
