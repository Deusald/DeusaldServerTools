using System.Security.Claims;

namespace DeusaldServerToolsBackend;

public static class BaseClaimNames
{
    public const string USER_ID                   = ClaimTypes.NameIdentifier;
    public const string USERNAME                  = "username";
    public const string SECURITY_STAMP            = "security_stamp";
    public const string LOGIN_PROVIDER            = "login_provider";
    public const string LOGIN_PROVIDER_KEY        = "login_provider_key";
    public const string BACKUP_LOGIN_PROVIDER     = "backup_login_provider";
    public const string BACKUP_LOGIN_PROVIDER_KEY = "backup_login_provider_key";
    public const string PLATFORM                  = "platform";
    public const string CLIENT_VERSION            = "client_version";
    public const string LOGIC_VERSION             = "logic_version";
    public const string EXPIRE                    = "exp";
    public const string BACKEND_VERSION           = "backend_version";
}