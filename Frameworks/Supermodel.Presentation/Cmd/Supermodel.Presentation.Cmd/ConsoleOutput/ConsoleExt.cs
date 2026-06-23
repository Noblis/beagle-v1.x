using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Supermodel.Presentation.Cmd.Models;

namespace Supermodel.Presentation.Cmd.ConsoleOutput;

public static class ConsoleExt
{
    #region EmbeddedTypes
    public class SelectListItem(string value, string label)
    {
        #region Constructors

        public SelectListItem(string value) : this(value, value)
        {
        }
        #endregion

        #region Properties
        public string Value { get; } = value;
        public string Label { get; } = label;

        #endregion

        #region Static constants
        public static SelectListItem Empty { get; } = new("", "----");
        #endregion
    }
    #endregion
        
    #region Date
    public static DateTime? EditDate(DateTime? value, StringWithColor? placeholder)
    {
        placeholder ??= StringWithColor.Empty;
            
        var valueStr = value != null ? value.Value.ToString("MM/dd/yyyy") : "";
        while(true)
        {
            var input = EditLine(valueStr, placeholder, IsValidDateChar);
            if (string.IsNullOrWhiteSpace(input)) 
            {
                if (!CmdContext.IsPropertyRequired) return null;
                PrintRequiredFieldMessage();
                continue;
            }

            if (DateTime.TryParse(input, out var result))
            {
                return result;
            }
            else
            {
                PrintErrorMessage();
                valueStr = input;
            }
        }
    }
    #endregion

    #region Edit Boolean
    public static bool EditBool(bool? value)
    {
        var valueStr = value == true ? "Yes" : "No";
        var result = EditDropdownList(valueStr, BoolOptions);
        return result == "Yes";
    }
    private static SelectListItem[] BoolOptions { get; } = [new("Yes"), new("No")];
    #endregion

    #region Edit Floating Point
    public static decimal? EditFloat(decimal? value)
    {
        // ReSharper disable once ConstantNullCoalescingCondition
        var valueStr = value.ToString() ?? "";
        while(true)
        {
            var input = EditLine(valueStr, StringWithColor.Empty, IsValidFloatingPoint);
            if (string.IsNullOrWhiteSpace(input)) 
            {
                if (!CmdContext.IsPropertyRequired) return null;
                PrintRequiredFieldMessage();
                continue;
            }

            if (decimal.TryParse(input, out var result))
            {
                return result;
            }
            else
            {
                PrintErrorMessage();
                valueStr = input;
            }
        }
    }
    public static double? EditFloat(double? value)
    {
        // ReSharper disable once ConstantNullCoalescingCondition
        var valueStr = value.ToString() ?? "";
        while(true)
        {
            var input = EditLine(valueStr, StringWithColor.Empty, IsValidFloatingPoint);
            if (string.IsNullOrWhiteSpace(input)) 
            {
                if (!CmdContext.IsPropertyRequired) return null;
                PrintRequiredFieldMessage();
                continue;
            }

            if (double.TryParse(input, out var result))
            {
                return result;
            }
            else
            {
                PrintErrorMessage();
                valueStr = input;
            }
        }
    }
    public static float? EditFloat(float? value)
    {
        // ReSharper disable once ConstantNullCoalescingCondition
        var valueStr = value.ToString() ?? "";
        while(true)
        {
            var input = EditLine(valueStr, StringWithColor.Empty, IsValidFloatingPoint);
            if (string.IsNullOrWhiteSpace(input)) 
            {
                if (!CmdContext.IsPropertyRequired) return null;
                PrintRequiredFieldMessage();
                continue;
            }

            if (float.TryParse(input, out var result))
            {
                return result;
            }
            else
            {
                PrintErrorMessage();
                valueStr = input;
            }
        }
    }
    #endregion
        
