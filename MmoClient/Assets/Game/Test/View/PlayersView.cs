using System;
using System.Collections.Generic;
using Shared;
using UnityEngine;

namespace Game
{
    public class PlayersView : MonoBehaviour
    {
        [SerializeField] private PlayerPrototype[] _prototypes;
        private readonly Dictionary<byte, PlayerView> _prototypesByViewID = new();
        private readonly Dictionary<ushort, PlayerView> _viewsByPlayerId = new();
        [SerializeField] private PlayerCamera _playerCamera;
        private ushort _playerId;

        public void SetPlayerId(ushort playerId)
        {
            _playerId = playerId;
            TrySetPlayerCamera();
        }

        private void TrySetPlayerCamera()
        {
            if (!_playerCamera.HasTarget && _viewsByPlayerId.TryGetValue(_playerId, out PlayerView playerView))
            {
                _playerCamera.SetTarget(playerView);
            }
        }

        private void Awake()
        {
            foreach (var playerPrototype in _prototypes)
            {
                _prototypesByViewID.Add(playerPrototype.ViewID, playerPrototype.PlayerView);
            }
        }

        public void Synchronize(PlayerSnapshot[] players)
        {
            foreach (PlayerSnapshot snapshot in players)
            {
                if (!_viewsByPlayerId.TryGetValue(snapshot.PlayerId, out PlayerView playerView))
                {
                    if (!_prototypesByViewID.TryGetValue(snapshot.ViewId, out PlayerView prototype))
                    {
                        Debug.LogError($"not binded prototype for view id {snapshot.ViewId} ");
                        continue;
                    }

                    var position = snapshot.Position.ToVector3();
                    playerView = Instantiate(prototype, position, Quaternion.identity);
                    _viewsByPlayerId.Add(snapshot.PlayerId, playerView);
                    playerView.Initialize(position);
                }
                else
                {
                    playerView.SetPosition(snapshot.Position.ToVector3());
                }
            }
            TrySetPlayerCamera();
        }
    }

    [Serializable]
    public struct PlayerPrototype
    {
        public PlayerView PlayerView;
        public byte ViewID;
    }
}