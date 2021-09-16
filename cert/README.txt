SELF SIGNED CERT FOR .NET and IIS Express
=========================================

CREATE
------
openssl req -config localhost.config -new -out csr.pem
openssl x509 -req -days 3652 -sha256 -extfile localhost.config -extensions v3_req -in csr.pem -signkey key.pem -out localhost.crt
openssl pkcs12 -export -out localhost.pfx -inkey key.pem -in localhost.crt -password pass:_localhost_
openssl x509 -in localhost.crt -out localhost.pem

CHECK
-----
##openssl x509 -in csr.pem -noout -text -inform pem
##openssl x509 -in localhost.crt -noout -text -inform der
openssl.exe pkcs12 -info -in localhost.pfx

WINDOWS INSTALL
---------------
Search "Manage computer certificates" and open it. 
Right click on "Personal/Certificates" and import pfx file.
Install crt file to LocalComputer "Trusted Root Certificates Authorities".

KESTREL INSTALLATION (USER)
---------------------------
# Install new certificate for .net core (Kestrel)
dotnet dev-certs https -v --clean --import localhost.pfx -p _localhost_
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
# Push crt file to "/sdcard/Download"
adb push localhost.crt /sdcard/Download
# On phone Settings, Security, Encription & credentials use "Install from SD card"


IOS SIMULATION INSTALLATION
----------------
# Open simulation on Mac, grag and drop the cer file.

IOS PHONE INSTALL
-----------------
- email PEM file
- open on the phone and install
- General->About->Certificate Trus Serttings, enable full trust
USE SAFARI - CHROME DOES NOT WORK