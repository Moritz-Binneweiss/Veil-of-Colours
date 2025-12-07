using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class ColorChange : NetworkBehaviour
{
    [Header("Farblayer GameObjects")]
    [SerializeField]
    private Tilemap Farblayer1;

    [SerializeField]
    private Tilemap Farblayer2;

    [SerializeField]
    private Tilemap Farblayer3;

    [Header("Input Actions")]
    [SerializeField]
    private InputActionReference Color1;

    [SerializeField]
    private InputActionReference Color2;

    [SerializeField]
    private InputActionReference Color3;

    void Update()
    {
        if (!IsOwner)
            return;

        ReadInput();
    }

    public void switchFarblayer() { }

    private void ReadInput()
    {
        // Prüfe Input für Color1 (Taste 1)
        if (Color1.action.WasPressedThisFrame())
        {
            ActivateFarblayer1();
        }

        // Prüfe Input für Color2 (Taste 2)
        if (Color2.action.WasPressedThisFrame())
        {
            ActivateFarblayer2();
        }

        if (Color3.action.WasPressedThisFrame())
        {
            ToggleFarblayer3ForOtherPlayerServerRpc();
        }
    }

    [ServerRpc]
    private void ToggleFarblayer3ForOtherPlayerServerRpc(ServerRpcParams rpcParams = default)
    {
        // An alle anderen Clients senden (außer Sender)
        ulong senderClientId = rpcParams.Receive.SenderClientId;

        // ClientRpc an alle außer Sender
        ToggleFarblayer3ClientRpc(senderClientId);

        Debug.Log($"Player {senderClientId} triggered Farblayer3 toggle for other players");
    }

    [ClientRpc]
    private void ToggleFarblayer3ClientRpc(ulong senderClientId)
    {
        // Nur ausführen wenn nicht der Sender
        if (NetworkManager.Singleton.LocalClientId == senderClientId)
            return;

        if (Farblayer3 != null)
        {
            bool isActive = Farblayer3.gameObject.activeSelf;
            Farblayer3.gameObject.SetActive(!isActive);
            Debug.Log(
                $"Farblayer3 {(!isActive ? "activated" : "deactivated")} by Player {senderClientId}!"
            );
        }
    }

    private void ActivateFarblayer1()
    {
        if (Farblayer1 != null && Farblayer2 != null)
        {
            Farblayer1.gameObject.SetActive(true);
            Farblayer2.gameObject.SetActive(false);
            Debug.Log("Farblayer1 activated, Farblayer2 deactivated!");
        }
    }

    private void ActivateFarblayer2()
    {
        if (Farblayer1 != null && Farblayer2 != null)
        {
            Farblayer1.gameObject.SetActive(false);
            Farblayer2.gameObject.SetActive(true);
            Debug.Log("Farblayer2 activated, Farblayer1 deactivated!");
        }
    }

    void OnEnable()
    {
        Color1.action.Enable();
        Color2.action.Enable();
        Color3.action.Enable();
    }

    void OnDisable()
    {
        Color1.action.Disable();
        Color2.action.Disable();
        Color3.action.Disable();
    }
}
