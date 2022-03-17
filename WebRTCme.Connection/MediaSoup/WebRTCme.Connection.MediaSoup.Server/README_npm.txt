WINDOWS REQUIRES THE FOLLOWING SETTINGS:
- install latest node (64 bit)
- install python27
- vs2019 is required (2022 is not recognized in mediasoup build script!!!)
- set following environmental variables
    o set PATH to MSBuild.exe, for example: C:\Program Files\Microsoft Visual Studio\2022\Community\Msbuild\Current\Bin\amd64
    o set python version 2.7 PATH before 3.x, for example: C:\Program Files\Python27
    o GYP_MSVS_VERSION to VS version, for example: GYP_MSVS_VERSION=2019 (2022 is not recognized)
Hard coded values may require some changes based on your environment.

To create node_modules, right click 'package.json' and select 'Restore Packages'.

For some reason heapdump is reporting problems. To solve it:
- remove node_modules\heapdump folder
- then run 'npm install heapdump' from command line.


