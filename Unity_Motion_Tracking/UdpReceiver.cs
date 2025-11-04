using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Net;
using System.Linq;

public class UdpDebugReceiver : MonoBehaviour
{
    // 포트 번호 설정
    private const int port = 6006;

    private Thread receiveThread;   // UDP 데이터 수신을 저장할 변수
    private UdpClient client;       // UDP 통신 객체 변수
    private volatile string lastReceivedData = "";   // 가장 최신 받은 문자열을 저장하는 버퍼

    public Transform[] handLandmarks = new Transform[21];   // Unity 오브젝트 뼈대랑 연결할 배열
    public float scaleFactor = 1.0f;    // 좌표 스케일 값

    // start 함수
    void Start()    // 스크립트 활성화 후 1번 실행
    {
        // ReceiveData 함수를 실행할 새로운 작업공간 선언
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;  // 백그라운드에서 실행
        receiveThread.Start();  // Thread 실행

        Debug.Log("UDP Debug Receiver started on port " + port);
    }

    // 데이터를 수신하는 백그라운드 스레드 함수 (무한 반복)
    private void ReceiveData()
    {
        try
        {
            // 포트를 열어 통신을 받을 준비
            client = new UdpClient(port);

            while (true)
            {
                // 데이터를 보낸 상대방의 IP주소를 저장할 공간 준비
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);

                // 대기하다가 데이터가 도착하면 byte 배열로 저장
                byte[] data = client.Receive(ref anyIP);

                // 수신된 데이터를 문자열로 변환
                string receivedMessage = Encoding.UTF8.GetString(data);

                // 수신된 최신 좌표 저장
                lastReceivedData = receivedMessage;
            }
        }
        // 예외 처리
        catch (System.Threading.ThreadAbortException)
        {
            // 종료 시 발생하는 정상적인 예외는 무시
        }
        catch (System.Exception err)
        {
            Debug.LogError("UDP Debug Error: " + err.ToString());
        }
    }

    // 메인 스레드에서 데이터 처리 및 적용
    void Update()
    {
        float[] coords;
        try
        {
            // 문자열을 쉼표(,)로 분리하여 63개의 배열로 변환
            coords = lastReceivedData.Split(',')
                                    .Select(float.Parse)
                                    .ToArray();
        }
        catch (System.FormatException)
        {
            return;
        }
        catch (System.ArgumentException)
        {
            return;
        }

        // 예외 처리
        if (coords.Length != 63 || handLandmarks.Length != 21)
            return;

        // 21개의 관절을 순서대로 반복
        for (int i = 0; i < 21; i++)
        {
            if (handLandmarks[i] == null)
                continue;

            // i번째 관절의 x, y, z 좌표 추출
            float x = coords[i * 3];
            float y = coords[i * 3 + 1];
            float z = coords[i * 3 + 2];

            // 추출된 x, y, z 좌표에 스케일 값을 곱하여 vector3 객체 생성
            Vector3 Position = new Vector3(
                x * scaleFactor,
                y * scaleFactor,
                z * scaleFactor
            );

            // Unity 오브젝트 관절에 Vector 값을 넣어줌
            handLandmarks[i].localPosition = Position;
        }
    }

    // 자원 정리
    void OnApplicationQuit()    // unity play 모드를 종료할 때 실행
    {
        if (receiveThread != null && receiveThread.IsAlive)
        {
            // 백그라운드 스레드에 강제 종료 명령
            receiveThread.Abort();
        }
        if (client != null)
        {
            // 포트 닫기
            client.Close();
        }
    }
}
