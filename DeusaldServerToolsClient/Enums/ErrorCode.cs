using DeusaldSharp;

namespace DeusaldServerToolsClient
{
    [SerializableEnum(SerializableEnumType.Byte)]
    public enum ErrorCode : byte
    {
        None                   = 0,
        Success                = 1,
        HttpError              = 2,
        CantDeserializeRequest = 3,
        ServerInternalError    = 4,
        WrongRequestId         = 5,
        DataVerification       = 6, // Data Verification error have specially prepared message that can be used to display more detailed error info to user
        ClientError            = 7,
        MaintenanceModeIsOn    = 8,
        Unauthorized           = 9,
        CustomError            = 10,
        ClientUnsupported      = 11
    }
}