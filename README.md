# [Find Party Protocol](https://github.com/Socket-Protocol)
## 기능
- 채팅방 생성
- 채팅방 참여
- 채팅방 인원 확인
- 채팅방 삭제
- 채팅방 탈퇴
- 쿠폰 발급 및 모두에게 알리기
---
## 메시지 포맷
- ID : id 등록
- FP : 사람을 구하는 파티 등록
- IP : 파티에 참여
- CP : 파티 참여자 명단 확인
- DP : 파티 삭제하기
- QP : 파티에서 탈퇴하기
- Coupon : 쿠폰 전송 및 모두에게 알리기
- BR : 전체 사용자에게 메시지 전송
- TO : 한 사용자에게 메시지 전송
---
## 소켓통신 네트워크 연결 절차
![네트워크 연결 절차](https://github.com/max990624/C-_LoginApplication/assets/39523433/efab5f28-bb9b-460b-aa92-10da331bad67)

---
ID : id 등록

- Client : “ID: 사용자 id”

- Server : 성공 시“ID_REG_SUCCESS”를 Client에 전송, 다른 사용자들에게 새로운 사용자 알림

FP : 사람을 구하는 파티 등록

- Client : “FP: 파티 이름:최대인원”

- Server : 성공 시 “FP_SUCCESS”를 Client에 전송, 해당 파티 배열 생성, 전체 사용자들에게 “[User:등록한 사용자][Make Party:파티이름][max member:최대인원]” 새로운 파티 등록 알림

IP : 파티에 참여

- Client : “IP: 등록할 파티 이름”

- Server : 배열 탐색 전 [{파티이름}파티 참가 시도 중...]을 Client에 전송. 배열 내 빈칸 발견 실패 시 “IP_Fail”을 Client에 전송, 성공 시 “IP_SUCCESS”를 Client에 전송 “IP_Fail”과 “IP_SUCCESS”는 배열을 모두 탐색하거나 성공 할 때까지 진행, 해당 파티 배열에 사용자 추가, 전체 사용자들에게 파티에 “IP:사용자:파티이름:” 새로운 인원 참가 알림

CP : 파티 참여자 명단 확인

- Client : “CP: 파티 이름”

- Server : 성공 시“[{파티이름}파티 참가 인원]:”과 파티 참여 인원 목록, “[총 @명]”을  Client에 전송, 실패 시(파티이름이 일치 하지 않는 경우) “{파티이름}은 없는 파티입니다.”를 Client에 전송

DP : 파티 삭제

- Client : “DP: 파티 이름”

- Server : 시도 시 “[{파티이름} 파티 삭제 시도 중...]”을 Client에 전송, 성공 시“DP_Success”를 Client에 전송, 해당 파티 배열 초기화, 다른 사용자들에게 파티 삭제 알림

QP : 파티에서 탈퇴하기

- Client : “QP: 파티 이름”

- Server : 성공 시“QP_SUCCESS”를 Client에 전송, 해당 파티 배열에서 사용자 삭제, 다른 사용자들에게 파티 탈퇴 알림

Coupon : 서버에 쿠폰 이미지 송신 및 전체 사용자에게 메시지 전송

- Client : “Coupon : 파일명.확장자”

- Server : 성공 시“Coupon _Success”를 보낸 사용자에게 전송, 전체 사용자들에게 "쿠폰이 발급되었습니다." 메시지 전송 (보낸 사용자 제외)

BR : 전체 사용자에게 메시지 전송

- Client : “BR: 메시지”

- Server : 성공 시“BR_SUCCESS”를 Client에 전송, 전체 사용자들에게 사용자가 보낸 메시지 전송 (보낸 사용자 제외)

TO : 한 사용자에게 메시지 전송

- Client : “TO: Receiver id:메시지”

- Server : 성공 시“TO_SUCCESS”를 Client에 전송, Receiver 사용자에게 Sender 사용자가 보낸 메시지 전송 
