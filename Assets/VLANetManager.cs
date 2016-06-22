﻿// --------------------------------------------------------------
// VLENetManager.cs is part of the VLAB project.
// Copyright (c) 2016 All Rights Reserved
// Li Alex Zhang fff008@gmail.com
// 5-16-2016
// --------------------------------------------------------------

using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using VLab;

namespace VLabAnalysis
{
    public class VLANetManager : NetworkManager
    {
        public VLAUIController uicontroller;
        GameObject vlabanalysismanagerprefab;

        void RegisterSpawnHandler()
        {
            vlabanalysismanagerprefab = Resources.Load<GameObject>("VLAnalysisManager");
            var assetid = vlabanalysismanagerprefab.GetComponent<NetworkIdentity>().assetId;
            ClientScene.RegisterSpawnHandler(assetid, new SpawnDelegate(AnalysisManagerSpawnHandler),
                new UnSpawnDelegate(AnalysisManagerUnSpawnHandler));
        }

        /// <summary>
        /// Prepare network so that when connected to server, will react properly to 
        /// server commands
        /// </summary>
        /// <param name="client"></param>
        public override void OnStartClient(NetworkClient client)
        {
            // override default handler with our own to deal with server's ChangeScene message.
            client.RegisterHandler(MsgType.Scene, new NetworkMessageDelegate(OnClientScene));
            RegisterSpawnHandler();
        }

        /// <summary>
        /// our custom handler for server's ChangeScene message, since VLabAnalysis doesn't deal
        /// with any scene, we just ingore the message, pretending that scene has already been loaded
        /// and immediatly tell server we have changed the scene and ready to proceed.
        /// </summary>
        /// <param name="netMsg"></param>
        void OnClientScene(NetworkMessage netMsg)
        {
            if (IsClientConnected() && !NetworkServer.active)
            {
                OnClientSceneChanged(client.connection);
            }
        }

        GameObject AnalysisManagerSpawnHandler(Vector3 position, NetworkHash128 assetId)
        {
            GameObject go;
            if(uicontroller.alsmanager==null)
            {
                go = Instantiate(vlabanalysismanagerprefab);
                var als = go.GetComponent<VLAnalysisManager>();
                als.uicontroller = uicontroller;
                uicontroller.alsmanager = als;
                go.name = "VLAnalysisManager";
                go.transform.SetParent(transform);
            }
            else
            {
                go = uicontroller.alsmanager.gameObject;
            }
            return go;
        }

        void AnalysisManagerUnSpawnHandler(GameObject spawned)
        {
        }

        /// <summary>
        /// because the fundamental difference of VLabAnalysis and VLabEnvironment, VLab should treat them
        /// differently, so whenever a client connected to server, it seeds information about the client, so
        /// that server could treat them accordingly.
        /// </summary>
        /// <param name="conn"></param>
        public override void OnClientConnect(NetworkConnection conn)
        {
            if (LogFilter.logDebug)
            {
                UnityEngine.Debug.Log("Send PeerType Message.");
            }
            client.Send(VLMsgType.PeerType, new IntegerMessage((int)VLPeerType.VLabAnalysis));
        }

        public override void OnClientDisconnect(NetworkConnection conn)
        {
            base.OnClientDisconnect(conn);
            uicontroller.OnClientDisconnect();
        }

    }
}