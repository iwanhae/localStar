Connection namespace에서 사용할 bytes array가 기본인 메세지 객체.

1.  URL         메세지가 가야할 서비스의 종류
2.  localTo     그러기 위해 도달할 Connection localId
    localId 는 short형식, 연결된 Port를 따라감.
3.  localFrom   메세지가 생성된 Connection  localId
4.  data        데이터 원본
5.  length      길이
6.  type
    Initial         생성되서 처음으로 발생한 메세지
    Ordinary        일반적인 데이터 메세지
    Termination     연결 종료를 알리는 메세지


    localTo가 0인 경우는 Node 정보 교환정보임.