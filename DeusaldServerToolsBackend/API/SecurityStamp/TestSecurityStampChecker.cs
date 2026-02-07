namespace DeusaldServerToolsBackend;

public class TestSecurityStampChecker : ISecurityStampChecker
{
    public Task<bool> IsSecurityStampInvalidAsync(Guid securityStamp)
    {
        return Task.FromResult(false);
    }
}