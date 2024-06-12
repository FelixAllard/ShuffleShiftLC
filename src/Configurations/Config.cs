using System;
using BepInEx.Configuration;
using Unity.Collections;
using Unity.Netcode;

namespace ShuffleShift.Configurations;

[Serializable]
public class Config : SyncedInstance<Config>
{ 
    public ConfigEntry<bool> ENABLE_POSITION_SWAP { get; private set; } 
    public ConfigEntry<float> TIME_BETWEEN_SWAP { get; private set; } 
    public ConfigEntry<float> TIME_BEFORE_FIRST_SWAP { get; private set; } 
    public ConfigEntry<int> CHANCE_FOR_SWAP_TO_HAPPEN { get; private set; } 
    public ConfigEntry<bool> ENABLE_SCREEN_SHAKE { get; private set; } 

    
    public Config(ConfigFile cfg)
    {
        InitInstance(this);
        ENABLE_POSITION_SWAP = cfg.Bind("Position Swap", "Enable position swap", true,
            "Set to true if you want the position swap to possibly happen"
        );
        TIME_BETWEEN_SWAP = cfg.Bind("Position Swap", "Time Between each swap", 45f,
            "How much time between each position swap of the players!"
        );
        TIME_BEFORE_FIRST_SWAP = cfg.Bind("Position Swap", "Time before the first swap", 30f,
            "Time before the first swap occurs"
        );
        CHANCE_FOR_SWAP_TO_HAPPEN = cfg.Bind("Position Swap", "Chance of the swap happening", 100,
            "Chance in percentage for the position swap to happen. Must not be lower than 0"
            );
        ENABLE_SCREEN_SHAKE = cfg.Bind("Position Swap", "Make the screen shake when teleporting", true,
            "true the screen shake will be, false, it will not"
        );

    }
    public static void RequestSync() {
        if (!IsClient) return;

        using FastBufferWriter stream = new(IntSize, Allocator.Temp);
        MessageManager.SendNamedMessage("Xilef992SwapShift_OnRequestConfigSync", 0uL, stream);
    }
    public static void OnRequestSync(ulong clientId, FastBufferReader _) {
        if (!IsHost) return;

        Plugin.Logger.LogInfo($"Config sync request received from client: {clientId}");

        byte[] array = SerializeToBytes(Instance);
        int value = array.Length;

        using FastBufferWriter stream = new(value + IntSize, Allocator.Temp);

        try {
            stream.WriteValueSafe(in value, default);
            stream.WriteBytesSafe(array);

            MessageManager.SendNamedMessage("Xilef992SwapShift_OnReceiveConfigSync", clientId, stream);
        } catch(Exception e) {
            Plugin.Logger.LogInfo($"Error occurred syncing config with client: {clientId}\n{e}");
        }
    }
    public static void OnReceiveSync(ulong _, FastBufferReader reader) {
        if (!reader.TryBeginRead(IntSize)) {
            Plugin.Logger.LogError("Config sync error: Could not begin reading buffer.");
            return;
        }

        reader.ReadValueSafe(out int val, default);
        if (!reader.TryBeginRead(val)) {
            Plugin.Logger.LogError("Config sync error: Host could not sync.");
            return;
        }

        byte[] data = new byte[val];
        reader.ReadBytesSafe(ref data, val);

        SyncInstance(data);

        Plugin.Logger.LogInfo("Successfully synced config with host.");
    }
    
}