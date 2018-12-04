using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LevelGenerator : MonoBehaviour
{
    public int levelIndex = 1;
    public GameObject bg_prefab;
    public GameObject[] tile_prefabs;

    protected static int xLength = 15;
    protected static int yLength = 8;
    
    private Vector3[,] space = new Vector3[xLength, yLength];

    private Tile target_first;
    private Tile target_second;
    public Tile[] tiles;

    private Vector2 startMousePos;

    private void Start()
    {
        tiles = new Tile[xLength * space.GetLength(1)];
        float xOffset = xLength / 2f - 0.5f;
        float yOffset = space.GetLength(1) / 2f - 0.5f;

        levelIndex = Random.Range(0, Levels.templates.Length);

        for (int y = 0; y < space.GetLength(1); y++)
        {
            for (int x = 0; x < xLength; x++)
            {
                var index = x + (y * xLength);
                if (Levels.templates[levelIndex][index] == 1)
                {
                    var pos = new Vector3(x - xOffset, y - yOffset, 0);
                    var temp = Instantiate(bg_prefab, pos, Quaternion.identity);
                    temp.transform.parent = transform;
                    var typeIndex = Random.Range(0, tile_prefabs.Length);
                    var tile = Instantiate(tile_prefabs[typeIndex], pos, Quaternion.identity);
                    tile.transform.parent = transform;
                    var tileComponent = tile.AddComponent<Tile>();
                    tileComponent.indexX = x;
                    tileComponent.indexY = y;
                    tileComponent.typeIndex = typeIndex;
                    tiles[index] = tileComponent;
                }
            }
        }
    }

    private void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
        if (hit.collider.tag != "Player") return;
        Debug.Log(1);
        if (Input.GetMouseButtonDown(0))
        {
            startMousePos = hit.point;
            target_first = hit.collider.gameObject.GetComponent<Tile>();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            Direction direction;
            float x = startMousePos.x - hit.point.x;
            float y = startMousePos.y - hit.point.y;
            if (Mathf.Abs(x) > Mathf.Abs(y)) //Горизонтально
            {
                if (startMousePos.x - hit.point.x < 0)
                {
                    direction = Direction.Right;

                }
                else
                {
                    direction = Direction.Left;
                }
            }
            else
            {
                if (startMousePos.y - hit.point.y < 0)
                {
                    direction = Direction.Up;
                }
                else
                {
                    direction = Direction.Down;
                }
            }
            Debug.Log(direction.ToString());

            Tile target_second = SearchTileToDiretion(target_first, direction);

            if (target_first != null && target_second != null)
            {
                StartCoroutine(NewPos(target_first, target_second));
                target_first = null;
                target_second = null;
            }
            else
            {
                target_first = null;
                target_second = null;
            }
        }

        //tiles[0].transform.position = hit.point;
    }

    private Tile SearchTileToDiretion(Tile target_first, Direction direction)
    {
        int indexX = target_first.indexX;
        int indexY = target_first.indexY;
        int searchIndexX = target_first.indexX;
        int searchIndexY = target_first.indexY;
        switch (direction)
        {
            case Direction.Up:
                searchIndexY += 1;
                if (searchIndexY < yLength -2)
                {
                    return tiles[searchIndexX + (searchIndexY * xLength)];
                }
                return null;
            case Direction.Down:
                searchIndexY -= 1;
                if (searchIndexY >= 0)
                {
                    return tiles[searchIndexX + (searchIndexY * xLength)];
                }
                return null;
            case Direction.Right:
                searchIndexX += 1;
                if (searchIndexX < xLength)
                {
                    return tiles[searchIndexX + (searchIndexY * xLength)];
                }
                return null;
            case Direction.Left:
                searchIndexX -= 1;
                if (searchIndexX >= 0)
                {
                    return tiles[searchIndexX + (searchIndexY * xLength)];
                }
                return null;
        }
        return null;
    }

    private IEnumerator NewPos(Tile target_first, Tile target_second)
    {
        float speed = 3f;
        float timer = 0f;

        Tile tempTile = target_first;
        tiles[target_first.indexX + (target_first.indexY * xLength)] = target_second;
        tiles[target_second.indexX + (target_second.indexY * xLength)] = tempTile;

        int x = target_first.indexX;
        int y = target_first.indexY;
        target_first.indexX = target_second.indexX;
        target_first.indexY = target_second.indexY;
        target_second.indexX = x;
        target_second.indexY = y;

        Vector2 start_first = target_first.transform.position;
        Vector2 start_second = target_second.transform.position;

        while (timer < 1)
        {
            timer += Time.deltaTime * speed;
            target_first.transform.position = Vector3.Lerp(start_first, start_second, timer);
            target_second.transform.position = Vector3.Lerp(start_second, start_first, timer);            
            yield return null;
        }

        target_second.transform.position = start_first;
        target_first.transform.position = start_second;        
    }

    private enum Direction
    {
        Up,
        Down,
        Right,
        Left
    }
}
