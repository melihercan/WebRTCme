SELF SIGNED CERT FOR .NET and IIS Express
=========================================

CREATE
------
openssl req -config webrtcme.config -new -out csr.pem
openssl x509 -req -days 3652 -extfile webrtcme.config -extensions v3_req -in csr.pem -signkey key.pem -out webrtcme.crt
openssl pkcs12 -export -out webrtcme.pfx -inkey key.pem -in webrtcme.crt -password pass:_webrtcme_

CHECK
-----
openssl x509 -in csr.pem -noout -text -inform pem
openssl x509 -in webrtme.crt -noout -text -inform der
openssl.exe pkcs12 -info -in webrtcme.pfx

WINDOWS INSTALL
---------------
Search "Manage computer certificates" and open it. 
Right click on "Personal/Certificates" and import pfx file.
Install crt file to LocalComputer "Trusted Root Certificates Authorities".

KESTREL INSTALLATION (USER)
---------------------------
# Install new certificate for .net core (Kestrel)
dotnet dev-certs https -v --clean --import webrtcme.pfx -p _webrtcme_
dotnet dev-certs https -v --trust

IIS EXPRESS INSTALLATION (SYSTEM)
---------------------------------
# Install new certificate for IIS Express
# Exporting cert to store
- Key is required by IIS, so exporting CER file to LocalComputer\Personal will not work. Search "Manage computer certificates" and open it. 
  Right click on "Personal/Certificates" and import pfx file.
- Export cer file to LocalComputer "Trusted Root Certificates Authorities".

certhash: (get it from Thumprint) ee607218eafba6e178482c7d475195b12ba7098e
Application ID : {214124cd-d05b-4309-9af9-9caa44b2b74a}

# Open administrative command window and go to "\Program Files (x86)\IIS Express".
# show current certificates
netsh http show sslcert

# delete existing certificates
for /L %i in (44300,1,44399) do IisExpressAdminCmd.exe deleteSslUrl -url:https://localhost:%i/

# add new certificates
for /L %i in (44300,1,44399) do netsh http add sslcert ipport=0.0.0.0:%i certhash=ee607218eafba6e178482c7d475195b12ba7098e appid={214124cd-d05b-4309-9af9-9caa44b2b74a}


ANDROID INSTALLATION
--------------------
# Push pfx file to "/sdcard/Download"
adb push webrtcme.cer /sdcard/Download
# On phone Settings, Security, Encription & credentials use "Install from SD card"


IOS SIMULATION INSTALLATION
----------------
# Open simulation on Mac, grag and drop the cer file.