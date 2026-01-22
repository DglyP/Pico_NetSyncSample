using UnityEngine;
using UnityEngine.InputSystem;
using Styly.NetSync;

public class NetSyncSampleController : MonoBehaviour
{
    private void Start()
    {
        // 1) Connected + initial sync complete
        NetSyncManager.Instance.OnReady.AddListener(() =>
        {
            Debug.Log("[NetSync] OnReady ✅ Connected + synced");

            // Set a global variable when ready (everyone should receive the change)
            NetSyncManager.Instance.SetGlobalVariable("sharedText", "Hello from OnReady");
        });

        // 2) Receive RPCs
        NetSyncManager.Instance.OnRPCReceived.AddListener((senderClientNo, functionName, args) =>
        {
            Debug.Log($"[NetSync] RPC received from client {senderClientNo} : {functionName}");
            if (args != null)
            {
                foreach (var a in args)
                {
                    Debug.Log($"[NetSync]   arg: {a}");
                }
            }
        });

        // 3) Listen for global variable changes
        NetSyncManager.Instance.OnGlobalVariableChanged.AddListener((name, oldVal, newVal) =>
        {
            Debug.Log($"[NetSync] GlobalVar changed: {name} | {oldVal} -> {newVal}");
        });
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // Space -> broadcast RPC to everyone
        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            NetSyncManager.Instance.Rpc(
                "Ping",
                new string[] { "Hello!", System.DateTime.Now.ToString("HH:mm:ss") }
            );
            Debug.Log("[NetSync] Sent RPC: Ping");
        }

        // G -> update a shared global variable
        if (keyboard.gKey.wasPressedThisFrame)
        {
            var value = "Update @" + System.DateTime.Now.ToString("HH:mm:ss");
            NetSyncManager.Instance.SetGlobalVariable("sharedText", value);
            Debug.Log("[NetSync] SetGlobalVariable sharedText = " + value);
        }

        // R -> read the current shared variable (local read)
        if (keyboard.rKey.wasPressedThisFrame)
        {
            var value = NetSyncManager.Instance.GetGlobalVariable("sharedText", "(not set)");
            Debug.Log("[NetSync] GetGlobalVariable sharedText = " + value);
        }
    }

    private void OnDestroy()
    {
        // Optional: clean up listeners (helps if you reload scenes)
        if (NetSyncManager.Instance == null) return;

        NetSyncManager.Instance.OnReady.RemoveAllListeners();
        NetSyncManager.Instance.OnGlobalVariableChanged.RemoveAllListeners();
        NetSyncManager.Instance.OnRPCReceived.RemoveAllListeners();
    }
}
