BASE VERSION

Start your clients with the command line arguments:
P2PClient.exe ReceivePort SendPort ServerIP ServerListenPort Name

Example, with local listen port 8881, send port 8882, server IP 130.70.82.158, server listen port 8888, unique name Client1:
"P2PClient.exe 8881 8882 130.70.82.158 8888 Client1"

P2PClient.exe can be found in P2PClient/P2PClient/bin/Debug

Start your server with the command line arguments:
P2PServer.exe ServerListenPort

Example, with server listen port 8888:
"P2PServer.exe 8888"

P2PServer.exe can be found in P2PServer/P2PServer/bin/Debug

DISTRIBUTED VERSION

Start P2PChord.exe with command line arguments: P2PChord.exe ListenPort SendPort JoinerIP JoinerPort Name
Example, with local listen port 8881, sender port 8882, joinerIP 130.70.82.158, joiner port 8888, and Name Sender1

"P2pChord.exe 8881 8882 130.70.82.158 8888 Sender1"

P2pChord.exe can be found in P2PChord/P2PChord/bin/Debug

Once running, each client needs to start its listening threads by typing giving the command "-". 

After this, each client can join on the "joiner" client. The "joiner" can finally add itself to the cloud.

After this, each client may add or remove files, or request files, or remove themselves from the cloud



FOR BOTH VERSIONS

File folder is maintained in C:/TempDir1