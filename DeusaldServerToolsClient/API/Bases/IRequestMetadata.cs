namespace DeusaldServerToolsClient
{
    public interface IRequestMetadata
    {
        public SendMethodType SendMethod { get; }
        public string         Address    { get; }
    }
}