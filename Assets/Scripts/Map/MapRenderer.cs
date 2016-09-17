using System.Collections.Generic;
using UnityEngine;

public class MapRenderer : MonoBehaviour
{
    public const string RENDER_URL_SCHEME = "http://mt1.googleapis.com/vt?lyrs=m@248009395&src=apiv3&hl=de&x={0}&y={1}&z={2}&apistyle=s.e%3Al%7Cp.v%3Aoff%2Cs.t%3A2&style=47,37%7Csmartmaps";
    public const int TILE_SIZE = 256;
    public const int PIXELS_PER_UNIT = 100;
    public static readonly Vector2 PIVOT = new Vector2(0, 1);
    public static readonly float UNITS_PER_TILE = (float)TILE_SIZE / PIXELS_PER_UNIT;
    public int MapRadius = 3;
    public int ZoomLevel = 17;
    public Transform Root;

    private List<RenderCell> _cells;
    private RenderGrid _renderGrid;
    private LRUSpriteDictionary _spriteDictionary;
    private ProjectedPosition _loadedPosition;

    private Vector3 _position;

    public ProjectedPosition CurrentPosition { get; set; }
    public float Width { get; private set; }
    public float Height { get; private set; }


    void Start()
    {
        SpriteLoaderService.Initialize(PIXELS_PER_UNIT, PIVOT);
        Initialize();
    }

    private void Initialize()
    {
        var dim = 2 * MapRadius + 1;
        InitCells(dim, dim);
        Width = dim * UNITS_PER_TILE;
        Height = dim * UNITS_PER_TILE;
        _spriteDictionary = new LRUSpriteDictionary((dim + 1) * (dim + 1));
        _renderGrid = new RenderGrid(_cells.ToArray(), dim, dim);
        _loadedPosition = new ProjectedPosition(MapRadius, MapRadius, ZoomLevel, 0, 0);
        _position = new Vector3();
    }

    private void InitCells(int x, int y)
    {
        _cells = new List<RenderCell>();
        for (int i = 0; i < y; i++)
        {
            for (int j = 0; j < x; j++)
            {
                var renderCell = CreateRenderCell(j,i);
                _cells.Add(renderCell);
            }
        }
    }

    void Update()
    {
        if (_loadedPosition != CurrentPosition)
        {
            LoadNewTiles();
        }
        MoveToCenter();
    }

    RenderCell CreateRenderCell(int x, int y)
    {
        GameObject go = new GameObject("tile_" + x + "_" + y);
        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.material = (Material)Resources.Load("Materials/MapTile", typeof(Material));
        renderer.sortingOrder = -1;

        Vector3 position = new Vector3();
        position.x = x * UNITS_PER_TILE;
        position.y = -y * UNITS_PER_TILE;
        go.transform.parent = Root;
        go.transform.localPosition = position;
        go.transform.localRotation = Quaternion.identity;
        var renderCell = go.AddComponent<RenderCell>();
        return renderCell;
    }

    private void LoadNewTiles()
    {
        if (Equals(CurrentPosition, default(ProjectedPosition)))
            return;
        _renderGrid.Shift(_loadedPosition.X - CurrentPosition.X, _loadedPosition.Y - CurrentPosition.Y);
        _loadedPosition = CurrentPosition;
        for (int i = _loadedPosition.Y - MapRadius; i <= _loadedPosition.Y + MapRadius; i++)
        {
            for (int j = _loadedPosition.X - MapRadius; j <= _loadedPosition.X + MapRadius; j++)
            {
                if (_renderGrid[j, i] == null)
                {
                    _renderGrid[j, i] = SpawnTile(j, i);
                }
            }
        }
    }

    private void MoveToCenter()
    {
        _position.x = -((MapRadius + (float)CurrentPosition.LocalX) * UNITS_PER_TILE);
        _position.y = ((MapRadius + (float)CurrentPosition.LocalY) * UNITS_PER_TILE);
        Root.localPosition = _position;
    }

    public RefCountedSprite SpawnTile(int x, int y)
    {
        RefCountedSprite sprite;
        if (_spriteDictionary.TryGetValue(new LRUSpriteDictionary.SpriteID(x, y), out sprite))
        {
            return sprite;
        }
        sprite = new RefCountedSprite();
        _spriteDictionary.Add(new LRUSpriteDictionary.SpriteID(x, y), sprite);

        SpriteLoaderService.LoadSpriteAsync(string.Format(RENDER_URL_SCHEME, x, y, ZoomLevel), sprite.SetSprite);

        return sprite;
    }

    public Vector2 GetPosition(ProjectedPosition pos)
    {
        pos.X -= _renderGrid.OffsetX;
        pos.Y -= _renderGrid.OffsetY;
        float x = (float)(pos.X + pos.LocalX) * UNITS_PER_TILE + _position.x;
        float y = -((float)(pos.Y + pos.LocalY) * UNITS_PER_TILE) + _position.y;
        return new Vector2(x, y);
    }
}
