# LocalStar

### Comm
### TODO - SHORT TERM
1. Message 에서 url로 초기화 부분 구현
    1. Node에서 최단경로 찾는부분 구현
    2. NodeConnectionManager 에서 해당 노드로 가는길 찾는부분 구현
    3. ServiceConnectionManager 에서 해당 서비스로 가는길 찾는부분 구현
2. NodeStream 에서 Header 및 통신부분 구현
    1. Message 에서 직접 Header 만드는 방향으로

### TODO - LONG TERM
1. dns 서버 만들기
2. static NodeManager
3. Node
4. static GlobalServiceManager
5. static LocalServiceManager
6. Comm

### Node 데이터 순환구조.
    1. 처음 접속한 노드는 주변 노드에 접속을 시도하며 바로 노드 데이터 요청 메세지를 전송함.
    2. Connected Node에 한해 주기적으로 노드정보를 요청함.
    3. 요청 주기는 1KB/s가 되도록.

CurrentNode                             nodeId: 0
ConnectedNodes                      1   2   3   4   5
KnowNodes                           2   3   4   5   1   