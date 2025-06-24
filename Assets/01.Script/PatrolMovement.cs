using UnityEngine;

public class PatrolMovement : MonoBehaviour
{
    public float leftX = -3f;
    public float rightX = 3f;
    public float speed = 2f;

    private Vector3 targetPosition;

    void Start()
    {
        targetPosition = new Vector3(rightX, transform.position.y, transform.position.z);
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        // ��ǥ ������ ���� �����ϸ� ���� ��ȯ
        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            if (Mathf.Approximately(targetPosition.x, rightX))
                targetPosition = new Vector3(leftX, transform.position.y, transform.position.z);
            else
                targetPosition = new Vector3(rightX, transform.position.y, transform.position.z);
        }
    }
}
