set r=25
set s=10
dotnet build -c release
cd bin\Release\net10.0
rem for /l %%i in (1, 1, %%r) do Run.exe RunFeynman=5 StopAfterMin=%%s NoEscMenu
for /l %%i in (1, 1, %r%) do Run.exe RunFeynman=7 StopAfterMin=%s% NoEscMenu
for /l %%i in (1, 1, %%r) do Run.exe RunFeynman=14 StopAfterMin=%%s NoEscMenu
rem for /l %%i in (1, 1, %%r) do Run.exe RunFeynman=18 StopAfterMin=%%s NoEscMenu
rem for /l %%i in (1, 1, %%r) do Run.exe RunFeynman=20 StopAfterMin=%%s NoEscMenu
rem for /l %%i in (1, 1, %%r) do Run.exe RunFeynman=21 StopAfterMin=%%s NoEscMenu
rem for /l %%i in (1, 1, %%r) do Run.exe RunFeynman=29 StopAfterMin=%%s NoEscMenu
for /l %%i in (1, 1, %%r) do Run.exe RunFeynman=31 StopAfterMin=%%s NoEscMenu
rem for /l %%i in (1, 1, %%r) do Run.exe RunFeynman=38 StopAfterMin=%%s NoEscMenu
for /l %%i in (1, 1, %%r) do Run.exe RunFeynman=43 StopAfterMin=%%s NoEscMenu
rem for /l %%i in (1, 1, %%r) do Run.exe RunFeynman=44 StopAfterMin=%%s NoEscMenu
rem for /l %%i in (1, 1, %%r) do Run.exe RunFeynman=56 StopAfterMin=%%s NoEscMenu
for /l %%i in (1, 1, %%r) do Run.exe RunFeynman=57 StopAfterMin=%%s NoEscMenu
rem for /l %%i in (1, 1, %%r) do Run.exe RunFeynman=72 StopAfterMin=%%s NoEscMenu
for /l %%i in (1, 1, %%r) do Run.exe RunFeynman=80 StopAfterMin=%%s NoEscMenu
rem for /l %%i in (1, 1, %%r) do Run.exe RunFeynman=86 StopAfterMin=%%s NoEscMenu
rem for /l %%i in (1, 1, %%r) do Run.exe RunFeynman=87 StopAfterMin=%%s NoEscMenu
rem for /l %%i in (1, 1, %%r) do Run.exe RunFeynman=90 StopAfterMin=%%s NoEscMenu
for /l %%i in (1, 1, %%r) do Run.exe RunFeynman=91 StopAfterMin=%%s NoEscMenu
pause