    #region Edit Integer
    public static ulong? EditInteger(ulong? value)
    {
        // ReSharper disable once ConstantNullCoalescingCondition
        var valueStr = value.ToString() ?? "";
        while(true)
        {
            var input = EditLine(valueStr, StringWithColor.Empty, IsValidUnsignedIntegerChar);
            if (string.IsNullOrWhiteSpace(input)) 
            {
                if (!CmdContext.IsPropertyRequired) return null;
                PrintRequiredFieldMessage();
                continue;
            }

            if (ulong.TryParse(input, out var result))
            {
                return result;
            }
            else
            {
                PrintErrorMessage();
                valueStr = input;
            }
        }
    }
    public static uint? EditInteger(uint? value)
    {
        // ReSharper disable once ConstantNullCoalescingCondition
        var valueStr = value.ToString() ?? "";
        while(true)
        {
            var input = EditLine(valueStr, StringWithColor.Empty, IsValidUnsignedIntegerChar);
            if (string.IsNullOrWhiteSpace(input)) 
            {
                if (!CmdContext.IsPropertyRequired) return null;
                PrintRequiredFieldMessage();
                continue;
            }

            if (uint.TryParse(input, out var result))
            {
                return result;
            }
            else
            {
                PrintErrorMessage();
                valueStr = input;
            }
        }
    }
    public static ushort? EditInteger(ushort? value)
    {
        // ReSharper disable once ConstantNullCoalescingCondition
        var valueStr = value.ToString() ?? "";
        while(true)
        {
            var input = EditLine(valueStr, StringWithColor.Empty, IsValidUnsignedIntegerChar);
            if (string.IsNullOrWhiteSpace(input)) 
            {
                if (!CmdContext.IsPropertyRequired) return null;
                PrintRequiredFieldMessage();
                continue;
            }

            if (ushort.TryParse(input, out var result))
            {
                return result;
            }
            else
            {
                PrintErrorMessage();
                valueStr = input;
            }
        }
    }
    public static byte? EditInteger(byte? value)
    {
        // ReSharper disable once ConstantNullCoalescingCondition
        var valueStr = value.ToString() ?? "";
        while(true)
        {
            var input = EditLine(valueStr, StringWithColor.Empty, IsValidUnsignedIntegerChar);
            if (string.IsNullOrWhiteSpace(input)) 
            {
                if (!CmdContext.IsPropertyRequired) return null;
                PrintRequiredFieldMessage();
                continue;
            }

            if (byte.TryParse(input, out var result))
            {
                return result;
            }
            else
            {
                PrintErrorMessage();
                valueStr = input;
            }
        }
    }
        
    public static long? EditInteger(long? value)
    {
        // ReSharper disable once ConstantNullCoalescingCondition
        var valueStr = value.ToString() ?? "";
        while(true)
        {
            var input = EditLine(valueStr, StringWithColor.Empty, IsValidIntegerChar);
            if (string.IsNullOrWhiteSpace(input)) 
            {
                if (!CmdContext.IsPropertyRequired) return null;
                PrintRequiredFieldMessage();
                continue;
            }

            if (long.TryParse(input, out var result))
            {
                return result;
            }
            else
            {
                PrintErrorMessage();
                valueStr = input;
            }
        }
    }
    public static int? EditInteger(int? value)
    {
        // ReSharper disable once ConstantNullCoalescingCondition
        var valueStr = value.ToString() ?? "";
        while(true)
        {
            var input = EditLine(valueStr, StringWithColor.Empty, IsValidIntegerChar);
            if (string.IsNullOrWhiteSpace(input)) 
            {
                if (!CmdContext.IsPropertyRequired) return null;
                PrintRequiredFieldMessage();
                continue;
            }

            if (int.TryParse(input, out var result))
            {
                return result;
            }
            else
            {
                PrintErrorMessage();
                valueStr = input;
            }
        }
    }
    public static short? EditInteger(short? value)
    {
        // ReSharper disable once ConstantNullCoalescingCondition
        var valueStr = value.ToString() ?? "";
        while(true)
        {
            var input = EditLine(valueStr, StringWithColor.Empty, IsValidIntegerChar);
            if (string.IsNullOrWhiteSpace(input)) 
            {
                if (!CmdContext.IsPropertyRequired) return null;
                PrintRequiredFieldMessage();
                continue;
            }

            if (short.TryParse(input, out var result))
            {
                return result;
            }
            else
            {
                PrintErrorMessage();
                valueStr = input;
            }
        }
    }
    public static sbyte? EditInteger(sbyte? value)
    {
        // ReSharper disable once ConstantNullCoalescingCondition
        var valueStr = value.ToString() ?? "";
        while(true)
        {
            var input = EditLine(valueStr, StringWithColor.Empty, IsValidIntegerChar);
            if (string.IsNullOrWhiteSpace(input)) 
            {
                if (!CmdContext.IsPropertyRequired) return null;
                PrintRequiredFieldMessage();
                continue;
            }

            if (sbyte.TryParse(input, out var result))
            {
                return result;
            }
            else
            {
                PrintErrorMessage();
                valueStr = input;
            }
        }
    }
    #endregion
        
