using UnityEngine;

public class Line_Tracking : MonoBehaviour
{
    // UI 설정
    [Header("연결할 랜드마크 포인트")]
    public Transform startPoint;    // Line 시작점
    public Transform endPoint;      // Line 끝점

    [Header("컴포넌트")]
    // 선을 그릴 LineRenderer 컴포넌트 변수
    public LineRenderer lineRenderer;

    [Header("Line Width")]
    public float lineWidth = 0.01f; // 기본 두께를 0.01 유닛으로 설정

    void Awake()
    {
        lineRenderer.positionCount = 2;     // 시작점, 끝점 2개만 필요

        // Line 두께 조절 설정 초기화
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
    }

    void Update()   // 실시간 위치 업데이트
    {
        if (startPoint != null && endPoint != null)
        {
            // 매 프레임, 시작점과 끝점의 월드 위치를 읽어와 LineRenderer에 적용
            lineRenderer.SetPosition(0, startPoint.position); // 시작점 (0번 인덱스)
            lineRenderer.SetPosition(1, endPoint.position);   // 끝점 (1번 인덱스)
        }
    }
}