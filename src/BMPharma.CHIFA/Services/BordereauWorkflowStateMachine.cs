using BMPharma.Domain.Enums;

namespace BMPharma.CHIFA.Services;

public class BordereauWorkflowStateMachine
{
    private static readonly Dictionary<BordereauWorkflowState, HashSet<BordereauWorkflowState>> AllowedTransitions = new()
    {
        [BordereauWorkflowState.Draft] = new()
        {
            BordereauWorkflowState.Preparing,
            BordereauWorkflowState.Error
        },
        [BordereauWorkflowState.Preparing] = new()
        {
            BordereauWorkflowState.Created,
            BordereauWorkflowState.Error,
            BordereauWorkflowState.Draft
        },
        [BordereauWorkflowState.Created] = new()
        {
            BordereauWorkflowState.InvoicesAttached,
            BordereauWorkflowState.Error,
            BordereauWorkflowState.Draft
        },
        [BordereauWorkflowState.InvoicesAttached] = new()
        {
            BordereauWorkflowState.AwaitingSignature,
            BordereauWorkflowState.Error,
            BordereauWorkflowState.Draft
        },
        [BordereauWorkflowState.AwaitingSignature] = new()
        {
            BordereauWorkflowState.PartiallySigned,
            BordereauWorkflowState.ReadyForClosure,
            BordereauWorkflowState.SignatureError,
            BordereauWorkflowState.Error
        },
        [BordereauWorkflowState.PartiallySigned] = new()
        {
            BordereauWorkflowState.ReadyForClosure,
            BordereauWorkflowState.AwaitingSignature,
            BordereauWorkflowState.SignatureError,
            BordereauWorkflowState.Error
        },
        [BordereauWorkflowState.ReadyForClosure] = new()
        {
            BordereauWorkflowState.AwaitingClosure,
            BordereauWorkflowState.Error
        },
        [BordereauWorkflowState.AwaitingClosure] = new()
        {
            BordereauWorkflowState.Closed,
            BordereauWorkflowState.ClosureError,
            BordereauWorkflowState.Error
        },
        [BordereauWorkflowState.Closed] = new()
        {
            BordereauWorkflowState.AwaitingTransmission,
            BordereauWorkflowState.Error
        },
        [BordereauWorkflowState.AwaitingTransmission] = new()
        {
            BordereauWorkflowState.Transmitted,
            BordereauWorkflowState.TransmissionError,
            BordereauWorkflowState.Error
        },
        [BordereauWorkflowState.Transmitted] = new()
        {
            BordereauWorkflowState.Completed
        },
        [BordereauWorkflowState.Completed] = new(),
        [BordereauWorkflowState.Error] = new()
        {
            BordereauWorkflowState.Draft,
            BordereauWorkflowState.Preparing,
            BordereauWorkflowState.SyncError
        },
        [BordereauWorkflowState.SyncError] = new()
        {
            BordereauWorkflowState.Draft,
            BordereauWorkflowState.AwaitingSignature
        },
        [BordereauWorkflowState.SignatureError] = new()
        {
            BordereauWorkflowState.Draft,
            BordereauWorkflowState.AwaitingSignature
        },
        [BordereauWorkflowState.ClosureError] = new()
        {
            BordereauWorkflowState.Draft,
            BordereauWorkflowState.AwaitingClosure
        },
        [BordereauWorkflowState.TransmissionError] = new()
        {
            BordereauWorkflowState.Draft,
            BordereauWorkflowState.AwaitingTransmission
        }
    };

    public bool CanTransition(BordereauWorkflowState from, BordereauWorkflowState to)
    {
        return AllowedTransitions.TryGetValue(from, out var targets) && targets.Contains(to);
    }

    public BordereauWorkflowState Transition(BordereauWorkflowState current, BordereauWorkflowState target)
    {
        if (!CanTransition(current, target))
        {
            throw new InvalidOperationException(
                $"Invalid bordereau state transition from {current} to {target}. " +
                $"Allowed transitions from {current}: [{string.Join(", ", GetAllowedTransitions(current))}]");
        }
        return target;
    }

    public IReadOnlyCollection<BordereauWorkflowState> GetAllowedTransitions(BordereauWorkflowState state)
    {
        return AllowedTransitions.TryGetValue(state, out var targets)
            ? targets
            : Array.Empty<BordereauWorkflowState>();
    }

    public bool IsTerminal(BordereauWorkflowState state)
    {
        return state == BordereauWorkflowState.Completed;
    }

    public bool IsError(BordereauWorkflowState state)
    {
        return state == BordereauWorkflowState.Error ||
               state == BordereauWorkflowState.SyncError ||
               state == BordereauWorkflowState.SignatureError ||
               state == BordereauWorkflowState.ClosureError ||
               state == BordereauWorkflowState.TransmissionError;
    }