    #region Edit String & Password
    public static string EditString(string value)
    {
        while(true)
        {
            var input = EditLine(value, StringWithColor.Empty, x => char.IsLetterOrDigit(x) || char.IsSymbol(x) || char.IsPunctuation(x) || char.IsSeparator(x));
            if (string.IsNullOrWhiteSpace(input)) 
            {
                if (!CmdContext.IsPropertyRequired) return "";
                PrintRequiredFieldMessage();
                continue;
            }
            return input;
        }
    }
    public static string ReadPassword(StringWithColor? placeholder = null)
    {
        placeholder ??= StringWithColor.Empty;
            
        while(true)
        {
            var input = ReadPasswordLine(placeholder, x => char.IsLetterOrDigit(x) || char.IsSymbol(x) || char.IsPunctuation(x) || char.IsSeparator(x));
            if (string.IsNullOrWhiteSpace(input)) 
            {
                if (!CmdContext.IsPropertyRequired) return "";
                PrintRequiredFieldMessage();
                continue;
            }
            return input;
        }
    }
    #endregion

    #region Low Level Edit Line
    public static string EditLine(string value, StringWithColor placeholder, Func<char, bool> isValidCharFunc)
    {
        if (value.Contains('\n')) throw new ArgumentException("Cannot contain new line", nameof(value));
            
        var cursorIdx = value.Length;
        var cursorLeft = Console.CursorLeft;
        var cursorTop = Console.CursorTop;

        if (!string.IsNullOrEmpty(value)) Console.Write(value);
        else placeholder.WriteToConsole();

        var chars = new List<char>();
        if (!string.IsNullOrEmpty(value)) chars.AddRange(value.ToCharArray());

        while (true)
        {
            var info = Console.ReadKey(true);
            if (info.Key == ConsoleKey.Backspace)
            {
                if (cursorIdx > 0)
                {
                    cursorIdx--;
                    chars.RemoveAt(cursorIdx);
                }

                Console.CursorVisible = false;
                UpdateText();
                SetCursorPosition();
                Console.CursorVisible = true;
            }
            else if (info.Key == ConsoleKey.LeftArrow)
            {
                if (cursorIdx > 0) cursorIdx--;

                Console.CursorVisible = false;
                if (cursorIdx <= 0) UpdateText();
                SetCursorPosition();
                Console.CursorVisible = true;
            }
            else if (info.Key == ConsoleKey.RightArrow)
            {
                if (cursorIdx < chars.Count) cursorIdx++;

                Console.CursorVisible = false;
                if (cursorIdx >= chars.Count) UpdateText();
                SetCursorPosition();
                Console.CursorVisible = true;
            }
            else if (info.Key == ConsoleKey.Enter) 
            { 
                Console.WriteLine(); 
                break; 
            }
            else if (info.Key == ConsoleKey.Escape)
            {
                if (CmdContext.CtrlEscEnabled && info.Modifiers.HasFlag(ConsoleModifiers.Shift)) throw new ShiftEscException();
                    
                Console.CursorTop = cursorTop;
                Console.CursorLeft = cursorLeft;
                Console.Write("".PadRight(chars.Count));

                Console.CursorTop = cursorTop;
                Console.CursorLeft = cursorLeft;
                Console.WriteLine(value);

                chars.Clear();
                if (!string.IsNullOrEmpty(value)) chars.AddRange(value.ToCharArray());
                break;
            }
            else if (isValidCharFunc.Invoke(info.KeyChar))
            {
                chars.Insert(cursorIdx, info.KeyChar);
                cursorIdx++;

                Console.CursorVisible = false;
                UpdateText();
                SetCursorPosition();
                Console.CursorVisible = true;
            }
        }
        return new string(chars.ToArray());

        void UpdateText()
        {
            Console.CursorTop = cursorTop;
            Console.CursorLeft = cursorLeft;
            if (chars.Count > 0)
            {
                var newValue = new string(chars.ToArray());
                Console.Write(newValue);
            }
            else
            {
                var currentColors = FBColors.FromCurrent();
                placeholder.WriteToConsole();
                currentColors.SetColors();
            }
            var padding = Math.Max(placeholder.Content.Length - chars.Count + 1, 1);
            Console.Write("".PadRight(padding));
        }

        void SetCursorPosition()
        {
            Console.CursorLeft = (cursorLeft + cursorIdx) % Console.WindowWidth;
            Console.CursorTop = cursorTop + (cursorLeft + cursorIdx) / Console.WindowWidth;
        }
    }
    public static string ReadPasswordLine(StringWithColor placeholder, Func<char, bool> isValidCharFunc)
    {
        var value = "";

        var cursorIdx = value.Length;
        var cursorLeft = Console.CursorLeft;
        var cursorTop = Console.CursorTop;

        placeholder.WriteToConsole();
        var chars = new List<char>();
        if (!string.IsNullOrEmpty(value)) chars.AddRange(value.ToCharArray());

        while (true)
        {
            var info = Console.ReadKey(true);
            if (info.Key == ConsoleKey.Backspace)
            {
                if (cursorIdx > 0)
                {
                    cursorIdx--;
                    chars.RemoveAt(cursorIdx);
                }

                Console.CursorVisible = false;
                UpdateText();
                SetCursorPosition();
                Console.CursorVisible = true;
            }
            else if (info.Key == ConsoleKey.LeftArrow)
            {
                if (cursorIdx > 0) cursorIdx--;

                Console.CursorVisible = false;
                if (cursorIdx <= 0) UpdateText();
                SetCursorPosition();
                Console.CursorVisible = true;
            }
            else if (info.Key == ConsoleKey.RightArrow)
            {
                if (cursorIdx < chars.Count) cursorIdx++;

                Console.CursorVisible = false;
                if (cursorIdx >= chars.Count) UpdateText();
                SetCursorPosition();
                Console.CursorVisible = true;
            }
            else if (info.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                break;
            }
            else if (info.Key == ConsoleKey.Escape)
            {
                if (CmdContext.CtrlEscEnabled && info.Modifiers.HasFlag(ConsoleModifiers.Shift)) throw new ShiftEscException();

                Console.CursorTop = cursorTop;
                Console.CursorLeft = cursorLeft;
                Console.Write("".PadRight(chars.Count));

                Console.CursorTop = cursorTop;
                Console.CursorLeft = cursorLeft;
                Console.WriteLine(value);

                chars.Clear();
                if (!string.IsNullOrEmpty(value)) chars.AddRange(value.ToCharArray());
                break;
            }
            else if (isValidCharFunc.Invoke(info.KeyChar))
            {
                chars.Insert(cursorIdx, info.KeyChar);
                cursorIdx++;

                Console.CursorVisible = false;
                UpdateText();
                SetCursorPosition();
                Console.CursorVisible = true;
            }
        }
        return new string(chars.ToArray());

        void UpdateText()
        {
            Console.CursorTop = cursorTop;
            Console.CursorLeft = cursorLeft;
            for (var i = 0; i < chars.Count; i++) Console.Write('*');
            var padding = Math.Max(placeholder.Content.Length - chars.Count + 1, 1);
            Console.Write("".PadRight(padding));
        }

        void SetCursorPosition()
        {
            Console.CursorLeft = (cursorLeft + cursorIdx) % Console.WindowWidth;
            Console.CursorTop = cursorTop + (cursorLeft + cursorIdx) / Console.WindowWidth;
        }
    }
    public static string EditLineAllCaps(string value, Func<char, bool> isValidCharFunc)
    {
        if (value.Contains('\n')) throw new ArgumentException("Cannot contain new line", nameof(value));
            
        value = value.ToUpper();
            
        var cursorIdx = value.Length;
        var cursorLeft = Console.CursorLeft;
        var cursorTop = Console.CursorTop;

        Console.Write(value);
        var chars = new List<char>();
        if (!string.IsNullOrEmpty(value)) chars.AddRange(value.ToCharArray());

        while (true)
        {
            var info = Console.ReadKey(true);
            var infoKeyChar = char.ToUpper(info.KeyChar);

            if (info.Key == ConsoleKey.Backspace && cursorIdx > 0)
            {
                cursorIdx--;
                chars.RemoveAt(cursorIdx);
                    
                Console.CursorVisible = false;
                UpdateText();
                SetCursorPosition();
                Console.CursorVisible = true;
            }
            else if (info.Key == ConsoleKey.LeftArrow && cursorIdx > 0)
            { 
                cursorIdx--;

                Console.CursorVisible = false;
                SetCursorPosition();
                Console.CursorVisible = true;
            }
            else if (info.Key == ConsoleKey.RightArrow && cursorIdx < chars.Count)
            { 
                cursorIdx++;

                Console.CursorVisible = false;
                SetCursorPosition();
                Console.CursorVisible = true;
            }
            else if (info.Key == ConsoleKey.Enter) 
            { 
                Console.WriteLine(); 
                break; 
            }
            else if (info.Key == ConsoleKey.Escape)
            {
                if (CmdContext.CtrlEscEnabled && info.Modifiers.HasFlag(ConsoleModifiers.Shift)) throw new ShiftEscException();
                    
                Console.CursorTop = cursorTop;
                Console.CursorLeft = cursorLeft;
                Console.Write("".PadRight(chars.Count));

                Console.CursorTop = cursorTop;
                Console.CursorLeft = cursorLeft;
                Console.WriteLine(value);

                chars.Clear();
                if (!string.IsNullOrEmpty(value)) chars.AddRange(value.ToCharArray());
                break;
            }
            else if (isValidCharFunc.Invoke(infoKeyChar))
            {
                chars.Insert(cursorIdx, infoKeyChar);
                cursorIdx++;

                Console.CursorVisible = false;
                UpdateText();
                SetCursorPosition();
                Console.CursorVisible = true;
            }
        }
        return new string(chars.ToArray());

        void UpdateText()
        {
            Console.CursorTop = cursorTop;
            Console.CursorLeft = cursorLeft;
            var newValue = new string(chars.ToArray());
            Console.Write(newValue);
            Console.Write(' ');
        }

        void SetCursorPosition()
        {
            Console.CursorLeft = (cursorLeft + cursorIdx) % Console.WindowWidth;
            Console.CursorTop = cursorTop + (cursorLeft + cursorIdx) / Console.WindowWidth;
        }
    }
    #endregion

