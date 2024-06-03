using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AStar : MonoBehaviour
{
    private DrawGrid myGrid;//필요한 변수를 가져오기 위함
    private Vector3 mousePosition;
    private bool[,] grid = new bool[50, 50]; //열린 목록, 닫힌 목록 탐색을 관리하는 배열
    private int[] inputCell = new int[4]; //탐색에 사용할 인풋 데이터를 관리하는 배열
    private int noData = -1;
    private int drawingTime = 5; //셀 색칠에 사용함
    private float coloringFrequency = 0.1f; 
 


    void Start()
    {
        myGrid = GetComponent<DrawGrid>();
        InitData();
    }

    void Update()
    {
        SetCellType();
    }

    void InitData()
    {
        for (int i = 0; i < inputCell.Length; i++)
        {
            inputCell[i] = -1;
        }

        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                grid[x, y] = true;
            }
        }
    }

    void SetInputCell(int x, int y)
    {
        //인풋 데이터를 넣는다.
        //0,1번에는 시작 셀(x,y)이, 2,3번에는 마지막 셀(x,y)데이터가 차례대로 들어간다.
        
        if (inputCell[0] == noData)
        {
            inputCell[0] = x;
            inputCell[1] = y;
        }
        else if (inputCell[0] != noData || inputCell[2] == noData)
        {
            inputCell[2] = x;
            inputCell[3] = y;
        }

        //시작 셀(x,y) 마지막 셀(x2,y2)데이터가 전부 있으면 탐색 시작
        if (inputCell[3] != noData)
            FindPath(inputCell[0], inputCell[1], inputCell[2], inputCell[3], grid);
    }

    void ColoringCell(int x, int y, float cellSize, Color color)
    {
        //빈칸을 채우는 함수
        
        float startX = x * cellSize;
        float startY = y * cellSize;

        //세로선 그리기 (y고정)
        for (float next = startX; next <= startX + cellSize; next += coloringFrequency)
        {
            Vector3 startHeightVertex = new Vector3(next, startY, 0);
            Vector3 endHeightVertex = new Vector3(next, startY + cellSize, 0);

            Debug.DrawLine(startHeightVertex, endHeightVertex, color, drawingTime);
        }

        //가로선 그리기 (x고정)
        for (float next = startY; next <= startY + cellSize; next += coloringFrequency)
        {
            Vector3 startWidthVertex = new Vector3(startX, next, 0);
            Vector3 endWidthVertex = new Vector3(startX + cellSize, next, 0);

            Debug.DrawLine(startWidthVertex, endWidthVertex, color, drawingTime);
        }
    }

    //마우스 왼쪽 클릭으로 셀 선택
    //마우스 오른쪽 클릭으로 장애물 셀 설정
    void SetCellType()
    {
        mousePosition = Input.mousePosition;
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);

        int xCoordinate = (int)(mousePosition.x / myGrid.cellSize);
        int yCoordinate = (int)(mousePosition.y / myGrid.cellSize);

        //시작 셀과 마지막 셀 선택
        if (Input.GetMouseButtonDown(0))
        {
            //Debug.Log("실제 마우스 클릭 위치: " + mousePosition);
            //Debug.Log($"선택한 셀 좌표 : ({xCoordinate},{yCoordinate})");

            //마우스 클릭 범위 체크
            if (xCoordinate < 0 || yCoordinate < 0 || grid[xCoordinate, yCoordinate] == false ||
                yCoordinate > myGrid.planeSize - 1 || xCoordinate > myGrid.planeSize - 1)
            {
                Debug.Log("그리드를 벗어나거나 장애물 블럭을 클릭했습니다.");
                return;
            }

            ColoringCell(xCoordinate, yCoordinate, myGrid.cellSize, Color.blue);
            SetInputCell(xCoordinate, yCoordinate);
        }
        //장애물 셀 선택
        else if (Input.GetMouseButtonDown(1))
        {
            //Debug.Log("실제 마우스 클릭 위치: " + mousePosition);
            //Debug.Log($"금지한 셀 좌표 : ({xCoordinate},{yCoordinate})");

            grid[xCoordinate, yCoordinate] = false;//탐색을 막기 위한 값 설정
            ColoringCell(xCoordinate, yCoordinate, myGrid.cellSize, Color.red);
        }
    }


    public List<Node> FindPath(int startX, int startY, int endX, int endY, bool[,] grid)
    {
        // 시작 노드와 목표 노드 생성
        Node startNode = new Node(startX, startY);
        Node endNode = new Node(endX, endY);

        // 열린 목록과 닫힌 목록 초기화
        List<Node> openList = new List<Node> { startNode };
        HashSet<Node> closedList = new HashSet<Node>();

        while (openList.Count > 0) // 열린 목록이 비어있지 않은 동안 반복
        {
            // openList에서 fCost가 가장 작은 노드를 currentNode로 선택
            Node currentNode = openList[0];
            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].fCost < currentNode.fCost ||
                    (openList[i].fCost == currentNode.fCost && openList[i].hCost < currentNode.hCost))
                {
                    currentNode = openList[i];
                }
            }

            openList.Remove(currentNode); // 선택된 노드를 열린 목록에서 제거
            closedList.Add(currentNode); // 선택된 노드를 닫힌 목록에 추가
            this.grid[currentNode.x, currentNode.y] = false; //다시 탐색할 필요 없게 설정
            //Debug.Log($"Node added to closed list: ({currentNode.x}, {currentNode.y})");

            // 목표 노드에 도달한 경우 경로 출력
            if (currentNode.x == endNode.x && currentNode.y == endNode.y)
            {
                endNode.parent = closedList.Last();
                List<Node> path = AStarPath(endNode);
                path.RemoveAt(path.Count - 1); //중복 제거
                if (path != null)
                {
                    foreach (Node node in path)
                    {
                        //Debug.Log($"({node.x}, {node.y})");
                        ColoringCell(node.x, node.y, myGrid.cellSize, Color.green);
                    }

                    InitData();
                    return path;
                }
            }

            // 현재 노드의 이웃 노드들을 처리
            List<Node> neighbors = GetNeighbors(currentNode, grid);
            foreach (Node neighbor in neighbors)
            {
                // 이웃 노드가 이미 닫힌 목록에 있으면 건너뛰기
                if (closedList.Contains(neighbor))
                    continue;
                // 시작 노드에서 이웃 노드까지의 새로운 비용 계산
                int newGCost = currentNode.gCost + GetDistance(currentNode, neighbor);
                // 열린 목록에 이웃 노드가 없거나, 새로운 비용이 더 작은 경우
                if (!openList.Contains(neighbor) || newGCost < neighbor.gCost)
                {
                    neighbor.gCost = newGCost; // 새로운 gCost 할당
                    neighbor.hCost = GetDistance(neighbor, endNode); // 목표 노드까지의 예상 비용 계산
                    neighbor.parent = currentNode; // 경로 재구성을 위해 부모 노드 할당
                    // 열린 목록에 이웃 노드가 없으면 추가 
                    if (!openList.Contains(neighbor))
                    {
                        openList.Add(neighbor);
                        //Debug.Log($"Neighbor added to open list: ({neighbor.x}, {neighbor.y}) with gCost: {neighbor.gCost}," +
                        //          $" hCost: {neighbor.hCost} my parents x :{neighbor.parent.x}, {neighbor.parent.y}");
                    }
                }
            }
        }
        // 열린 목록이 비어있으면 경로가 없다는 뜻
        InitData();
        Debug.Log("경로가 없음!");
        return null;
    }

    // 주변 이웃 노드들을 찾는 함수
    private List<Node> GetNeighbors(Node node, bool[,] grid)
    {
        List<Node> neighbors = new List<Node>();
        //Debug.Log($"{node.x},{node.y}");

        // 상하좌우 노드들을 확인하여 이동 가능한 노드를 neighbors에 추가
        if (node.x - 1 >= 0 && grid[node.x - 1, node.y]) // 왼쪽
            neighbors.Add(new Node(node.x - 1, node.y, node));

        if (node.x + 1 < myGrid.planeSize && grid[node.x + 1, node.y]) // 오른쪽
            neighbors.Add(new Node(node.x + 1, node.y, node));

        if (node.y - 1 >= 0 && grid[node.x, node.y - 1]) // 아래쪽
            neighbors.Add(new Node(node.x, node.y - 1, node));

        if (node.y + 1 < myGrid.planeSize && grid[node.x, node.y + 1]) // 위쪽
            neighbors.Add(new Node(node.x, node.y + 1, node));

        return neighbors;
    }

    private int GetDistance(Node a, Node b)
    {
        int dx = Math.Abs(a.x - b.x);
        int dy = Math.Abs(a.y - b.y);

        return dx + dy;
    }

// 목표 노드에서 시작 노드까지의 경로를 구성하는 함수
    private List<Node> AStarPath(Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        // 목표 노드에서 시작하여 부모 노드를 따라가며 경로를 재구성
        while (currentNode != null)
        {
            path.Insert(0, currentNode); // 경로에 현재 노드 추가
            currentNode = currentNode.parent; // 다음 노드는 부모 노드
        }

        return path;
    }
}

public class Node
{
    public int x, y;
    public int gCost, hCost;
    public Node parent;

    public Node(int x, int y, Node parent = null, int gCost = 0, int hCost = 0)
    {
        this.x = x;
        this.y = y;
        this.parent = parent;
        this.gCost = gCost;
        this.hCost = hCost;
    }

    public int fCost
    {
        get { return gCost + hCost; }
    }
}