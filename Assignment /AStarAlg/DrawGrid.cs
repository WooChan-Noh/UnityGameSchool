using UnityEngine;

public class DrawGrid : MonoBehaviour
{
    private float sideLength = 10f;
    public float cellSize = 1f;
    [HideInInspector]
    public float planeSize = 0f;
    public Color lineColor = Color.black;

    void Update()
    {
        CheckCellSize();
    }

    private void LateUpdate()
    {
        DrawLine();
    }

    //플레이모드에서만 보인다 -> 격자가 안보이면 게임 뷰의 기즈모 체크
    private void DrawLine()
    {
        planeSize = sideLength / cellSize;

        for (int next = 0; next <= planeSize; next++)
        {
            float position = cellSize * next;
            //세로선
            Vector3 startHeightVerxtex = new Vector3(position, 0, 0);
            Vector3 endHeightVertex = new Vector3(position, sideLength, 0);
            //가로선
            Vector3 startWidthVertex = new Vector3(0, position, 0);
            Vector3 endWidthVertex = new Vector3(sideLength, position, 0);

            Debug.DrawLine(startWidthVertex, endWidthVertex, lineColor);
            Debug.DrawLine(startHeightVerxtex, endHeightVertex, lineColor);
        }
    }

    private void CheckCellSize()
    {
        if (cellSize > 10f)
        {
            Debug.Log("셀 크기는 10보다 작아야 합니다. 현재 크기는 5입니다.");
            cellSize = 5f;
        }

        if (cellSize <= 0f)
        {
            Debug.Log("셀 크기는 0보다 커야 합니다. 현재 크기는 0.2입니다.");
            cellSize = 0.2f;
        }
    }
}