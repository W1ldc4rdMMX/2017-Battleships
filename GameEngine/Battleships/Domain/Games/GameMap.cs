﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Domain.Maps;
using Domain.Players;
using Domain.Ships;
using Domain.Weapons;
using Newtonsoft.Json;

namespace Domain.Games
{
    public class GameMap
    {
        public const int GameLevel = 1;

        public const string GameVersion = "1.0.0";

        [JsonIgnore] private readonly Dictionary<PlayerType, BattleshipPlayer> _players;

        [JsonIgnore] private readonly Dictionary<PlayerType, PlayerMap> _opponentMaps;

        [JsonIgnore] private readonly Dictionary<PlayerType, PlayerMap> _playersMaps;

        [JsonProperty]
        public int Phase { get; set; }

        [JsonProperty]
        public IList<BattleshipPlayer> RegisteredPlayers { get; }

        [JsonProperty]
        public int CurrentRound { get; set; }

        [JsonProperty]
        public int MapSize { get; set; }

        [JsonProperty]
        public bool SuccessfulFirstRound { get; set; }

        [JsonProperty]
        public string ReasonForFirstRoundFailure { get; set; }

        private GameMap()
        {
            this._players = new Dictionary<PlayerType, BattleshipPlayer>();
            this._opponentMaps = new Dictionary<PlayerType, PlayerMap>();
            this._playersMaps = new Dictionary<PlayerType, PlayerMap>();
        }

        public GameMap(string playerOneName, string playerTwoName, int mapWidth, int mapHeight)
            : this()
        {
            this.MapSize = mapHeight;
            this.RegisteredPlayers = new List<BattleshipPlayer>();

            var playerOne = new BattleshipPlayer(playerOneName, 'A', PlayerType.One);
            var playerTwo = new BattleshipPlayer(playerTwoName, 'B', PlayerType.Two);

            this._players[PlayerType.One] = playerOne;
            this._players[PlayerType.Two] = playerTwo;

            var playerOneMap = new PlayerMap(mapWidth, mapHeight, playerOne);
            var playerTwoMap = new PlayerMap(mapWidth, mapHeight, playerTwo);

            this._opponentMaps[PlayerType.One] = playerTwoMap;
            this._opponentMaps[PlayerType.Two] = playerOneMap;

            this._playersMaps[PlayerType.One] = playerOneMap;
            this._playersMaps[PlayerType.Two] = playerTwoMap;

            this.Phase = 1;
        }

        public bool Shoot(PlayerType player, Point target, WeaponType weaponType)
        {
            var actor = this._players[player];
            var targetMap = this._opponentMaps[player];
            var playerMap = this._playersMaps[player];

            if (!playerMap.IsReady())
            {
                throw new InvalidOperationException("All your ships must be placed before you are allowed to shoot");
            }
            return targetMap.Shoot(target, actor.GetWeapon(weaponType));
        }


        public bool WasShipDestroyed(PlayerType player, Point point)
        {
            var targetMap = this._opponentMaps[player];
            var ship = targetMap.GetShipAtPoint(point);
            return ship != null && ship.Destroyed;
        }

        public void Place(PlayerType playerType, ShipType shipType, Point coordinate, Direction direction)
        {
            var player = this._players[playerType];

            var shipToPlace = player.Ships.Single(x => x.GetType() == shipType.EnumToType());

            if (shipToPlace.Placed)
            {
                throw new InvalidOperationException($"{shipType} has already been placed");
            }

            _playersMaps[playerType].Place(shipToPlace, coordinate, direction);
        }

        public bool CanPlace(PlayerType playerType, ShipType shipType, Point coordinate, Direction direction)
        {
            var player = this._players[playerType];

            var shipToPlace = player.Ships.Single(x => x.GetType() == shipType.EnumToType());

            return !shipToPlace.Placed && _playersMaps[playerType].CanPlace(shipToPlace, coordinate, direction);
        }

        public void CleanMapBeforePlace(PlayerType playerType)
        {
            var playerMap = _playersMaps[playerType];
            var player = _players[playerType];
            player.ResetShips();
            playerMap.ClearMap();
        }

        public BattleshipPlayer GetBattleshipPlayer(PlayerType playerType)
        {
            return this._players[playerType];
        }

        public BattleshipPlayer GetOppoentBattleshipPlayer(PlayerType playerType)
        {
            switch (playerType)
            {
                case PlayerType.One:
                    return this._players[PlayerType.Two];
                case PlayerType.Two:
                    return this._players[PlayerType.One];
                default:
                    return null;
            }
        }

        public PlayerMap GetOpponetMap(PlayerType playerType)
        {
            switch (playerType)
            {
                case PlayerType.One:
                    return this._playersMaps[PlayerType.Two];
                case PlayerType.Two:
                    return this._playersMaps[PlayerType.One];
                default:
                    return null;
            }
        }

        public PlayerMap GetPlayerMap(PlayerType playerType)
        {
            return _playersMaps[playerType];
        }

        public void RegisterPlayer(BattleshipPlayer player)
        {
            RegisteredPlayers.Add(player);
        }
    }
}