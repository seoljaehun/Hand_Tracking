# MediaPipe를 이용한 Hand Tracking

import cv2
import mediapipe as mp
import socket

# MediaPipe 솔루션 초기화: 손 추적 기능과 유틸리티 기능 불러오기
mp_hands = mp.solutions.hands
mp_drawing = mp.solutions.drawing_utils

# Hand 객체 초기화: 손 인식 모델 로드(딥러닝 추론 모델)
hands = mp_hands.Hands(
    static_image_mode=False,
    max_num_hands=1,
    min_detection_confidence=0.5,
    min_tracking_confidence=0.5)
# static_image_mode: 비디오 스트림용
# max_num_hands = 1 : 최대 1개의 손 감지
# min_detection_confidence = 0.5 : 탐지 신뢰도
# min_tracking_confidence = 0.5 : 추적 신뢰도

# 웹캠 열기
width, height = 720, 720
cap = cv2.VideoCapture(0)
cap.set(3, width)    # 웹캠 너비 (Width)
cap.set(4, height)     # 웹캠 높이 (Height)

# UDP 통신 설정
UDP_IP = "127.0.0.1"        # unity가 실행되는 PC의 IP 주소
UDP_PORT = 6006             # 데이터를 주고받을 포트 번호
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)     # UDP 소켓 생성

# 웹캠이 성공적으로 열렸는지 확인
while cap.isOpened():
    
    # 웹캠에서 프레임, 이미지 읽기
    # Frame : 성공 여부 (True/False) -> 필요 없음
    # Image : 실제 이미지 데이터
    Frame, image = cap.read() 
    
    # 이미지 복사본 만들지 않고, 데이터를 직접 처리
    image.flags.writeable = False
    
    # BGR -> RGB
    image_rgb = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
    
    # 모델을 실행하여 손 위치 추론
    results = hands.process(image_rgb)
    
    # 추론 후, OpenCV에게 쓰기 권한 부여
    image.flags.writeable = True
    
    # 하나 이상의 손이 탐지됬는지 확인
    if results.multi_hand_landmarks:
        # 탐지된 손에 대해 반복 (현재 1회 반복 -> 손 1개 탐지)
        for hand_landmarks in results.multi_hand_landmarks:
            
            print("--- 새 손 감지 ---")
            
            # 21개의 점 데이터를 저장할 리스트 초기화
            data_to_send = []
            
            # 감지된 손의 21개 점을 하나씩 반복
            for idx, landmark in enumerate(hand_landmarks.landmark):
                
                # 감지된 점 마다의 3D 좌표를 출력
                print(f'랜드마크 {idx}: x={landmark.x:.4f}, y={1 - landmark.y:.4f}, z={landmark.z:.4f}')
                
                # x, y, z 좌표를 리스트에 추가 (x,y,z 순서대로 총 63개) -> unity에 전달할 값
                data_to_send.append(f"{landmark.x - 0.5:.4f}")
                data_to_send.append(f"{(1 - landmark.y) - 0.5:.4f}")
                data_to_send.append(f"{landmark.z:.4f}")
            
            # 쉼표(,)로 연결하여 하나의 문자열 생성    
            message = ",".join(data_to_send)
            # UDP로 데이터 전송
            sock.sendto(message.encode('utf-8'), (UDP_IP, UDP_PORT))
            
            # 원본 image 위에 점과 점을 이은 연결선을 그림  
            mp_drawing.draw_landmarks(
                image,
                hand_landmarks,
                mp_hands.HAND_CONNECTIONS)
        
    # 이미지 좌우반전
    flipped_image = cv2.flip(image, 1)
    
    # 처리된 최종 이미지를 화면에 출력
    cv2.imshow('MediaPipe Hands', flipped_image)
    
    # 'q' 키가 눌리면 루프 종료
    if cv2.waitKey(5) & 0xFF == ord('q'):
        break

# 웹캠 닫고, Hand 객체 해제
hands.close()
cap.release()
cv2.destroyAllWindows()
