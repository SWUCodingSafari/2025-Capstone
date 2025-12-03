using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Unity.Mathematics;
using UnityEngine;
using static WeatherSystem;

public class TileMapManager : MonoBehaviour
{
    public static TileMapManager Instance
    {
        get
        {
            if(instance == null)
            {
                instance = FindAnyObjectByType<TileMapManager>();
                if(instance == null )
                {
                    return null;
                }
            }
            return instance;
        }
    }
    private static TileMapManager instance;

    public class TileInfo
    {
        public const float MAXScent = 10f;
        public Vector2Int Index { get; set; }
        public WeatherInfo WeatherSetting { get; set; }
        public float deerScent;

        public void AddScent(float amount)
        {
            deerScent += amount;

            if (deerScent < 0f)
                deerScent = 0f;

            if(deerScent > MAXScent)
                deerScent = MAXScent;
        }

        public void DecayScent(float deltaTime, float decayRate)
        {
            float rate = Mathf.Clamp01(decayRate);

            float factor = 1f - rate * deltaTime;
            factor = Mathf.Clamp01(factor);

            deerScent *= factor;

            if (deerScent < 0.01f)
                deerScent = 0f;
        }
    }

    [Header("Map")]
    [SerializeField] private Transform mapTop;
    [SerializeField] private Transform mapBottom;

    [SerializeField] private Vector2 tileSize = Vector2.one;

    private int widthCount = 0;
    private int heightCount = 0;

    private List<List<TileInfo>> tiles = new List<List<TileInfo>>();

    [Header("Deer")]
    [SerializeField] private float decayInterval = 0.2f;
    [SerializeField] private float decayPercentPerSecond = 0.2f;
    private float fixedTimer = 0f;

    private void Awake()
    {
        instance = this;

        float width = mapTop.position.x - mapBottom.position.x;
        float height = mapTop.position.z - mapBottom.position.z;

        widthCount = Mathf.CeilToInt(width / tileSize.x);
        heightCount = Mathf.CeilToInt(height / tileSize.y);

        for (int i = 0; i < widthCount; i++)
        {
            tiles.Add(new List<TileInfo>());
            for (int j = 0; j < heightCount; j++)
            {
                TileInfo tile = new TileInfo
                {
                    Index = new Vector2Int(i, j),
                    deerScent = 0f
                };

                tiles[i].Add(tile);
                tiles[i][j].WeatherSetting = new WeatherInfo();
                tiles[i][j].WeatherSetting.State = WeatherState.None;
                tiles[i][j].WeatherSetting.Degree = Degree.Warm;
                tiles[i][j].WeatherSetting.windDirection = WindDirection.None;
                tiles[i][j].WeatherSetting.windSpeed = 0f;
            }
        }
    }

    private void FixedUpdate()
    {
        fixedTimer += Time.fixedDeltaTime;
        if (fixedTimer < decayInterval)
            return;

        DecayAllTiles(fixedTimer);
        fixedTimer = 0f;
    }

    private void DecayAllTiles(float deltaTime)
    {
        if(tiles == null) return;

        for (int x = 0; x < widthCount; ++x)
        {
            for (int y = 0; y < heightCount; ++y)
            {
                tiles[x][y].DecayScent(deltaTime, decayPercentPerSecond);
            }
        }
    }

    public List<TileInfo> GetAroundTiles(Vector3 pos, int range)
    {
        List<TileInfo> result = new List<TileInfo>();

        Vector2Int tileIndex = GetTilePos(pos);

        for (int i = -range; i <= range; ++i)
        {
            for (int j = -range; j <= range; ++j)
            {
                int x = tileIndex.x + i;
                int y = tileIndex.y + j;

                if (IsInBounds(x, y) == false)
                    continue;

                if (i * i + j * j > range * range)
                    continue;

                result.Add(tiles[x][y]);
            }
        }

        return result;
    }

    private bool IsInBounds(int x, int y)
        => x>= 0 && x < widthCount && y >= 0 && y < heightCount;

    public TileInfo GetTile(Vector3 pos)
    {
        return GetTile(GetTilePos(pos));
    }

    private Vector2Int GetTilePos(Vector3 pos)
    {
        Vector3 offset = pos - mapBottom.position;
        return new Vector2Int(Mathf.FloorToInt(offset.x / tileSize.x),
            Mathf.FloorToInt(offset.z / tileSize.y));
    }

    private TileInfo GetTile(Vector2Int tilePos)
    {
        if (IsInBounds(tilePos.x, tilePos.y) == false)
            return null;

        return tiles[tilePos.x][tilePos.y];
    }

    public void LeaveDeerScent(Vector3 pos, int radius, float maxStrength)
    {
        Vector2Int index = GetTilePos(pos);
        TileInfo center = GetTile(index);
        if (center == null)
            return;

        List<TileInfo> circleTiles = GetAroundTiles(pos, radius);

        foreach (TileInfo t in circleTiles)
        {
            float dist = Vector2Int.Distance(index, t.Index);
            float normalized = Mathf.Clamp01(dist / radius);
            float strength = Mathf.Lerp(10f, 0f, normalized);

            t.AddScent(strength);
        }

        if (center.WeatherSetting != null &&
            center.WeatherSetting.windDirection != WindDirection.None)
        {
            Vector2Int dirOffset = WeatherSystem.DirectionToOffset(center.WeatherSetting.windDirection);
            int maxDepth = 5;

            for (int i = 1; i <= maxDepth; ++i)
            {
                int x = index.x + dirOffset.x * i;
                int y = index.y + dirOffset.y * i;

                if (IsInBounds(x, y) == false)
                    break;

                TileInfo t = tiles[x][y];

                // 바람 꼬리는 기본 냄새보다 조금 약하게 예시
                float tailStrength = maxStrength * 0.5f * (1f - (i - 1) / (float)maxDepth);
                t.AddScent(tailStrength);
            }
        }
    }
}
