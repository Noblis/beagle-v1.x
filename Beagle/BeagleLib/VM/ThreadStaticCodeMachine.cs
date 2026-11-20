using BeagleLib.VM.DoublePrescision;

namespace BeagleLib.VM;

public static class ThreadStaticCodeMachine
{
    [ThreadStatic] public static CodeMachine Instance;
    [ThreadStatic] public static CodeMachine64 Instance64;
}