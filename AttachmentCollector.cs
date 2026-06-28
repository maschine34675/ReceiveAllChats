using ChatShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReceiveAllChats;

internal static class AttachmentCollector
{
    public static bool HasPendingAttachments(SocialNetworkClass socialNetwork) =>
        socialNetwork.Dialogues.Any(d =>
            d.Type != EMessageType.GlobalChat && (d.AttachmentsNew > 0 || d.HasMessagesWithRewards));

    public static async Task<ChatMessageClass[]> CollectAllMessages(SocialNetworkClass socialNetwork)
    {
        for (var i = 0; i < 50 && !socialNetwork.FullyRead; i++)
        {
            while (socialNetwork.MessageListUpdating)
            {
                await Task.Yield();
            }

            await RequestNextDialoguePage(socialNetwork);
        }

        var dialogues = socialNetwork.Dialogues
            .Where(d => d.Type != EMessageType.GlobalChat && (d.AttachmentsNew > 0 || d.HasMessagesWithRewards))
            .ToList();

        if (dialogues.Count == 0)
        {
            return Array.Empty<ChatMessageClass>();
        }

        var results = await Task.WhenAll(dialogues.Select(d => FetchDialogAttachments(socialNetwork, d._id)));
        var allMessages = new List<ChatMessageClass>();
        foreach (var messages in results)
        {
            if (messages.Length > 0)
            {
                allMessages.AddRange(messages);
            }
        }

        return allMessages.ToArray();
    }

    private static Task RequestNextDialoguePage(SocialNetworkClass socialNetwork)
    {
        var tcs = new TaskCompletionSource<bool>();
        socialNetwork.UpdateDialogueList(() => tcs.TrySetResult(true));
        return tcs.Task;
    }

    private static Task<ChatMessageClass[]> FetchDialogAttachments(SocialNetworkClass socialNetwork, string dialogId)
    {
        var tcs = new TaskCompletionSource<ChatMessageClass[]>();
        socialNetwork.AllAttachmentsFromDialog(dialogId, messages => tcs.TrySetResult(messages ?? Array.Empty<ChatMessageClass>()));
        return tcs.Task;
    }
}
