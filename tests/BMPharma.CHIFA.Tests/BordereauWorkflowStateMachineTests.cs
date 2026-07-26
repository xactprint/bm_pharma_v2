using FluentAssertions;
using BMPharma.CHIFA.Services;
using BMPharma.Domain.Enums;
using Xunit;

namespace BMPharma.CHIFA.Tests;

public class BordereauWorkflowStateMachineTests
{
    private readonly BordereauWorkflowStateMachine _sm = new();

    [Fact]
    public void BSM001_Draft_CanTransitionToPreparing()
    {
        _sm.CanTransition(BordereauWorkflowState.Draft, BordereauWorkflowState.Preparing).Should().BeTrue();
    }

    [Fact]
    public void BSM002_Draft_CannotTransitionToCompleted()
    {
        _sm.CanTransition(BordereauWorkflowState.Draft, BordereauWorkflowState.Completed).Should().BeFalse();
    }

    [Fact]
    public void BSM003_Completed_IsTerminal()
    {
        _sm.IsTerminal(BordereauWorkflowState.Completed).Should().BeTrue();
    }

    [Fact]
    public void BSM004_Draft_IsNotTerminal()
    {
        _sm.IsTerminal(BordereauWorkflowState.Draft).Should().BeFalse();
    }

    [Fact]
    public void BSM005_Error_IsErrorState()
    {
        _sm.IsError(BordereauWorkflowState.Error).Should().BeTrue();
        _sm.IsError(BordereauWorkflowState.SyncError).Should().BeTrue();
        _sm.IsError(BordereauWorkflowState.SignatureError).Should().BeTrue();
        _sm.IsError(BordereauWorkflowState.ClosureError).Should().BeTrue();
        _sm.IsError(BordereauWorkflowState.TransmissionError).Should().BeTrue();
    }

    [Fact]
    public void BSM006_Draft_IsNotError()
    {
        _sm.IsError(BordereauWorkflowState.Draft).Should().BeFalse();
    }

    [Fact]
    public void BSM007_AwaitingSignature_RequiresHumanAction()
    {
        _sm.RequiresHumanAction(BordereauWorkflowState.AwaitingSignature).Should().BeTrue();
        _sm.RequiresHumanAction(BordereauWorkflowState.AwaitingClosure).Should().BeTrue();
        _sm.RequiresHumanAction(BordereauWorkflowState.PartiallySigned).Should().BeTrue();
    }

    [Fact]
    public void BSM008_Completed_DoesNotRequireHumanAction()
    {
        _sm.RequiresHumanAction(BordereauWorkflowState.Completed).Should().BeFalse();
    }

    [Fact]
    public void BSM009_FullHappyPath_AllTransitionsValid()
    {
        var states = new[]
        {
            BordereauWorkflowState.Draft,
            BordereauWorkflowState.Preparing,
            BordereauWorkflowState.Created,
            BordereauWorkflowState.InvoicesAttached,
            BordereauWorkflowState.AwaitingSignature,
            BordereauWorkflowState.ReadyForClosure,
            BordereauWorkflowState.AwaitingClosure,
            BordereauWorkflowState.Closed,
            BordereauWorkflowState.AwaitingTransmission,
            BordereauWorkflowState.Transmitted,
            BordereauWorkflowState.Completed
        };

        for (int i = 0; i < states.Length - 1; i++)
        {
            _sm.CanTransition(states[i], states[i + 1]).Should().BeTrue(
                $"Transition from {states[i]} to {states[i + 1]} should be allowed");
        }
    }

    [Fact]
    public void BSM010_InvalidTransition_Throws()
    {
        Action act = () => _sm.Transition(BordereauWorkflowState.Draft, BordereauWorkflowState.Completed);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void BSM011_ValidTransition_ReturnsTarget()
    {
        var result = _sm.Transition(BordereauWorkflowState.Draft, BordereauWorkflowState.Preparing);
        result.Should().Be(BordereauWorkflowState.Preparing);
    }

    [Fact]
    public void BSM012_ErrorStates_CanRecoverToDraft()
    {
        _sm.CanTransition(BordereauWorkflowState.Error, BordereauWorkflowState.Draft).Should().BeTrue();
        _sm.CanTransition(BordereauWorkflowState.SyncError, BordereauWorkflowState.Draft).Should().BeTrue();
        _sm.CanTransition(BordereauWorkflowState.SignatureError, BordereauWorkflowState.Draft).Should().BeTrue();
        _sm.CanTransition(BordereauWorkflowState.ClosureError, BordereauWorkflowState.Draft).Should().BeTrue();
        _sm.CanTransition(BordereauWorkflowState.TransmissionError, BordereauWorkflowState.Draft).Should().BeTrue();
    }

    [Fact]
    public void BSM013_GetAllowedTransitions_ReturnsCorrectSet()
    {
        var allowed = _sm.GetAllowedTransitions(BordereauWorkflowState.Draft);
        allowed.Should().Contain(BordereauWorkflowState.Preparing);
        allowed.Should().Contain(BordereauWorkflowState.Error);
    }

    [Fact]
    public void BSM014_IsSimulatedState_ReturnsTrueForCHIFAStates()
    {
        _sm.IsSimulatedState(BordereauWorkflowState.Created).Should().BeTrue();
        _sm.IsSimulatedState(BordereauWorkflowState.AwaitingSignature).Should().BeTrue();
        _sm.IsSimulatedState(BordereauWorkflowState.Completed).Should().BeTrue();
        _sm.IsSimulatedState(BordereauWorkflowState.Draft).Should().BeFalse();
    }

    [Fact]
    public void BSM015_PartiallySigned_CanGoToReadyForClosure()
    {
        _sm.CanTransition(BordereauWorkflowState.PartiallySigned, BordereauWorkflowState.ReadyForClosure).Should().BeTrue();
    }

    [Fact]
    public void BSM016_PartiallySigned_CanGoBackToAwaitingSignature()
    {
        _sm.CanTransition(BordereauWorkflowState.PartiallySigned, BordereauWorkflowState.AwaitingSignature).Should().BeTrue();
    }

    [Fact]
    public void BSM017_GetActionDescription_ReturnsNonEmptyForAllStates()
    {
        foreach (BordereauWorkflowState state in Enum.GetValues<BordereauWorkflowState>())
        {
            var desc = _sm.GetActionDescription(state, "TEST");
            desc.Should().NotBeNullOrEmpty($"Description for {state} should not be empty");
        }
    }

    [Fact]
    public void BSM018_AwaitingSignature_ActionApplication_IsCHIFA()
    {
        _sm.GetActionApplication(BordereauWorkflowState.AwaitingSignature).Should().Be("CHIFA-OFFICINE");
        _sm.GetActionApplication(BordereauWorkflowState.AwaitingClosure).Should().Be("CHIFA-OFFICINE");
    }

    [Fact]
    public void BSM019_Completed_ActionBmPharmaWaits_IsEmpty()
    {
        _sm.GetActionBmPharmaWaits(BordereauWorkflowState.Completed).Should().Be("Aucune attente. Le bordereau est terminé.");
    }
}
