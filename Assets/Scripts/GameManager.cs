using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public AudioClip cellClick;
    public AudioClip areaClick;
    public AudioClip mineClick;
    public AudioClip setFlagClick;
    public AudioClip removeFlagClick;
    public Tilemap boardMap;
    public Tilemap minesMap;
    public Tilemap cellsMap;
    public Tilemap flagMap;
    public Tile boardTile;
    public Tile mineTile;
    public Tile cellTile;
    public Tile flagTile;
    public Tile[] numbersTiles;
    private List<Vector3Int> mines = new List<Vector3Int>();
    private Vector3Int? firstClick;
    private Vector2Int boardSize = new Vector2Int(20, 20);
    private int minesAmount = 30;
    private int flagsAmount = 0;
    private Vector3 cameraPosition;
    private bool isGameOver = false;
    public GameObject gameOverScreen;
    public GameObject winScreen;
    
    public RectInt Bounds {
        get
        {
            Vector2Int position = new Vector2Int(-BoardSize.x / 2, -BoardSize.y / 2);
            return new RectInt(position, BoardSize);
        }
    }

    public int MinesAmount { get => minesAmount; set => minesAmount = value; }
    public int FlagsAmount { get => flagsAmount; set => flagsAmount = value; }
    public Vector2Int BoardSize { get => boardSize; set => boardSize = value; }
    public bool IsGameOver { get => isGameOver; set => isGameOver = value; }

    private void Awake() 
    {
        minesMap.size = new Vector3Int(Bounds.xMax, Bounds.yMax, 0);
        cellsMap.size = new Vector3Int(Bounds.xMax, Bounds.yMax, 0);
        boardMap.size = new Vector3Int(Bounds.xMax, Bounds.yMax, 0);
        
        cameraPosition = GameObject.Find("Main Camera").transform.position;
    }

    void Start()
    {
        CreateBoard();
        CoverCells();
    }

    void Update()
    {
        if(!IsGameOver)
        {
            if (Input.GetMouseButtonDown(0))
            {
                ClickedOnCell();
            }

            if (Input.GetMouseButtonDown(1))
            {
                SetFlag();
            }
        }
    }

    private void ClickedOnCell()
    {
        Vector3Int mousePosition = cellsMap.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));

        if (flagMap.HasTile(mousePosition)) return;

        if (cellsMap.HasTile(mousePosition))
        {
            if (firstClick == null)
            {
                firstClick = mousePosition;
                SpawnMines();
                SetNumbers();
            }

            AudioSource.PlayClipAtPoint(cellClick, cameraPosition);
            cellsMap.SetTile(mousePosition, null);

            if (!minesMap.HasTile(mousePosition))
            {
                AudioSource.PlayClipAtPoint(areaClick, cameraPosition);
                UncoverCells(GetPositionsOnAxis(mousePosition).ToList());
            }

            if (minesMap.HasTile(mousePosition) && minesMap.GetTile(mousePosition).name == mineTile.name)
            {
                AudioSource.PlayClipAtPoint(mineClick, cameraPosition);
                UncoverMines();
                GameOver();
            }
        }
    }

    private void SetFlag()
    {
        Vector3Int mousePosition = cellsMap.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));

        if (flagMap.HasTile(mousePosition)) 
        {
            AudioSource.PlayClipAtPoint(removeFlagClick, cameraPosition);
            flagMap.SetTile(mousePosition, null);
            FlagsAmount--;
        }
        else
        {
            if (cellsMap.HasTile(mousePosition)) 
            {
                AudioSource.PlayClipAtPoint(setFlagClick, cameraPosition);
                flagMap.SetTile(mousePosition, flagTile);
                FlagsAmount++;

                if (CheckWin()) Win();
            }
        }
    }

    private bool CheckWin()
    {
        if (minesAmount != flagsAmount) return false;
        
        foreach (var mine in mines)
        {
            if (!flagMap.HasTile(mine)) return false;
        }

        return true;
    }

    private void UncoverCells(List<Vector3Int> positions)
    {
        if (positions.Count() == 0) return;

        Vector3Int pos = positions.First();

        if (cellsMap.HasTile(pos) && !minesMap.HasTile(pos) && isOnBoard(pos)) 
        {
            cellsMap.SetTile(pos, null);
            positions = positions.Concat(GetPositionsOnAxis(pos).ToList()).Distinct().ToList();
        }

        positions.Remove(pos);
        UncoverCells(positions);
    }

    private void UncoverMines()
    {
        foreach (var mine in mines)
        {
            if (cellsMap.HasTile(mine))
            {
                if (flagMap.HasTile(mine)) flagMap.SetTile(mine, null);                
                cellsMap.SetTile(mine, null);
            }
        }
    }

    private void GameOver()
    {
        IsGameOver = true;
        gameOverScreen.SetActive(true);
    }

    private void Win()
    {
        IsGameOver = true;
        winScreen.SetActive(true);
    }

    private void SpawnMines()
    {
        for (int i = 0; i < MinesAmount; i++)
        {
            Vector3Int minePosition = RandomizePosition();

            while (minesMap.HasTile(minePosition) || GetPositionsAround((Vector3Int) firstClick).Where(x => (Vector3Int) x == minePosition).Count() != 0 || firstClick == minePosition)
            {
                minePosition = RandomizePosition();
            }

            mines.Add(minePosition);
            minesMap.SetTile(minePosition, mineTile);
        }
    }

    private Vector3Int RandomizePosition()
    {
        int xNew = Random.Range(Bounds.xMin, Bounds.xMax);
        int yNew = Random.Range(Bounds.yMin, Bounds.yMax);
        return new Vector3Int(xNew, yNew, 0);
    }

    private void CoverCells()
    {
        cellsMap.BoxFill(new Vector3Int(Bounds.xMin, Bounds.yMin, 0), cellTile, Bounds.xMin, Bounds.yMin, Bounds.xMax, Bounds.yMax);
    }

    private void CreateBoard()
    {
        boardMap.BoxFill(new Vector3Int(Bounds.xMin, Bounds.yMin, 0), boardTile, Bounds.xMin, Bounds.yMin, Bounds.xMax, Bounds.yMax);
    }

    private void SetNumbers()
    {
        foreach (var mine in mines)
        {
            Vector3Int[] coordsAround = GetPositionsAround(mine);

            foreach (var coord in coordsAround)
            {
                if (minesMap.HasTile((Vector3Int) coord)) continue;
                
                Vector3Int[] temp = GetPositionsAround(coord);
                int minesAround = 0;

                foreach (var item in temp)
                {
                    if (minesMap.HasTile((Vector3Int)item) && minesMap.GetTile((Vector3Int)item).name == mineTile.name) minesAround++;
                }

                if (minesAround > 0) minesMap.SetTile((Vector3Int)coord, numbersTiles[minesAround]);
            }
        }
    }

    private bool isOnBoard(Vector3Int position)
    {
        if (Bounds.Contains((Vector2Int) position)) return true;
        return false;
    }

    private Vector3Int[] GetPositionsAround(Vector3Int position)
    {
        Vector3Int[] minusCoords = {new Vector3Int(-1,0),
                                    new Vector3Int(-1,1),
                                    new Vector3Int(0,1),
                                    new Vector3Int(1,1),
                                    new Vector3Int(1,0),
                                    new Vector3Int(1,-1),
                                    new Vector3Int(0,-1),
                                    new Vector3Int(-1,-1)};
        List<Vector3Int> result = new List<Vector3Int>();

        for (int i = 0; i < minusCoords.Length; i++)
        {
            if (isOnBoard(minusCoords[i] + position)) result.Add(minusCoords[i] + position);
        }

        return result.ToArray();
    }

    private Vector3Int[] GetPositionsOnAxis(Vector3Int position)
    {
        Vector3Int[] minusCoords = {new Vector3Int(-1,0),
                                    new Vector3Int(0,1),
                                    new Vector3Int(1,0),
                                    new Vector3Int(0,-1)};
        List<Vector3Int> result = new List<Vector3Int>();

        for (int i = 0; i < minusCoords.Length; i++)
        {
            if (isOnBoard(minusCoords[i] + position)) result.Add(minusCoords[i] + position);
        }

        return result.ToArray();
    }
}