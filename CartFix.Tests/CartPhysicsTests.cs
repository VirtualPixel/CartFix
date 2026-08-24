using CartFix.Services;
using UnityEngine;
using Xunit;

namespace CartFix.Tests;

public class CartPhysicsTests
{
    [Fact]
    public void SteerMassScalesWithPayload()
    {
        Assert.Equal(24f, CartPhysics.OverrideMass(CartPhysics.SteerMass, 10f, 2f), 3);
    }

    [Fact]
    public void LockedSmallCartScalesWithPayload()
    {
        Assert.Equal(28f, CartPhysics.OverrideMass(CartPhysics.LockedSmallCartMass, 10f, 2f), 3);
    }

    [Fact]
    public void EmptyCartStaysVanilla()
    {
        Assert.Equal(4f, CartPhysics.OverrideMass(CartPhysics.SteerMass, 0f, 2f), 3);
    }

    [Fact]
    public void FactorZeroIsVanilla()
    {
        Assert.Equal(4f, CartPhysics.OverrideMass(CartPhysics.SteerMass, 35f, 0f), 3);
    }

    // If the game (or another mod) starts passing something other than the flat 4 / 8
    // this build was tuned against, the safe move is to leave it alone rather than
    // stack our payload term on top of whatever they did.
    [Fact]
    public void LeavesUnknownVanillaMassAlone()
    {
        Assert.Equal(5f, CartPhysics.OverrideMass(5f, 10f, 2f), 3);
        Assert.Equal(4.5f, CartPhysics.OverrideMass(4.5f, 10f, 2f), 3);
    }

    [Fact]
    public void CartVelocityAtIncludesSpin()
    {
        var v = CartPhysics.CartVelocityAt(new Vector3(1f, 0f, 0f), new Vector3(0f, 1f, 0f), new Vector3(0f, 0f, 1f));
        Assert.Equal(2f, v.x, 3);
        Assert.Equal(0f, v.y, 3);
        Assert.Equal(0f, v.z, 3);
    }

    [Fact]
    public void ThrownItemIsStillInFlight()
    {
        Assert.True(CartPhysics.StillInFlight(new Vector3(3f, 0f, 0f), Vector3.zero));
        Assert.False(CartPhysics.StillInFlight(new Vector3(0.5f, 0f, 0f), Vector3.zero));
    }

    [Fact]
    public void PullMovesThirtyPercentPerTick()
    {
        var v = CartPhysics.PullTowardCart(Vector3.zero, new Vector3(10f, 0f, 0f), 0.02f);
        Assert.Equal(3f, v.x, 3);
    }

    [Fact]
    public void PullNeverAddsUpwardVelocity()
    {
        var v = CartPhysics.PullTowardCart(new Vector3(0f, -1f, 0f), new Vector3(0f, 1f, 0f), 0.02f);
        Assert.Equal(-1f, v.y, 3);
    }
}
