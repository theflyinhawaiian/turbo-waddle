using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    GameObject tilePrefab;
    [SerializeField]
    int numRows = 20;
    [SerializeField]
    int numCols = 40;
    [SerializeField]
    float tileSize = 2f;

    Vector2 boardOffset;

    Tile[,] grid;

    bool bombsInitialized = false;

    ClickResult mostRecentClickResult;
    bool recentLeftClick;
    bool recentRightClick;
    bool doubleClick;
    float mostRecentEventTime;
    
    // Start is called before the first frame update
    void Start()
    {
        InitGrid();
    }

    private void InitGrid()
    {
        grid = new Tile[numCols, numRows];

        for(var i = 0; i < numCols; i++)
        {
            for(var j = 0; j < numRows; j++)
            {
                var tile = new Tile(Instantiate(tilePrefab, transform));
                var xPos = (i * tileSize) + tileSize/2;
                var yPos = (j * tileSize) + tileSize/2;

                tile.SetPosition(new Vector2(xPos, yPos));

                tile.AdjacentBombs = 0;// Random.Range(0, 8);
                grid[i, j] = tile;
            }
        }

        boardOffset = new Vector2((-numCols * tileSize) / 2 - tileSize / 2, (-numRows * tileSize) / 2 - tileSize/2);
        transform.position = boardOffset;
    }

    private Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, y) * tileSize;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameFlags.GameOver)
            return;

        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var gridpos = GridPositionFromMousePosition(mousePos);

        if (Input.GetMouseButtonUp(0))
        {
            if (!bombsInitialized)
                InitializeBombs(gridpos.x, gridpos.y);

            mostRecentClickResult = grid[gridpos.x, gridpos.y].OnClick();

            if(mostRecentClickResult == ClickResult.Mine)
            {
                StartCoroutine(AnimateGameOver());
                return;
            }

            var isEmpty = mostRecentClickResult == ClickResult.Clear;
            if (isEmpty)
                RecursivelyClickAdjacentSquares(gridpos.x, gridpos.y);

            if (mostRecentClickResult == ClickResult.AlreadyClicked)
            {
                if (recentLeftClick == true)
                {
                    doubleClick = true;
                }

                recentLeftClick = true;
                mostRecentEventTime = Time.time;
            }

        }else if (Input.GetMouseButtonUp(1))
        {
            mostRecentClickResult = grid[gridpos.x, gridpos.y].OnFlag();

            if (mostRecentClickResult == ClickResult.AlreadyClicked)
            {
                recentRightClick = true;
                mostRecentEventTime = Time.time;
            }
        }

        if(mostRecentEventTime + 100 < Time.time)
        {
            recentLeftClick = false;
            recentRightClick = false;
            doubleClick = false;
        }

        var TwoButtonRecursiveClick = recentLeftClick && recentRightClick && mostRecentClickResult == ClickResult.AlreadyClicked;
        var OneButtonRecursiveClick = doubleClick && mostRecentClickResult == ClickResult.AlreadyClicked;

        if(TwoButtonRecursiveClick || OneButtonRecursiveClick)
        {
            RecursivelyClickIfFlagged(gridpos.x, gridpos.y);
            recentLeftClick = false;
            recentRightClick = false;
            doubleClick = false;
        }

        if (PlayerHasWon())
        {
            GameFlags.PlayerWon = true;
            GameFlags.GameOver = true;
        }
    }

    void InitializeBombs(int startX, int startY)
    {
        if (bombsInitialized)
            return;

        var bombCoords = new List<(int x, int y)>
        {
            (startX, startY)
        };

        var bombCount = 0;
        while(bombCount < 99)
        {
            var x = Random.Range(0, numCols);
            var y = Random.Range(0, numRows);

            if (!bombCoords.Contains((x, y)))
            {
                bombCoords.Add((x, y));
                grid[x, y].HasBomb = true;
                bombCount++;
                UpdateAdjacentCounts(x, y);
            }
        }

        bombsInitialized = true;
    }

    void UpdateAdjacentCounts(int pivotX, int pivotY)
    {
        if (!grid[pivotX, pivotY].HasBomb)
            return;

        TraverseAdjacentSquares(pivotX, pivotY, tile => tile.AdjacentBombs++);
    }

    (int x, int y) GridPositionFromMousePosition(Vector3 mousePosition)
    {
        var position = new Vector2(mousePosition.x, mousePosition.y) - boardOffset;

        var x = Mathf.FloorToInt(position.x / tileSize);
        var y = Mathf.FloorToInt(position.y / tileSize);
        return (x, y);
    }

    void TraverseAdjacentSquares(int centerX, int centerY, Action<Tile> job)
    {
        var startX = Math.Max(0, centerX - 1);
        var endX = Math.Min(centerX + 1, numCols - 1);
        var startY = Math.Max(0, centerY - 1);
        var endY = Math.Min(centerY + 1, numRows - 1);

        for (var i = startX; i <= endX; i++)
        {
            for (var j = startY; j <= endY; j++)
            {
                if (i != centerX || j != centerY)
                {
                    job.Invoke(grid[i, j]);
                }
            }
        }
    }

    void RecursivelyClickIfFlagged(int centerX, int centerY)
    {
        var startX = Math.Max(0, centerX - 1);
        var endX = Math.Min(centerX + 1, numCols - 1);
        var startY = Math.Max(0, centerY - 1);
        var endY = Math.Min(centerY + 1, numRows - 1);

        var flaggedCount = 0;

        for (var i = startX; i <= endX; i++)
        {
            for (var j = startY; j <= endY; j++)
            {
                if (i != centerX || j != centerY)
                {
                    if (grid[i, j].IsFlagged)
                        flaggedCount++;
                }
            }
        }

        if (grid[centerX, centerY].AdjacentBombs == flaggedCount)
            RecursivelyClickAdjacentSquares(centerX, centerY);
    }

    void RecursivelyClickAdjacentSquares(int centerX, int centerY)
    {
        var startX = Math.Max(0, centerX - 1);
        var endX = Math.Min(centerX + 1, numCols - 1);
        var startY = Math.Max(0, centerY - 1);
        var endY = Math.Min(centerY + 1, numRows - 1);

        for (var i = startX; i <= endX; i++)
        {
            for (var j = startY; j <= endY; j++)
            {
                if (i != centerX || j != centerY && !grid[i,j].IsClicked)
                {
                    mostRecentClickResult = grid[i, j].OnClick();
                    if(mostRecentClickResult == ClickResult.Mine)
                    {
                        StartCoroutine(AnimateGameOver());
                        return;
                    }

                    var isEmpty = mostRecentClickResult == ClickResult.Clear;
                    if (isEmpty)
                        RecursivelyClickAdjacentSquares(i, j);
                }
            }
        }
    }

    IEnumerator AnimateGameOver()
    {
        for(var i = 0; i < numCols; i++)
        {
            for(var j = 0; j < numRows; j++)
            {
                if (grid[i, j].HasBomb)
                {
                    grid[i, j].OnClick();

                    yield return new WaitForSeconds(0.03f);
                }
            }
        }

        GameFlags.GameOver = true;
    }

    bool PlayerHasWon()
    {
        for(var i = 0; i < numCols; i++)
        {
            for(var j = 0; j < numRows; j++)
            {
                if(!grid[i, j].HasBomb && !grid[i, j].IsClicked)
                {
                    return false;
                }
            }
        }

        return true;
    }
}
