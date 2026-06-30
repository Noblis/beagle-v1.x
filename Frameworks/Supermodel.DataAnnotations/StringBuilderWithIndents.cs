using System;
using System.Text;

namespace Supermodel.DataAnnotations;

public class StringBuilderWithIndents
{
    #region Constructors
    public StringBuilderWithIndents()
    {
        _output = new StringBuilder();
        Indent = 0;
    }

    public StringBuilderWithIndents(int indent)
    {
        _output = new StringBuilder();
        Indent = indent;
    }
    #endregion

    #region Methods
    public new string ToString()
    {
        return Text;
    }
        
    public void Append(string str)
    {
        _output.Append(StrIndent + str);
        LineStart = false;
    }
    private void AppendIndentPlus(string str)
    {
        _output.Append(StrIndent + str);
        Indent++;
        LineStart = false;
    }
    private void AppendIndentMinus(string str)
    {
        Indent--;
        _output.Append(StrIndent + str);
        LineStart = false;
    }
    public void AppendLine(string str)
    {
        Append(str + Environment.NewLine);
        LineStart = true;
    }
    public void AppendLineIndentPlus(string str)
    {
        AppendIndentPlus(str + Environment.NewLine);
        LineStart = true;
    }
    public void AppendLineIndentMinus(string str)
    {
        AppendIndentMinus(str + Environment.NewLine);
        LineStart = true;

    }
    public void AppendFormat(string format, params object[] arg0)
    {
        _output.Append(StrIndent + string.Format(format, arg0));
        LineStart = false;
    }
    public void AppendLineFormat(string format, params object[] arg0)
    {
        _output.Append(StrIndent + string.Format(format, arg0) + Environment.NewLine);
        LineStart = true;

    }
    public void AppendFormatIndentPlus(string format, params object[] arg0)
    {
        AppendIndentPlus(StrIndent + string.Format(format, arg0));
        LineStart = false;
    }
    public void AppendLineFormatIndentPlus(string format, params object[] arg0)
    {
        AppendIndentPlus(StrIndent + string.Format(format, arg0) + Environment.NewLine);
        LineStart = true;
    }
    public void AppendFormatIndentMinus(string format, params object[] arg0)
    {
        AppendIndentMinus(StrIndent + string.Format(format, arg0));
        LineStart = false;
    }
    public void AppendLineFormatIndentMinus(string format, params object[] arg0)
    {
        AppendIndentMinus(StrIndent + string.Format(format, arg0) + Environment.NewLine);
        LineStart = true;
    }
    public bool TrimEndWhitespaceIfNewLinePresent()
    {
        var newLinePresent = false;

        var index = _output.Length;
        while(index > 0 && char.IsWhiteSpace(_output[index - 1]))
        {
            if (_output[index - 1] == '\n') newLinePresent = true;
            index--;
        }
            
        if (newLinePresent)
        {
            _output.Length = index;
            LineStart = false;
        }

        return newLinePresent;
    }
    public void TrimEndWhitespace()
    {
        var index = _output.Length;
        while(index > 0 && char.IsWhiteSpace(_output[index - 1]))
        {
            index--;
        }
        _output.Length = index;
        LineStart = false;
    }
    #endregion

    #region Accessors and Mutators
    public string Text => _output.ToString();

    public int Indent
    {
        get => _indent;
        set
        {
            _indent = value;
            if (_indent < 0) _indent = 0; // throw new LendingArsenalEx("Indent may not be negative");
        }
    }
    public string StrIndent
    {
        get
        {
            if (LineStart != true) return "";
            StringBuilder ind = new StringBuilder();
            for (var i = 0; i < Indent; i++)
            {
                ind.Append("\t");
            }
            return ind.ToString();
        }
    }
    public bool LineStart { get; set; } = true;
    #endregion

    #region Internal fields
    private readonly StringBuilder _output;
    private int _indent;
    #endregion
}