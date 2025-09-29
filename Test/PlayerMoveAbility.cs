using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveAbility : MonoBehaviour
{
    public GridManager grid;
    public Vector2Int startGridPos = new Vector2Int(0, 0);
    public float moveDuration = 0.08f;

    private Vector2Int _gridPos;
    private bool _isMoving = false;

    private void Start()
    {
        if (grid == null) grid = FindObjectOfType<GridManager>();

        _gridPos = startGridPos;
        transform.position = grid.GridToWorld(_gridPos);
    }

    private void Update()
    {
        if (_isMoving) return;

        Vector2Int dir = Vector2Int.zero;
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) dir = Vector2Int.up;
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) dir = Vector2Int.down;
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) dir = Vector2Int.left;
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) dir = Vector2Int.right;

        if (dir != Vector2Int.zero) TryMove(dir);
    }

    private void TryMove(Vector2Int dir)
    {
        var target = _gridPos + dir;
        if (!grid.InBounds(target)) return;

        if (moveDuration <= 0f)
        {
            _gridPos = target;
            transform.position = grid.GridToWorld(_gridPos);
        }
        else
        {
            _isMoving = true;
            var start = transform.position;
            var end = grid.GridToWorld(target);
            StartCoroutine(MoveLerp(start, end, () =>
            {
                _gridPos = target;
                _isMoving = false;
            }));
        }
    }

    private System.Collections.IEnumerator MoveLerp(Vector3 from, Vector3 to, System.Action onComplete)
    {
        float t = 0f;
        while (t < moveDuration)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / moveDuration);
            transform.position = Vector3.Lerp(from, to, u);
            yield return null;
        }
        transform.position = to;
        onComplete?.Invoke();
    }
}
