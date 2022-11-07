using System;
using System.Web.UI.Design.WebControls;

namespace FUnreal
{
    public class FUnrealNotifier
    {
        public enum MessageType { INFO, WARN, ERRO }

        public Action<MessageType, string, string> OnSendMessage;

        public void Info(string contextMsg, string detailedMsgFormat = null, params string[] args)
        {
            SendMessage(MessageType.INFO, contextMsg, detailedMsgFormat, args);
        }

        public void Warn(string contextMsg, string detailedFormat = null, params string[] args)
        {
            SendMessage(MessageType.WARN, contextMsg, detailedFormat, args);
        }

        public void Erro(string contextMsg, string detailedFormat = null, params string[] args)
        {
            SendMessage(MessageType.ERRO, contextMsg, detailedFormat, args);
        }

        private void SendMessage(MessageType type, string context, string detailedFormat = null, params string[] args)
        {
            string detailed = detailedFormat == null ? context : string.Format(detailedFormat, args);
            OnSendMessage?.Invoke(type, context, detailed);
        }
    }
}