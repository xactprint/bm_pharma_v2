using System.Net.Http;

namespace BMPharma.CHIFA.Services;

public static class ChifaExceptionMapper
{
    public static string ToUserMessage(this Exception ex)
    {
        return ex switch
        {
            HttpRequestException httpEx => httpEx.StatusCode.HasValue
                ? $"Erreur réseau CHIFA (HTTP {(int)httpEx.StatusCode}): {httpEx.Message}"
                : $"Erreur réseau CHIFA: {httpEx.Message}",
            TaskCanceledException => "La demande a expiré. Vérifiez la connexion CHIFA-OFFICINE.",
            TimeoutException => "La demande a expiré. Vérifiez la connexion réseau.",
            OperationCanceledException => "L'opération a été annulée.",
            InvalidOperationException invOpEx => invOpEx.Message.Contains("token", StringComparison.OrdinalIgnoreCase)
                ? "Erreur de token: l'opération n'est pas autorisée dans l'état actuel."
                : $"Opération invalide: {invOpEx.Message}",
            UnauthorizedAccessException => "Accès refusé. Vérifiez les permissions du système.",
            _ => ex.InnerException != null
                ? ToUserMessage(ex.InnerException)
                : ex.Message
        };
    }

    public static string GetErrorCode(this Exception ex)
    {
        return ex switch
        {
            HttpRequestException => "HTTP_ERROR",
            TaskCanceledException => "TIMEOUT",
            TimeoutException => "TIMEOUT",
            OperationCanceledException => "CANCELED",
            InvalidOperationException => "INVALID_OPERATION",
            UnauthorizedAccessException => "ACCESS_DENIED",
            _ => "SYSTEM_ERROR"
        };
    }
}
