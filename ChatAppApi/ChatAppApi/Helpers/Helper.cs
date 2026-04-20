using ChatAppApi.Models;

namespace ChatAppApi.Helpers;

public static class Helper
{
    /// <summary>
    /// Validates the message format.
    /// </summary>
    public static bool IsValidMessage(Message message)
    {
        return message != null &&
               !string.IsNullOrEmpty(message.Type) &&
               new[] { "connect", "chat", "typing", "error" }.Contains(message.Type) &&
               !string.IsNullOrEmpty(message.SenderId) &&
               !string.IsNullOrEmpty(message.ReceiverId) &&
               !string.IsNullOrEmpty(message.Data);
    }

}

