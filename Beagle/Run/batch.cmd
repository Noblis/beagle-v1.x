dotnet build -c release
cd bin\Release\net10.0
rem for /l %%i in (1, 1, 10) do Run.exe RunFeynman=5 StopAfterMin=10 NoEscMenu
for /l %%i in (1, 1, 10) do Run.exe RunFeynman=%%i StopAfterMin=10 NoEscMenu
for /l %%i in (1, 1, 10) do Run.exe RunFeynman=14 StopAfterMin=10 NoEscMenu
rem for /l %%i in (1, 1, 10) do Run.exe RunFeynman=18 StopAfterMin=10 NoEscMenu
rem for /l %%i in (1, 1, 10) do Run.exe RunFeynman=20 StopAfterMin=10 NoEscMenu
rem for /l %%i in (1, 1, 10) do Run.exe RunFeynman=21 StopAfterMin=10 NoEscMenu
rem for /l %%i in (1, 1, 10) do Run.exe RunFeynman=29 StopAfterMin=10 NoEscMenu
for /l %%i in (1, 1, 10) do Run.exe RunFeynman=31 StopAfterMin=10 NoEscMenu
rem for /l %%i in (1, 1, 10) do Run.exe RunFeynman=38 StopAfterMin=10 NoEscMenu
for /l %%i in (1, 1, 10) do Run.exe RunFeynman=43 StopAfterMin=10 NoEscMenu
rem for /l %%i in (1, 1, 10) do Run.exe RunFeynman=44 StopAfterMin=10 NoEscMenu
rem for /l %%i in (1, 1, 10) do Run.exe RunFeynman=56 StopAfterMin=10 NoEscMenu
for /l %%i in (1, 1, 10) do Run.exe RunFeynman=57 StopAfterMin=10 NoEscMenu
rem for /l %%i in (1, 1, 10) do Run.exe RunFeynman=72 StopAfterMin=10 NoEscMenu
for /l %%i in (1, 1, 10) do Run.exe RunFeynman=80 StopAfterMin=10 NoEscMenu
rem for /l %%i in (1, 1, 10) do Run.exe RunFeynman=86 StopAfterMin=10 NoEscMenu
rem for /l %%i in (1, 1, 10) do Run.exe RunFeynman=87 StopAfterMin=10 NoEscMenu
rem for /l %%i in (1, 1, 10) do Run.exe RunFeynman=90 StopAfterMin=10 NoEscMenu
for /l %%i in (1, 1, 10) do Run.exe RunFeynman=91 StopAfterMin=10 NoEscMenu
pause
