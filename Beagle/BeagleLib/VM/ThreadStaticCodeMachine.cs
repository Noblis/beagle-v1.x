namespace BeagleLib.VM;

public static class ThreadStaticCodeMachine
{
    [ThreadStatic] public static CodeMachine Instance;
}