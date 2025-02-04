﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace Map
{
    public class MapGeneration : MonoBehaviour
    {
        private Tilemap _map;
        private readonly Vector3Int _mapSize = new Vector3Int(12,8, 0);
        private readonly Vector3Int _mapOffset = new Vector3Int(-6,-4,0);

        private TileTypesManager _tileTypes;

        private void Awake()
        {
            _map = transform.GetChild(0).GetComponent<Tilemap>();

            _tileTypes = FindObjectOfType<TileTypesManager>();

            GenerateMap();
        }

        private void ClearMap()
        {
            _map.ClearAllTiles();
        }

        private void GenerateMap()
        {
            ClearMap();
            PlaceRandomTileOnEveryColumn();
            FillEmptyWithTile(_tileTypes.grass);

            var bounds = _map.localBounds;
            var max = new Vector3Int(Convert.ToInt32(bounds.max.x), Convert.ToInt32(bounds.max.y), 0);
            var min = new Vector3Int(Convert.ToInt32(bounds.min.x), Convert.ToInt32(bounds.min.y), 0);

            var oneWithoutZ = new Vector3Int(1,1,0);
            _map.CompressBounds();
            var contour = GetContour(min - oneWithoutZ, max + oneWithoutZ);

            foreach (var tile in contour)
            {
                _map.SetTile(tile + _mapOffset - oneWithoutZ, _tileTypes.brick);
            }
        }

        private void PlaceRandomTileOnEveryColumn()
        {
            var placedTiles = new List<Vector3Int>();

            var startingX = 0-(_mapSize.x / 2);

            var minY = -(_mapSize.y / 2);
            var maxY = (_mapSize.y / 2);

            var minimumDifference = 3;

            var lastPosition = 0;

            bool skipCurrent = true;

            for (var currentTile = 0; currentTile < _mapSize.x; currentTile++)
            {
                skipCurrent = !skipCurrent;
                if (skipCurrent) continue;

                if (currentTile != 0 && currentTile != _mapSize.x-1)
                {
                    var skip = Random.Range(0, 9);
                    if (skip == 0) continue;
                }

                var posY = Random.Range(minY, maxY);

                while (Mathf.Abs(posY - lastPosition) < minimumDifference)
                {
                    posY = Random.Range(minY, maxY+1);
                }

                lastPosition = posY;

                var newPos = new Vector3Int(startingX + currentTile, posY, 0);

                _map.SetTile(newPos, _tileTypes.basicRoad);

                placedTiles.Add(newPos);
            }

            ConnectTiles(placedTiles);

            var lastTilePos = placedTiles[placedTiles.Count - 1];
            _map.SetTile(lastTilePos, _tileTypes.endStone);
        }

        private List<Vector3Int> GetContour(Vector3Int topLeft, Vector3Int bottomRight)
        {
            int height = Mathf.Abs(topLeft.y - bottomRight.y);
            int width = Mathf.Abs(topLeft.x - bottomRight.x);
            int max, min;

            max = Mathf.Max(height, width);
            min = Mathf.Min(height, width);

            var vectorList = new List<Vector3Int>();

            for (int i = 0; i < max; i++)
            {
                for (int j = 0; j < min; j++)
                {

                    if (i == 0 || i == max - 1)
                    {
                        var heightV = new Vector3Int(i, j,0);
                        vectorList.Add(heightV);
                    }
                    else if (j == 0 || j == min - 1)
                    {
                        var heightV = new Vector3Int(i, j,0);
                        vectorList.Add(heightV);
                    }
                }
            }

            return vectorList;
        }

        private void ConnectTiles(List<Vector3Int> placedTiles)
        {
            PlaceTilesUntilBottomIsReached(placedTiles[0], _tileTypes.basicRoad);

            for (int i = 0; i < placedTiles.Count; i++)
            {
                if (i == placedTiles.Count-1) break;
                
                var placedTile = placedTiles[i];
                var targetTile = placedTiles[i+1];

                var currentPos = new Vector3Int(placedTile.x, placedTile.y, placedTile.z);
                var targetPos = new Vector3Int(targetTile.x, targetTile.y, targetTile.z);

                currentPos = PlaceTilesToTheRight(currentPos, targetPos, _tileTypes.basicRoad);
                PlaceTilesUpDown(currentPos, targetPos, _tileTypes.basicRoad);
            }
        }

        private void PlaceTilesUntilBottomIsReached(Vector3Int startingTile, TileBase tileToPlace)
        {
            var howManyDown = Mathf.Abs(_mapOffset.y - startingTile.y);

            var currentPos = new Vector3Int(startingTile.x, startingTile.y, startingTile.z);
            for (int i = 0; i < howManyDown; i++)
            {
                currentPos += Vector3Int.down;
                _map.SetTile(currentPos, tileToPlace);
            }
        }

        private Vector3Int PlaceTilesToTheRight(Vector3Int currentPos, Vector3Int targetPos, TileBase tileToPlace)
        {
            var howManyToTheRight = targetPos.x - currentPos.x;

            for (int i = 0; i < howManyToTheRight; i++)
            {
                currentPos += Vector3Int.right;
                _map.SetTile(currentPos, tileToPlace);
            }

            var lastPosition = currentPos;
            return lastPosition;
        }

        private void PlaceTilesUpDown(Vector3Int currentPos, Vector3Int targetPos, TileBase tileToPlace)
        {
            var howManyTillTarget = Mathf.Abs(targetPos.y - currentPos.y);
            var direction = DirectionUpOrDown(currentPos, targetPos);

            for (int i = 0; i < howManyTillTarget; i++)
            {
                currentPos += direction;
                _map.SetTile(currentPos, tileToPlace);
            }
        }

        private Vector3Int DirectionUpOrDown(Vector3Int currentPos, Vector3Int targetPos)
        {
            var direction = (targetPos.y - currentPos.y) > 0;

            return direction ? Vector3Int.up : Vector3Int.down;
        }

        private void FillEmptyWithTile(TileBase tileToFill)
        {
            _map.CompressBounds();
            var size = _map.size;
            for (int i = 0; i < size.x; i++)
            {
                for (int j = 0; j < size.y; j++)
                {
                    var tilePos = new Vector3Int(i, j, 0) + _mapOffset;
                    if (_map.GetTile(tilePos) == null)
                    {
                        _map.SetTile(tilePos, tileToFill);
                    }
                }
            }
        }
    }
}
