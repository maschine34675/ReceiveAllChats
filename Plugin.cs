using BepInEx;
using ReceiveAllChats.Patches;

namespace ReceiveAllChats;

[BepInPlugin("com.maschine.ReceiveAllChats", "maschine-ReceiveAllChats", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    private void Awake()
    {
        new ReceiveAllFromAllChatsPatch().Enable();
        new ReceiveAllButtonVisibilityPatch().Enable();
        new ReceiveAllButtonVisibilityPatch.OnGlobalMessagePatch().Enable();
        new ReceiveAllButtonVisibilityPatch.UpdatePatch().Enable();
        new TransferItemsClosePatch().Enable();
    }
}
