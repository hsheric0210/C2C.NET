# C2C.NET :: Covert & secure **C2** **C**ommunication

A simple project that opens or hosts a lightweight C2 communication channel.

Will both support P2P and Centralized C2 server model. Currently aiming on Centralized C2 model.

* [ ] Multiple/Configurable fallback servers
* [ ] P2P

## Supported communication protocols

### Duplex

All protocol supports both Bind(Server) and Connect(Client) mode.
(e.g. tcp-bind, tcp-connect)

In case of Duplex Channel usage, one side should be 'Bind(Server)' and the other side should be 'Connect(Client)'.
(No 'bind-bind' or 'connect-connect' communication is possible; only 'bind-connect' form is allowed)

#### When using HTTP or WebSocket

The HTTP server (http_bind and ws_bind) implementation is based on **HTTP.sys**.

This means that you must run `netsh http add urlacl url=http://+:{port}/ user={domain}\{username}` to reserve URL namespace. (Or you will receive 'Access Denied' error)

Replace the `{port}`, `{domain}`, `{username}` placeholder to your desired port number, computor name, and user account name.

To delete URL namespace reservation, run `netsh http remove urlacl url=http://+:{port}/`.

Learn more information about this: <https://learn.microsoft.com/ko-kr/windows-server/networking/technologies/netsh/netsh-http>

#### Sockets

* [x] TCP
* UDP - Not supported because of its low confidence
* [x] WebSocket (Similar to HTTP)
* Telnet - It is just a simple wrapper over the TCP socket
* [ ] SSH

#### Request-Response

* [ ] TODO: If you chose to use high-level transmission protocols, C2C.NET will automatically *conceal* the data inside the legitimate-looking traffic. (like Steganography)

* [x] HTTP
* [ ] HTTPS (pinned certificate)

* [ ] FTP
* [ ] FTPS (pinned certificate)
* [ ] DNS

### Public Services

#### File Share (One-way; C2 -> Victim)

* [ ] Google Drive
* [ ] Dropbox
* [ ] MEGA
* [ ] MediaFire

#### Snippet Share (One-way; C2 -> Victim)

* [ ] Pastebin
* [ ] JSFiddle
* [ ] GitHub Gists
* [ ] GitLab Snippets
* [ ] Padlet
* [ ] Notion

#### Messenger Services

* [ ] Twitter (X)
* [ ] Discord (**NOT** Discord WebHook)
* [ ] Mastodon
* [ ] Signal
* [ ] Slack

#### Mail Services

* [ ] GMail
* [ ] Yahoo/AOL Mail
* [ ] Outlook Mail
* [ ] GMX Mail

## Traffic concealing

* [ ] Additional symmetric-key encryption layer
* [ ] Modified binary-to-text encoding (like Base64)
* [ ] Hide data in legitimate text, document, or image files (Steganography)

### Additional encryption layer

* [ ] AES-256 with *static* encryption key

* [ ] AES-256 with *ephemereal* encryption key (+ Simple DH handshake)
    * DH: Diffie-Hellman
    * ECDH: Elliptic-curve Diffie-Hellman

* [ ] Support additional cipher and key agreement suites (with BouncyCastle dependency)

## Related Repositories / Documentations

* https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.protocoltype
* https://llyllyll.tistory.com/entry/telnet-%ED%8C%A8%ED%82%B7%EB%B6%84%EC%84%9D
* https://www.google.com/search?q=ssh+packet&sourceid=chrome&ie=UTF-8
* https://blog.naver.com/PostView.nhn?blogId=sujunghan726&logNo=220316487529
* https://peemangit.tistory.com/57
* https://darksoulstory.tistory.com/66
* https://en.wikipedia.org/wiki/Uuencoding
* https://blog.naver.com/sik7854/221818621216
* https://llyllyll.tistory.com/entry/telnet-%ED%8C%A8%ED%82%B7%EB%B6%84%EC%84%9D
* https://github.com/feresg/java-steganography/tree/master/src/Steganography/Logic
* https://gist.github.com/AndreCAndersen/78b38ef60b402c7f1b7566e091941d0a
* https://github.com/DimitarPetrov/stegify
* https://github.com/JoshuaKasa/van-gonography
* https://github.com/computationalcore/cryptosteganography
* https://github.com/M4cs/pixcryption
* https://github.com/nikibobi/pastebin-csharp
* https://github.com/PaulSec/twittor
* https://github.com/byt3bl33d3r/gcat
* https://github.com/maldevel/gdog
