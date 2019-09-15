# Header
    바이트
    0
    1   ConnectionId 2바이트 (커넥션이 필요하지 않는 종류는 이 값 신경 안씀.)
    2
    3   PayloadLength   (최대 약 64KB)
    4   Type
    5   Reserved
    6   Reserved
    7   Reserved
    8   Reserved
    9   Settings    isEncrypted?

# Type
    0:  새로운 커넥션
    1:  기존에 존재해서 사용중인 커넥션
    2:  커넥션 종료알림
    
    10: TimeStamp 교환요청
    11: Node 정보 요청
    12: Node 데이터임을 명시