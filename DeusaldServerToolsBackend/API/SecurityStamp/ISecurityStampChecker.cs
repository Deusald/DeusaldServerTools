namespace DeusaldServerToolsBackend;

public interface ISecurityStampChecker
{
    public Task<bool> IsSecurityStampInvalidAsync(Guid securityStamp);
}