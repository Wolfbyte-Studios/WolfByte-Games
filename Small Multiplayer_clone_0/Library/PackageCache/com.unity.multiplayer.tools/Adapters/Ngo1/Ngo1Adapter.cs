using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Unity.Multiplayer.Tools.Common;
using Unity.Multiplayer.Tools.MetricEvents;
using Unity.Multiplayer.Tools.NetStats;
using Unity.Netcode;
using UnityEngine;

namespace Unity.Multiplayer.Tools.Adapters.Ngo1
{
    class Ngo1Adapter

        : INetworkAdapter

        // Events
        // --------------------------------------------------------------------
        , IGetConnectedClients
        , IMetricCollectionEvent

        // Queries
        // --------------------------------------------------------------------
        , IGetBandwidth
        , IGetClientId
        , IGetGameObject
        , IGetObjectIds
        , IGetOwnership
        , IGetRpcCount
    {
        [NotNull]
        readonly NetworkManager m_NetworkManager;

        [MaybeNull]
        NetworkSpawnManager SpawnManager => m_NetworkManager.SpawnManager;

        [MaybeNull]
        Dictionary<ulong, NetworkObject> SpawnedObjects => SpawnManager?.SpawnedObjects;

        public Ngo1Adapter([NotNull] NetworkManager networkManager)
        {
            DebugUtil.TraceMethodName();

            Debug.Assert(networkManager != null, $"The parameter {nameof(networkManager)} can't be null.");

            m_NetworkManager = networkManager;
            m_NetworkManager.OnClientConnectedCallback += OnClientConnected;
            m_NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
            m_NetworkManager.NetworkTickSystem.Tick += OnTick;

            MetricEventPublisher.OnMetricsReceived += OnMetricsReceived;
        }

        readonly List<ClientId> m_ClientIds = new();
        readonly List<ObjectId> m_ObjectIds = new();

        void OnTick()
        {
            RefreshObjectIds();
            RefreshClientIds();
        }

        void RefreshObjectIds()
        {
            m_ObjectIds.Clear();
            var spawnedObjects = m_NetworkManager.SpawnManager?.SpawnedObjectsList;
            if (spawnedObjects == null)
            {
                return;
            }
            foreach (var spawnedObject in spawnedObjects)
            {
                m_ObjectIds.Add((ObjectId)spawnedObject.NetworkObjectId);
            }
        }

        void RefreshClientIds()
        {
            if (m_NetworkManager.IsServer)
            {
                m_ClientIds.Clear();
                foreach (var clientId in m_NetworkManager.ConnectedClientsIds)
                {
                    m_ClientIds.Add((ClientId)clientId);
                }
            }
            else if (m_NetworkManager.SpawnManager != null)
            {
                // NetworkManager.ConnectedClientsIds is only available on the server
                foreach (var clientId in m_NetworkManager.SpawnManager.OwnershipToObjectsTable.Keys)
                {
                    if (!m_ClientIds.Contains((ClientId)clientId))
                    {
                        m_ClientIds.Add((ClientId)clientId);
                        OnClientConnected(clientId);
                    }
                }
            }
        }

        public AdapterMetadata Metadata { get; } = new AdapterMetadata
        {
            PackageInfo = new PackageInfo
            {
                PackageName = "com.unity.netcode.gameobjects",
                Version = new PackageVersion
                {
                    Major = 1,
                    Minor = 0,
                    Patch = 0,
                    PreRelease = ""
                }
            }
        };

        public T GetComponent<T>() where T : class, IAdapterComponent
        {
            return this as T;
        }

        // Events
        // --------------------------------------------------------------------
        public IReadOnlyList<ClientId> ConnectedClients => m_ClientIds;
        public event Action<ClientId> ClientConnectionEvent;
        void OnClientConnected(ulong clientId)
        {
            var typedClientId = (ClientId)clientId;
            if (!m_ClientIds.Contains(typedClientId))
            {
                m_ClientIds.Add(typedClientId);
            }
            ClientConnectionEvent?.Invoke(typedClientId);
        }

        public event Action<ClientId> ClientDisconnectionEvent;
        void OnClientDisconnected(ulong clientId)
        {
            var typedClientId = (ClientId)clientId;
            m_ClientIds.RemoveAll(id => id == typedClientId);
            ClientDisconnectionEvent?.Invoke(typedClientId);
        }

        public event Action<MetricCollection> MetricCollectionEvent;
        void OnMetricsReceived(MetricCollection metricCollection)
        {
            UpdateNetworkTrafficCaches(metricCollection);
            MetricCollectionEvent?.Invoke(metricCollection);
        }

        // Simple Queries
        // --------------------------------------------------------------------
        public ClientId LocalClientId => (ClientId)m_NetworkManager.LocalClientId;
        public ClientId ServerClientId => (ClientId)NetworkManager.ServerClientId;

        public IReadOnlyList<ObjectId> ObjectIds => m_ObjectIds;

        public GameObject GetGameObject(ObjectId objectId)
        {
            var spawnedObjects = SpawnedObjects;
            if (spawnedObjects.TryGetValue((ulong)objectId, out var networkObject))
            {
                return networkObject.gameObject;
            }
            return null;
        }

        public ClientId GetOwner(ObjectId objectId)
        {
            var spawnedObjects = SpawnedObjects;
            if (spawnedObjects.TryGetValue((ulong)objectId, out var networkObject))
            {
                return (ClientId)networkObject.OwnerClientId;
            }
            return 0;
        }

        // Cached Queries
        // --------------------------------------------------------------------
        ObjectBandwidthCache m_BandwidthCache;
        ObjectRpcCountCache m_RpcCountCache;

        void UpdateNetworkTrafficCaches(MetricCollection metricCollection)
        {
            if (m_OnBandwidthUpdated != null)
            {
                m_BandwidthCache.Update(metricCollection);
                m_OnBandwidthUpdated.Invoke();
            }

            if (m_OnRpcCountUpdated != null)
            {
                m_RpcCountCache.Update(metricCollection);
                m_OnRpcCountUpdated.Invoke();
            }
        }

        // IGetBandwidth
        // --------------------------------------------------------------------
        event Action m_OnBandwidthUpdated;

        public bool IsCacheEmpty => m_BandwidthCache == null || m_BandwidthCache.IsCold;
        
        public BandwidthTypes SupportedBandwidthTypes =>
            BandwidthTypes.Other | BandwidthTypes.Rpc | BandwidthTypes.NetVar;

        public event Action OnBandwidthUpdated
        {
            add
            {
                m_BandwidthCache ??= new();
                m_OnBandwidthUpdated += value;
            }
            remove
            {
                m_OnBandwidthUpdated -= value;
                if (m_OnBandwidthUpdated == null)
                {
                    m_BandwidthCache = null;
                }
            }
        }

        public float GetBandwidthBytes(
            ObjectId objectId,
            BandwidthTypes bandwidthTypes = BandwidthTypes.All,
            NetworkDirection networkDirection = NetworkDirection.SentAndReceived)
            => m_BandwidthCache?.GetBandwidth(objectId, bandwidthTypes, networkDirection)
                ?? throw new NoSubscribersException(nameof(IGetBandwidth), nameof(OnBandwidthUpdated));

        // IGetRpcCount
        // --------------------------------------------------------------------
        event Action m_OnRpcCountUpdated;
        public event Action OnRpcCountUpdated
        {
            add
            {
                m_RpcCountCache ??= new();
                m_OnRpcCountUpdated += value;
            }
            remove
            {
                m_OnRpcCountUpdated -= value;
                if (m_OnRpcCountUpdated == null)
                {
                    m_RpcCountCache = null;
                }
            }
        }

        public int GetRpcCount(ObjectId objectId)
            => m_RpcCountCache?.GetRpcCount(objectId)
               ?? throw new NoSubscribersException(nameof(IGetRpcCount), nameof(OnRpcCountUpdated));
    }
}
