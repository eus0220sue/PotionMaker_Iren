using UnityEngine;

public class CameraBase : MonoBehaviour
{
    [Header("Ä«¸Þ¶ó ÀÌµ¿ Á¦ÇÑ ¹üÀ§")]
    public Vector2 minPos;
    public Vector2 maxPos;

    private Transform target;
    private Camera cam;

    private void Start()
    {
        target = GManager.Instance.IsUserTrans;
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (!GManager.Instance.IsSettingFlag) return;
        if (target == null || cam == null) return;

        Vector3 targetPos = target.position;

        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        // ¸Ê »çÀÌÁî
        float mapWidth = maxPos.x - minPos.x;
        float mapHeight = maxPos.y - minPos.y;

        float clampedX, clampedY;

        // ³ÐÀº ¸ÊÀÏ °æ¿ì (¸ÊÀÌ Ä«¸Þ¶óº¸´Ù Å­)
        if (mapWidth > camWidth * 2)
        {
            clampedX = Mathf.Clamp(targetPos.x, minPos.x + camWidth, maxPos.x - camWidth);
        }
        else
        {
            // ¸ÊÀÌ Á¼À¸¸é Áß¾Ó °íÁ¤
            clampedX = (minPos.x + maxPos.x) / 2f;
        }

        if (mapHeight > camHeight * 2)
        {
            clampedY = Mathf.Clamp(targetPos.y, minPos.y + camHeight, maxPos.y - camHeight);
        }
        else
        {
            clampedY = (minPos.y + maxPos.y) / 2f;
        }

        transform.position = new Vector3(clampedX, clampedY, transform.position.z);
    }

    public void SetCameraBounds(Vector2 min, Vector2 max)
    {
        minPos = min;
        maxPos = max;
    }
}
