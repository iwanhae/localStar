# LocalStar

## InterNode Communication Protocal
1. HandShake
    우선순위 높은놈 : A
    우선순위 낮은놈 : B
    1. ID 교환, 중복여부 판단.
        1바이트 길이, 이후 UTF-8로 전송.
    2. 암호화등 통신환경설정 공유
    3. A -> B TimeStamp Echo   (속도확인)
    4. B -> A TimeStamp Echo   (속도확인)
    5. A -> B 자기 노드정보 공유
    6. B -> A 자기 노드정보 공유
    7. A -> B 주변 노드정보 공유
    8. B -> A 정보 보충해서 다시 공유
2. Comm
    10바이트 헤더 + Body 무한반복.

    바이트
    0
    1   ConnectionId 2바이트 (새로운 커넥션을 만들때는 00으로 호출)
    2
    3   PayloadLength   (최대 약 64KB)
    4   Reserved
    5   Reserved
    6   Reserved
    7   Reserved
    8   Settings
    9   Settings
3. End Transmission

### Comm
### TODO
1. dns 서버 만들기
2. static NodeManager
3. Node
4. static GlobalServiceManager
5. static LocalServiceManager
6. Comm