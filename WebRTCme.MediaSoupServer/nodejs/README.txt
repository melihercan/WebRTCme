- install node
- set PATH to MSBuild.exe, for example:
    set PATH=C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\amd64;%PATH%
- set python version 2.7, for example:
    set PATH=C:\Program Files\Python27;%PATH%
- set environment variable GYP_MSVS_VERSION to "2019" for VS version, for example:
    set GYP_MSVS_VERSION=2019
- build module:
    npm install mediasoup