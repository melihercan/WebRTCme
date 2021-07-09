WINDOWS REQUIRES THE FOLLOWING SETTINGS:
- install node
- install python27
- set following environmental variables
    o set PATH to MSBuild.exe, for example: C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\amd64
    o set python version 2.7 PATH before 3.x, for example: C:\Program Files\Python27
    o GYP_MSVS_VERSION to VS version, for example: GYP_MSVS_VERSION=2019
Hard coded values may require some changes based on your environment.

To create node_modules, right click 'package.json' and select 'Restore Packages'.

For some reason heapdump is reporting problems. To solve it run:
'npm install heapdump' from command line.


