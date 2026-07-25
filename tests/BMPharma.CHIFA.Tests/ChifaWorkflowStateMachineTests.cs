using FluentAssertions;
using BMPharma.CHIFA.Services;
using BMPharma.Domain.Enums;
using Xunit;

namespace BMPharma.CHIFA.Tests;

public class ChifaWorkflowStateMachineTests
{
    private readonly ChifaWorkflowStateMachine _sm = new();

    [Fact]
    public void SM001_Draft_CanTransitionTo_Validated()
    {
        _sm.CanTransition(ChifaWorkflowState.Draft, ChifaWorkflowState.Validated).Should().BeTrue();
    }

    [Fact]
    public void SM002_Draft_CanTransitionTo_Cancelled()
    {
        _sm.CanTransition(ChifaWorkflowState.Draft, ChifaWorkflowState.Cancelled).Should().BeTrue();
    }

    [Fact]
    public void SM003_Draft_CannotTransitionTo_Signed()
    {
        _sm.CanTransition(ChifaWorkflowState.Draft, ChifaWorkflowState.Signed).Should().BeFalse();
    }

    [Fact]
    public void SM004_Validated_CanTransitionTo_PreparedForChifa()
    {
        _sm.CanTransition(ChifaWorkflowState.Validated, ChifaWorkflowState.PreparedForChifa).Should().BeTrue();
    }

    [Fact]
    public void SM005_PreparedForChifa_CanTransitionTo_WrittenToChifa()
    {
        _sm.CanTransition(ChifaWorkflowState.PreparedForChifa, ChifaWorkflowState.WrittenToChifa).Should().BeTrue();
    }

    [Fact]
    public void SM006_WrittenToChifa_CanTransitionTo_VisibleInChifa()
    {
        _sm.CanTransition(ChifaWorkflowState.WrittenToChifa, ChifaWorkflowState.VisibleInChifa).Should().BeTrue();
    }

    [Fact]
    public void SM007_VisibleInChifa_CanTransitionTo_Signed()
    {
        _sm.CanTransition(ChifaWorkflowState.VisibleInChifa, ChifaWorkflowState.Signed).Should().BeTrue();
    }

    [Fact]
    public void SM008_VisibleInChifa_CanTransitionTo_Rejected()
    {
        _sm.CanTransition(ChifaWorkflowState.VisibleInChifa, ChifaWorkflowState.Rejected).Should().BeTrue();
    }

    [Fact]
    public void SM009_Signed_CanTransitionTo_BordereauAssigned()
    {
        _sm.CanTransition(ChifaWorkflowState.Signed, ChifaWorkflowState.BordereauAssigned).Should().BeTrue();
    }

    [Fact]
    public void SM010_BordereauAssigned_CanTransitionTo_BordereauClosed()
    {
        _sm.CanTransition(ChifaWorkflowState.BordereauAssigned, ChifaWorkflowState.BordereauClosed).Should().BeTrue();
    }

    [Fact]
    public void SM011_BordereauClosed_CanTransitionTo_Transmitted()
    {
        _sm.CanTransition(ChifaWorkflowState.BordereauClosed, ChifaWorkflowState.Transmitted).Should().BeTrue();
    }

    [Fact]
    public void SM012_Transmitted_IsTerminal()
    {
        _sm.IsTerminal(ChifaWorkflowState.Transmitted).Should().BeTrue();
        _sm.GetAllowedTransitions(ChifaWorkflowState.Transmitted).Should().BeEmpty();
    }

    [Fact]
    public void SM013_Failed_CanReturnTo_Draft()
    {
        _sm.CanTransition(ChifaWorkflowState.Failed, ChifaWorkflowState.Draft).Should().BeTrue();
    }

    [Fact]
    public void SM014_Rejected_CanReturnTo_Draft()
    {
        _sm.CanTransition(ChifaWorkflowState.Rejected, ChifaWorkflowState.Draft).Should().BeTrue();
    }

    [Fact]
    public void SM015_RollbackRequired_CanReturnTo_Draft()
    {
        _sm.CanTransition(ChifaWorkflowState.RollbackRequired, ChifaWorkflowState.Draft).Should().BeTrue();
    }

    [Fact]
    public void SM016_VisibleInChifa_RequiresHumanAction()
    {
        _sm.RequiresHumanAction(ChifaWorkflowState.VisibleInChifa).Should().BeTrue();
    }

    [Fact]
    public void SM017_BordereauAssigned_RequiresHumanAction()
    {
        _sm.RequiresHumanAction(ChifaWorkflowState.BordereauAssigned).Should().BeTrue();
    }

    [Fact]
    public void SM018_Draft_DoesNotRequireHumanAction()
    {
        _sm.RequiresHumanAction(ChifaWorkflowState.Draft).Should().BeFalse();
    }

    [Fact]
    public void SM019_Transition_ThrowsOnInvalid()
    {
        var act = () => _sm.Transition(ChifaWorkflowState.Draft, ChifaWorkflowState.Signed);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void SM020_Transition_ReturnsTarget_IfValid()
    {
        var result = _sm.Transition(ChifaWorkflowState.Draft, ChifaWorkflowState.Validated);
        result.Should().Be(ChifaWorkflowState.Validated);
    }

    [Fact]
    public void SM021_AllowedTransitions_Draft_IncludesValidatedAndCancelled()
    {
        var transitions = _sm.GetAllowedTransitions(ChifaWorkflowState.Draft);
        transitions.Should().Contain(ChifaWorkflowState.Validated);
        transitions.Should().Contain(ChifaWorkflowState.Cancelled);
    }

    [Fact]
    public void SM022_Cancelled_IsTerminal()
    {
        _sm.IsTerminal(ChifaWorkflowState.Cancelled).Should().BeTrue();
    }

    [Fact]
    public void SM023_WrittenToChifa_IsSimulatedState()
    {
        _sm.IsSimulatedState(ChifaWorkflowState.WrittenToChifa).Should().BeTrue();
    }

    [Fact]
    public void SM024_Draft_IsNotSimulatedState()
    {
        _sm.IsSimulatedState(ChifaWorkflowState.Draft).Should().BeFalse();
    }

    [Fact]
    public void SM025_PreparedForChifa_CannotSkipTo_Signed()
    {
        _sm.CanTransition(ChifaWorkflowState.PreparedForChifa, ChifaWorkflowState.Signed).Should().BeFalse();
    }
}