    #region Low Level Dropdown List
    public static string EditDropdownList(string value, IEnumerable<SelectListItem> options)
    {
        var savedCursorVisible = true;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            savedCursorVisible = Console.CursorVisible;
            Console.CursorVisible = false;
        }

        var optionsArray = options.ToArray();
        var selectedOption = optionsArray.Single(x => x.Value == value);
        bool originalValue = true;
            
        var cursorLeft = Console.CursorLeft;
        var cursorTop = Console.CursorTop;
        var maxLenPlus2 = optionsArray.Select(x => x.Label.Length).Max() + 2;

        while(true)
        {
            PrintOption(selectedOption!, maxLenPlus2, cursorLeft, cursorTop);

            var info = Console.ReadKey(true);
            if (info.Key == ConsoleKey.Enter) 
            {
                if (CmdContext.IsPropertyRequired && string.IsNullOrEmpty(selectedOption?.Value))
                {
                    Console.WriteLine();
                    PrintRequiredFieldMessage();
                        
                    cursorLeft = Console.CursorLeft;
                    cursorTop = Console.CursorTop;
                    maxLenPlus2 = optionsArray.Select(x => x.Label.Length).Max() + 2;

                    continue;
                }
                    
                Console.WriteLine();
                Console.CursorVisible = savedCursorVisible;
                return selectedOption!.Value;
            }
            if (info.Key == ConsoleKey.Escape) 
            {
                if (CmdContext.CtrlEscEnabled && info.Modifiers.HasFlag(ConsoleModifiers.Shift)) throw new ShiftEscException();
                    
                selectedOption = optionsArray.Single(x => x.Value == value);
                PrintOption(selectedOption, maxLenPlus2, cursorLeft, cursorTop);
                Console.WriteLine();
                Console.CursorVisible = savedCursorVisible;
                return selectedOption.Value;
            }
            if (info.Key == ConsoleKey.DownArrow)
            {
                if (originalValue)
                {
                    var newSelectedOption = optionsArray.First();
                    if (newSelectedOption.Value == selectedOption!.Value) newSelectedOption = optionsArray.ElementAtOrDefault(1);
                    selectedOption = newSelectedOption;
                }
                else
                {
                    var remaining = optionsArray.SkipWhile(x => x.Value != selectedOption!.Value).ToArray();
                    var newSelectedOption = remaining.ElementAtOrDefault(1);
                    selectedOption = newSelectedOption ?? optionsArray.First();
                }
                originalValue = false;
            }
            if (info.Key == ConsoleKey.UpArrow)
            {
                if (originalValue)
                {
                    var newSelectedOption = optionsArray.Last();
                    if (newSelectedOption.Value == selectedOption!.Value) newSelectedOption = optionsArray.Reverse().ElementAtOrDefault(1);
                    selectedOption = newSelectedOption ?? optionsArray.First();
                }
                else
                {
                    var remaining = optionsArray.TakeWhile(x => x.Value != selectedOption!.Value).Reverse().ToArray();
                    var newSelectedOption = remaining.FirstOrDefault();
                    selectedOption = newSelectedOption ?? optionsArray.Last();
                }
                originalValue = false;
            }
        }
    }
    #endregion

    #region Private Helpers
    private static void PrintOption(SelectListItem selectedOption, int maxLenPlus2, int cursorLeft, int cursorTop)
    {
        //Erase the old value
        Console.CursorLeft = cursorLeft;
        Console.CursorTop = cursorTop;
        Console.Write("".PadRight(maxLenPlus2));
                
        //Print the new value
        Console.CursorLeft = cursorLeft;
        Console.CursorTop = cursorTop;
        CmdScaffoldingSettings.Value?.SetColors();
        Console.Write(selectedOption.Label);
        CmdScaffoldingSettings.DropdownArrow?.SetColors();
        Console.Write('▼');
    }
    private static void PrintRequiredFieldMessage()
    {
        var currentColors = FBColors.FromCurrent();
        CmdScaffoldingSettings.InvalidValueMessage?.SetColors();

        if (CmdContext.PropertyDisplayName != null) Console.Write($"The {CmdContext.PropertyDisplayName} is required. ");
        else Console.Write("This field is required. ");

        CmdScaffoldingSettings.Prompt?.SetColors();
        Console.Write("Try again: ");

        currentColors.SetColors();
    }
    private static void PrintErrorMessage()
    {
        var currentColors = FBColors.FromCurrent();
            
        CmdScaffoldingSettings.InvalidValueMessage?.SetColors();
        Console.Write("Invalid entry. ");
        CmdScaffoldingSettings.Prompt?.SetColors();
        Console.Write("Try again: ");

        currentColors.SetColors();
    }
    private static bool IsValidFloatingPoint(char x)
    {
        return char.IsDigit(x) || x == '-' || x == '+' || x == '.' || x == 'e' || x == 'E';
    }
    private static bool IsValidUnsignedIntegerChar(char x)
    {
        return char.IsDigit(x);
    }
    private static bool IsValidIntegerChar(char x)
    {
        return char.IsDigit(x) || x == '-';
    }
    private static bool IsValidDateChar(char x)
    {
        return char.IsDigit(x) || x == '/';
    }
    #endregion
}