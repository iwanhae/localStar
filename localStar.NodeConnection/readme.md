Service	<-ServiceConnection->	Node	<-NodeConnection->	Node	<-ServiceConnection->	Service
App	<-ServiceConnection->

ServiceConnection
	1. 새로 생긴 연결의 경우 HTTP 분석을 통해 목적지가 어딘지 파악해둔다.	(HTTP 분석은 별개 메소드로.)
		NodeManager에 질의를 통해 어느 고유ID로 가야하는지 정한다.	(질의는 URL만)
			NodeManager는 들어온 메세지의 최단거리를 먼저 판단하고, ConnectionManager에 질의를 날려 고유 ID를 얻어온다.
	2. 1의 요청을 포함해서 들어온 요청을 MemoryStream에 쌓아둔다.
	3. MemoryStream에 쌓인 요청을 App에 돌려준다.
	4. 접속된 Port를 고유 ID로 사용.

Service	<-		ServiceConnection			->	Node
	<-	NetworkStream	-><-	MemoryStream	->

ServiceConnectionManager
	1. ServiceConnection에 쌓인 MemoryStream을 불러와 Bytes Array로 만들어준다.
	2. NodeConnectionManager로부터 Bytes Array를 받는다.
	3. Bytes Array의 목적지를 확인하고 ServiceConnection과 NodeConnectionManager 둘중 하나로 보낸다.

NodeConnection
	1. Bytes Array를 다른 Node로 보냄
	2. 받은 데이터를 분석해서 Bytes Array로 만듬.
	3. 접속된 Port를 고유 ID로 사용.
	4. 자체 Dictionary가 존재함
		local고유ID를 자체 넘버링으로 변환
		상대방으로부터 알려지지 않은 넘버링으로 메세지가 날라오면 NodeManager에 질의를 통해 어느 고유ID로 가야할 메세지인지 정함.
			NodeManager는 들어온 메세지의 최단거리를 먼저 판단하고, ConnectionManager에 질의를 날려 고유 ID를 얻어온다.
			넘버링은 2바이트의 공간을 가지며 우선순위가 높은쪽이 1~32,767까지, 낮은쪽이 -32,768~-1 까지 사용한다.
			넘버링은 새로 생성할때마다 하나씩 커지며, 최대값이후는 다시 0부터 시작한다.

NodeConnectionManager
	1. NodeConnection과 ServiceConnectionManager로부터 Bytes Array를 받는다.
	2. Bytes Array를 분석해 NodeConnections와 ServiceConnectionManger 중 어디로 보낼지 결정후 호출한다.
	3. 각 Connections들을 모니터링한다.


Node		<-	NodeConnection	->		Node
Bytes	->	<-	NetworkStream	->	<-	Bytes
Bytes	->					<-	Bytes

NodeConnectionManager
	1. 


|--							Node								--|
<-	ServiceConnections	-><-	ServiceConnectionManager	-><-	NodeConnectionManager	-><-	NodeConnections 	->
<-	NetworkStreams	-><- 	MemoryStream	 -><- BytesArrays	-><-		BytesArrays 		-><-	NetworkStreams	->


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
2. Comm
    10바이트 헤더 + Body 무한반복.

    바이트
    0
    1   ConnectionId 2바이트
    2
    3   PayloadLength   (최대 약 64KB)
    4   Reserved
    5   Reserved
    6   Reserved
    7   Reserved
    8   Settings
    9   Settings
3. End Transmission