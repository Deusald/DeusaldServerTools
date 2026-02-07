using System;

namespace DeusaldServerToolsClient
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class HubMsgAttribute : Attribute
    {
        public string MsgId { get; }

        public HubMsgAttribute(Type msgType)
        {
            MsgId = RequestData.GetHubMsg(msgType);
        }
    }
}