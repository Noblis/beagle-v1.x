using BeagleLib.VM.DoublePrescision;

namespace BeagleLib.VM;

public static class ThreadStaticCodeMachine
{
    [ThreadStatic] public static CodeMachine64 Instance64;
    [ThreadStatic] public static CodeMachine Instance;
}