using BMPharma.Domain.Enums;

namespace BMPharma.CHIFA.Services;

public class ChifaWorkflowStateMachine
{
    private static readonly Dictionary<ChifaWorkflowState, HashSet<ChifaWorkflowState>> AllowedTransitions = new()
    {
        [ChifaWorkflowState.Draft] = new()
        {
            ChifaWorkflowState.Validated,
            ChifaWorkflowState.Cancelled
        },
        [ChifaWorkflowState.Validated] = new()
        {
            ChifaWorkflowState.PreparedForChifa,
            ChifaWorkflowState.Failed,
            ChifaWorkflowState.Cancelled
        },
        [ChifaWorkflowState.PreparedForChifa] = new()
        {
            ChifaWorkflowState.WrittenToChifa,
            ChifaWorkflowState.Failed,
            ChifaWorkflowState.RollbackRequired
        },
        [ChifaWorkflowState.WrittenToChifa] = new()
        {
            ChifaWorkflowState.VisibleInChifa,
            ChifaWorkflowState.Failed,
            ChifaWorkflowState.RollbackRequired
        },
        [ChifaWorkflowState.VisibleInChifa] = new()
        {
            ChifaWorkflowState.Signed,
            ChifaWorkflowState.Rejected,
            ChifaWorkflowState.Failed
        },
        [ChifaWorkflowState.Signed] = new()
        {
            ChifaWorkflowState.BordereauAssigned,
            ChifaWorkflowState.Failed
        },
        [ChifaWorkflowState.BordereauAssigned] = new()
        {
            ChifaWorkflowState.BordereauClosed,
            ChifaWorkflowState.Rejected,
            ChifaWorkflowState.Failed
        },
        [ChifaWorkflowState.BordereauClosed] = new()
        {
            ChifaWorkflowState.Transmitted,
            ChifaWorkflowState.Failed
        },
        [ChifaWorkflowState.Transmitted] = new(),
        [ChifaWorkflowState.Failed] = new()
        {
            ChifaWorkflowState.Draft
        },
        [ChifaWorkflowState.Rejected] = new()
        {
            ChifaWorkflowState.Draft
        },
        [ChifaWorkflowState.RollbackRequired] = new()
        {
            ChifaWorkflowState.Draft
        },
        [ChifaWorkflowState.Cancelled] = new()
        {
            ChifaWorkflowState.Draft
        }
    };

    public bool CanTransition(ChifaWorkflowState from, ChifaWorkflowState to)
    {
        return AllowedTransitions.TryGetValue(from, out var targets) && targets.Contains(to);
    }

    public ChifaWorkflowState Transition(ChifaWorkflowState current, ChifaWorkflowState target)
    {
        if (!CanTransition(current, target))
        {
            throw new InvalidOperationException(
                $"Invalid state transition from {current} to {target}. " +
                $"Allowed transitions from {current}: [{string.Join(", ", GetAllowedTransitions(current))}]");
        }
        return target;
    }

    public IReadOnlyCollection<ChifaWorkflowState> GetAllowedTransitions(ChifaWorkflowState state)
    {
        return AllowedTransitions.TryGetValue(state, out var targets)
            ? targets
            : Array.Empty<ChifaWorkflowState>();
    }

    public bool IsTerminal(ChifaWorkflowState state)
    {
        return state == ChifaWorkflowState.Transmitted ||
               state == ChifaWorkflowState.Cancelled;
    }

    public bool RequiresHumanAction(ChifaWorkflowState state)
    {
        return state == ChifaWorkflowState.VisibleInChifa ||
               state == ChifaWorkflowState.BordereauAssigned;
    }

    public bool IsSimulatedState(ChifaWorkflowState state)
    {
        return state == ChifaWorkflowState.WrittenToChifa ||
               state == ChifaWorkflowState.VisibleInChifa ||
               state == ChifaWorkflowState.Signed ||
               state == ChifaWorkflowState.BordereauAssigned ||
               state == ChifaWorkflowState.BordereauClosed ||
               state == ChifaWorkflowState.Transmitted;
    }
}
