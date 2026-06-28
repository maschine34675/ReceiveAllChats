using EFT.UI.Chat;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Reflection;
using UnityEngine;

namespace ReceiveAllChats.Patches;

internal sealed class ReceiveAllFromAllChatsPatch : ModulePatch
{
    private static readonly MethodInfo ProcessMessagesMethod =
        AccessTools.Method(typeof(ChatScreen), "method_17");

    private static bool _collecting;

    protected override MethodBase GetTargetMethod() =>
        AccessTools.Method(typeof(ChatScreen), nameof(ChatScreen.method_3));

    [PatchPrefix]
    private static bool Prefix(ChatScreen __instance, SocialNetworkClass ___socialNetworkClass)
    {
        Collect(__instance, ___socialNetworkClass);
        return false;
    }

    private static async void Collect(ChatScreen chatScreen, SocialNetworkClass socialNetwork)
    {
        if (_collecting)
        {
            return;
        }

        _collecting = true;
        try
        {
            var messages = await AttachmentCollector.CollectAllMessages(socialNetwork);
            if (messages.Length > 0)
            {
                ProcessMessagesMethod.Invoke(chatScreen, new object[] { messages });
            }
        }
        catch (Exception exception)
        {
            Debug.LogError($"ReceiveAllChats failed to collect attachments: {exception}");
        }
        finally
        {
            _collecting = false;
        }
    }
}
