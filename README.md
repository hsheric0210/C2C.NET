# C2C.NET :: Covert & secure **C2** **C**ommunication

A simple project that opens or hosts a lightweight C2 communication channel.

Will both support P2P and Centralized C2 server model. Currently aiming on Centralized C2 model.

* [ ] Multiple/Configurable fallback servers
* [ ] P2P

## Supported communication protocols

### Low-level Sockets

* [ ] TCP
* [ ] UDP

### High-level Sockets

If you chose to use high-level transmission protocols, C2C.NET will automatically *conceal* the data inside the legitimate-looking traffic. (like Steganography)

#### Request-Response

* [ ] HTTP(S) (HTTPS with whitelisted cert fingerprint)
* [ ] FTP(S) (HTTPS with whitelisted cert fingerprint)
* [ ] DNS

#### Duplex

* [ ] WebSocket (Similar to HTTP)
* [ ] SSH

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
