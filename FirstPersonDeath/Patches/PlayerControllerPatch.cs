﻿using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;
using static UnityEngine.Rendering.DebugUI;

namespace FirstPersonDeath.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerPatch
    {
        public static PlayerControllerB RoundController;
        public static PlayerControllerB NetworkController;

        public static GameObject MeshModel;
        public static Rigidbody[] BodyParts;
        public static DeadBodyInfo[] DeadMesh;

        public static GameObject MainCamera;
        public static GameObject PivotCamera;   
        public static GameObject CameraHolder;
        public static GameObject SpectateCamera;
        public static AudioListener AudioListener;

        public static int ClientId;
        public static string PlayerUsername;

        public static bool PlayerBody = true;
        public static bool PlayerUnderwater = false;

        //[HarmonyPatch(typeof(PlayerControllerB), "KillPlayer"]
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        public static void FirstPersonPatch()
        {
            if (!PlayerBody)
            {
                return;
            }

            if (GameNetworkManager.Instance.localPlayerController != null && GameNetworkManager.Instance.localPlayerController != null)
            {
                RoundController = StartOfRound.Instance.localPlayerController;
                NetworkController = GameNetworkManager.Instance.localPlayerController;

                PivotCamera = RoundController.spectateCameraPivot.gameObject;
                SpectateCamera = StartOfRound.Instance.spectateCamera.gameObject;

                PlayerUsername = NetworkController.playerUsername;
                ClientId = (int)NetworkController.playerClientId;

                MeshModel = NetworkController.thisPlayerModel.gameObject;

                if (!NetworkController.isPlayerDead)
                {
                    PlayerUnderwater = NetworkController.isUnderwater;
                }

                if (NetworkController.isPlayerDead)
                {
                    if (!AudioListener)
                    {
                        AudioListener = GameObject.FindObjectOfType<AudioListener>();
                    }

                    if (!CameraHolder)
                    {
                        DeadMesh = UnityEngine.Object.FindObjectsOfType<DeadBodyInfo>();

                        foreach (DeadBodyInfo DeadBodyInfo in DeadMesh)
                        {
                            if (DeadBodyInfo.playerObjectId == ClientId)
                            {
                                BodyParts = DeadBodyInfo.bodyParts;

                                foreach (Rigidbody Rigidbody in BodyParts)
                                {
                                    if (Rigidbody.name == "spine.004")
                                    {
                                        CameraHolder = Rigidbody.gameObject;
                                    }
                                }
                            }
                        }
                    }

                    if (StartOfRound.Instance.shipIsLeaving)
                    {
                        StartOfRound.Instance.overrideSpectateCamera = false;
                        SpectateCamera.transform.parent = PivotCamera.transform;
                        SpectateCamera.transform.position = PivotCamera.transform.position;
                    }
                    else
                    {
                        if (KeyDownPatch.UsePlayerCamera == true)
                        {
                            if (PlayerUnderwater)
                            {
                                HUDManager.Instance.setUnderwaterFilter = true;
                            }
                            HUDManager.Instance.spectatingPlayerText.text = "";
                            StartOfRound.Instance.overrideSpectateCamera = true;
                            AudioListener.gameObject.transform.parent = SpectateCamera.transform;
                            AudioListener.gameObject.transform.localPosition = new Vector3 (0, 0, 0);
                        }
                        else
                        {
                            //set text back to default, probably need to look at script?
                            HUDManager.Instance.setUnderwaterFilter = false;
                            StartOfRound.Instance.overrideSpectateCamera = false;
                            AudioListener.gameObject.transform.parent = PivotCamera.transform;
                            AudioListener.gameObject.transform.localPosition = new Vector3(0, 0, 0);
                        }

                        if (CameraHolder)
                        {
                            SpectateCamera.transform.position = CameraHolder.transform.position;
                            SpectateCamera.transform.parent = CameraHolder.transform;
                            SpectateCamera.transform.localPosition = new Vector3(0, 0, 0.2f);
                            SpectateCamera.transform.localRotation = Quaternion.Euler(0, 0, 0);
                        }
                        else
                        {
                            StartOfRound.Instance.overrideSpectateCamera = false;
                            PlayerBody = false;
                            return;
                        }
                    }
                }
            }
        }
    }
}
