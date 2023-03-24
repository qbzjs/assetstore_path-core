using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using KinematicCharacterController;

public class PlayerCharacterNetworkController : NetworkBehaviour
{
    [Header("Server Values")] [SerializeField] [SyncVar(hook = nameof(ServerPositionChanged))]
    public Vector3 ServerPosition;

    [SerializeField] [SyncVar(hook = nameof(ServerRotationChanged))]
    public Quaternion ServerRotation;

    [SerializeField] [SyncVar(hook = nameof(ServerScaleChanged))]
    public Vector3 ServerScale;

    [SerializeField] public Vector3 lastSentPosition;
    [SerializeField] public Quaternion lastSentRotation;
    [SerializeField] public Vector3 lastSentScale;

    [SerializeField] private KinematicCharacterMotor Motor;
    [SerializeField] private Transform MeshRoot;
    
    private void ServerScaleChanged(Vector3 oldValue, Vector3 newValue)
    {
        Debug.Log("i am the server and have allowed this change of : scale \nthe old position value: " + oldValue +
                  " the new poisition value: " + newValue);
    }

    private void ServerRotationChanged(Quaternion oldValue, Quaternion newValue)
    {
        //Debug.Log("i am the server and have allowed this change of : rotation \nthe old position value: " + oldValue +
        //        " the new poisition value: " + newValue);
    }

    private void ServerPositionChanged(Vector3 oldValue, Vector3 newValue)
    {
        //Debug.Log("i am the server and have allowed this change of : movement\nthe old position value: " + oldValue +
        //      " the new poisition value: " + newValue);
    }

    #region Commands

    #endregion

    [Command]
    public void Cmd_ServerUpdatePositionAndRotation(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        ServerPosition = position;
        ServerRotation = rotation;
        ServerScale = scale;

        RpcCmd_ServerBroadcastUpdatedPositionRotationAndScale(ServerPosition, ServerRotation, ServerScale);
    }


    #region ClientRPC_Commands

    [ClientRpc]
    public void RpcCmd_ServerBroadcastUpdatedPositionRotationAndScale(Vector3 newPosition, Quaternion newRotation,
        Vector3 newScale)
    {
        if (!isLocalPlayer)
        {
            // Update the position and rotation on non-local players
            Motor.SetPosition(newPosition, false);
            Motor.SetRotation(newRotation, false);
            MeshRoot.localScale = newScale;

            Debug.Log("Updated the players position,rotation and scale on the server.");
        }
    }

    #endregion
}