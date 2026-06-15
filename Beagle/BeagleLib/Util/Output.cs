
namespace BeagleLib.Util;

//public static class OutputWithBuffer
//{
//    #region Properties
//    public static string FileName
//    {
//        get
//        {
//            if (_fileName == null) throw new Exception("FileName is not set");
//            return _fileName;
//        }
//        set
//        {
//            if (StreamWriter != null) throw new Exception($"Cannot set FileName to {value}. It is already set to {_fileName}");
//            _fileName = value;

//            //Check if file already exists
//            if (File.Exists(_fileName)) throw new Exception($"File {_fileName} already exists!");

//            //Create file 
//            // ReSharper disable once AssignNullToNotNullAttribute
//            File.Create(_fileName).Dispose();

//            //Create stream writer. We don't need to worry about disposing it. We need it for the duration of the program
//            StreamWriter = File.AppendText(_fileName);
//            StreamWriter.AutoFlush = true;
//        }
//    }
//    private static string? _fileName;

//    private static StreamWriter? StreamWriter { get; set; }

//    private static readonly char[] _buffer = new char[2048];
//    #endregion
//}

public static class Output
{
    #region Methods
    public static string ReadLine()
    {
        Console.CursorVisible = true;
        var input = Console.ReadLine() ?? "";
        Console.CursorVisible = false;
        WriteLine(input, true);
        return input;
    }
    public static void WriteLineUnlessAtLineStart(bool toFileOnly = false)
    {
        if (Console.CursorLeft > 0) WriteLine(toFileOnly);
    }
    public static void Write(string str, bool toFileOnly = false)
    {
        if (!toFileOnly) Console.Write(str);
        FileStreamWriter?.Write(str);
    }
    public static void WriteLine(string str, bool toFileOnly = false)
    {
        if (!toFileOnly) Console.WriteLine(str);
        FileStreamWriter?.WriteLine(str);
    }
    public static void WriteLine(bool toFileOnly = false)
    {
        if (!toFileOnly) Console.WriteLine();
        FileStreamWriter?.WriteLine();
    }
    public static void Dispose()
    {
        FileStreamWriter?.Flush();
        FileStreamWriter?.Dispose();
        FileStreamWriter = null;
        _fileName = null;
    }
    public static void DisposeAndRename(string newFileName)
    {
        var oldFileName = FileName;
        Dispose();
        File.Move(oldFileName, newFileName);
    }
    public static void FlushFileStream()
    {
        FileStreamWriter?.Flush();
    }

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //private static void ConsoleWrite(string str)
    //{
    //    for(var i=0; i < str.Length; i++) ConsoleStreamWriter.Write(str[i]);
    //    ConsoleStreamWriter.Flush();
    //}
    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //private static void ConsoleWriteLine(string str)
    //{
    //    for (var i = 0; i < str.Length; i++) ConsoleStreamWriter.Write(str[i]);
    //    ConsoleStreamWriter.Write('\n');
    //    ConsoleStreamWriter.Flush();
    //}
    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //private static void ConsoleWriteLine()
    //{
    //    ConsoleStreamWriter.Write('\n');
    //    ConsoleStreamWriter.Flush();
    //}
    #endregion

    #region Properties
    public static string FileName
    {
        get
        {
            if (_fileName == null) throw new Exception("FileName is not set");
            return _fileName;
        }
        set
        {
            if (FileStreamWriter != null) throw new Exception($"Cannot set FileName to {value}. It is already set to {_fileName}");
            _fileName = value;

            //Check if file already exists
            if (File.Exists(_fileName)) throw new Exception($"File {_fileName} already exists!");

            //Create file 
            // ReSharper disable once AssignNullToNotNullAttribute
            File.Create(_fileName).Dispose();

            //Create stream writer. We don't need to worry about disposing it. We need it for the duration of the program
            FileStreamWriter = File.AppendText(_fileName);
            //FileStreamWriter.AutoFlush = true;
        }
    }
    private static string? _fileName;

    private static StreamWriter? FileStreamWriter { get; set; }
    //private static StreamWriter ConsoleStreamWriter { get; set; } = new(Console.OpenStandardOutput());
    #endregion
}