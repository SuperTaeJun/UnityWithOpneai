using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Size")]
    public int width = 5;
    public int height = 5;

    [Header("Prefabs")]
    public GameObject cellPrefab;

    [Header("Visual")]
    public Vector2 cellWorldSize = new Vector2(1f, 1f);

    private Dictionary<Vector2Int, Cell> _cells = new Dictionary<Vector2Int, Cell>();

    public bool InBounds(Vector2Int gp) => gp.x >= 0 && gp.x < width && gp.y >= 0 && gp.y < height;

    public Vector3 GridToWorld(Vector2Int gp)
    {
        // 그리드의 "시작점"(좌하 코너)을 중앙 기준으로 계산
        Vector2 half = new Vector2((width - 1) * 0.5f, (height - 1) * 0.5f);
        Vector3 origin = transform.position
                       - new Vector3(half.x * cellWorldSize.x, half.y * cellWorldSize.y, 0f);

        return origin + new Vector3(gp.x * cellWorldSize.x, gp.y * cellWorldSize.y, 0f);
    }
    [ContextMenu("Build Grid")]
    public void BuildGrid()
    {
        ClearAll();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var gp = new Vector2Int(x, y);
                var cellGo = Instantiate(cellPrefab, GridToWorld(gp), Quaternion.identity, transform);
                cellGo.name = $"Cell_{x}_{y}";

                var cell = cellGo.GetComponent<Cell>();
                if (cell == null) cell = cellGo.AddComponent<Cell>();
                cell.gridPos = gp;

                _cells.Add(gp, cell);
            }
        }
    }

    [ContextMenu("Clear All")]
    public void ClearAll()
    {
        var toDestroy = new List<GameObject>();
        foreach (Transform child in transform) toDestroy.Add(child.gameObject);
        foreach (var go in toDestroy) DestroyImmediate(go);

        _cells.Clear();
    }
}