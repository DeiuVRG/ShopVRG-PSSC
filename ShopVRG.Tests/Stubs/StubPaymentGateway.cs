namespace ShopVRG.Tests.Stubs;

/// <summary>
/// Stub for payment gateway simulation
/// Always succeeds unless configured to fail
/// </summary>
public class StubPaymentGateway
{
    private bool _shouldFail;
    private string _failureReason = "";

    public void ConfigureSuccess()
    {
        _shouldFail = false;
        _failureReason = "";
    }

    public void ConfigureFailure(string reason = "Payment declined")
    {
        _shouldFail = true;
        _failureReason = reason;
    }

    public (bool Success, string TransactionReference, string? Error) ProcessPayment(
        string cardNumber,
        string cardHolderName,
        decimal amount)
    {
        if (_shouldFail)
        {
            return (false, "", _failureReason);
        }

        // Simulate card validation - cards ending in 0000 are declined
        if (cardNumber.EndsWith("0000"))
        {
            return (false, "", "Card declined by issuer");
        }

        // Generate transaction reference
        var transactionRef = $"TXN-{Guid.NewGuid():N}".ToUpper()[..20];
        return (true, transactionRef, null);
    }
}