    public bool RequiresHumanAction(BordereauWorkflowState state)
    {
        return state == BordereauWorkflowState.AwaitingSignature ||
               state == BordereauWorkflowState.AwaitingClosure ||
               state == BordereauWorkflowState.PartiallySigned;
    }

    public bool IsSimulatedState(BordereauWorkflowState state)
    {
        return state == BordereauWorkflowState.Created ||
               state == BordereauWorkflowState.InvoicesAttached ||
               state == BordereauWorkflowState.AwaitingSignature ||
               state == BordereauWorkflowState.PartiallySigned ||
               state == BordereauWorkflowState.ReadyForClosure ||
               state == BordereauWorkflowState.AwaitingClosure ||
               state == BordereauWorkflowState.Closed ||
               state == BordereauWorkflowState.AwaitingTransmission ||
               state == BordereauWorkflowState.Transmitted ||
               state == BordereauWorkflowState.Completed;
    }

    public string GetActionDescription(BordereauWorkflowState state, string? numBord = null)
    {
        var bord = numBord ?? "ce bordereau";
        return state switch
        {
            BordereauWorkflowState.Draft => $"Le bordereau {bord} est en cours de préparation.",
            BordereauWorkflowState.Preparing => $"Préparation du bordereau {bord} en cours.",
            BordereauWorkflowState.Created => $"Le bordereau {bord} a été créé. Attente de l'association des factures.",
            BordereauWorkflowState.InvoicesAttached => $"Les factures sont associées au bordereau {bord}. Validation requise.",
            BordereauWorkflowState.AwaitingSignature => $"Signature requise pour {bord}. Ouvrez CHIFA-OFFICINE et signez avec le token professionnel.",
            BordereauWorkflowState.PartiallySigned => $"Signature partielle pour {bord}. Certaines factures nécessitent une signature.",
            BordereauWorkflowState.ReadyForClosure => $"Le bordereau {bord} est prêt à être clôturé dans CHIFA-OFFICINE.",
            BordereauWorkflowState.AwaitingClosure => $"Clôture requise pour {bord}. Ouvrez CHIFA-OFFICINE pour effectuer la clôture réglementaire.",
            BordereauWorkflowState.Closed => $"Le bordereau {bord} est clôturé. Transmission CNAS en attente.",
            BordereauWorkflowState.AwaitingTransmission => $"Transmission CNAS requise pour {bord}.",
            BordereauWorkflowState.Transmitted => $"Le bordereau {bord} a été transmis à la CNAS.",
            BordereauWorkflowState.Completed => $"Le bordereau {bord} est terminé. Aucune action requise.",
            BordereauWorkflowState.Error => $"Une erreur empêche la progression du bordereau {bord}.",
            BordereauWorkflowState.SyncError => $"Erreur de synchronisation pour {bord}. Vérifiez la connexion PostgreSQL.",
            BordereauWorkflowState.SignatureError => $"Erreur de signature pour {bord}. Vérifiez le token et réessayez.",
            BordereauWorkflowState.ClosureError => $"Erreur de clôture pour {bord}. Le bordereau doit être signé avant la clôture.",
            BordereauWorkflowState.TransmissionError => $"Erreur de transmission pour {bord}. Vérifiez la connexion CNAS.",
            _ => $"État inconnu pour {bord}."
        };
    }

    public string GetActionApplication(BordereauWorkflowState state)
    {
        return state switch
        {
            BordereauWorkflowState.AwaitingSignature => "CHIFA-OFFICINE",
            BordereauWorkflowState.PartiallySigned => "CHIFA-OFFICINE",
            BordereauWorkflowState.AwaitingClosure => "CHIFA-OFFICINE",
            BordereauWorkflowState.AwaitingTransmission => "CNAS",
            _ => "BM Pharma"
        };
    }

    public string GetActionBmPharmaWaits(BordereauWorkflowState state)
    {
        return state switch
        {
            BordereauWorkflowState.AwaitingSignature => "BM Pharma attend que la signature soit effectuée dans CHIFA-OFFICINE.",
            BordereauWorkflowState.PartiallySigned => "BM Pharma attend que toutes les factures soient signées.",
            BordereauWorkflowState.AwaitingClosure => "BM Pharma attend que la clôture soit effectuée dans CHIFA-OFFICINE.",
            BordereauWorkflowState.AwaitingTransmission => "BM Pharma attend la confirmation de transmission à la CNAS.",
            BordereauWorkflowState.Transmitted => "BM Pharma a confirmé la transmission. Aucune attente.",
            BordereauWorkflowState.Completed => "Aucune attente. Le bordereau est terminé.",
            _ => ""
        };
    }
}
