namespace RabanSoft.AsyncSocketNetwork.Example.Utilities.Models {
    /// <summary>
    /// Defines the supported chat protocol functionalities.
    /// </summary>
    public enum OpCodes : byte {
        ConversationJoin = 0xE0,
        ConversationMessage = 0xE1,
    }
}
