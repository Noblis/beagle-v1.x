using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Supermodel.DataAnnotations;
using Attribute = BrowserEmulator.Attribute;

namespace WebMonk.RazorSharp.Html2RazorSharp;

public class TextualRazorSharpGenerator : GeneratorOfIGenerateHtml<string, string>
{
    #region Constructors
    public TextualRazorSharpGenerator(bool sortAttributes = false, bool generateInvalidTags = false) 
        : base(sortAttributes, generateInvalidTags) {}
    #endregion

    #region Overrides
    internal override void Initialize()
    {
        Sb.AppendLine("var html = new Tags");
        Sb.AppendLineIndentPlus("{");
    }

    internal override string GenerateAttributes(List<Attribute> attributes)
    {
        var sb = new StringBuilder();

        IEnumerable<Attribute> potentiallySortedAttributes;
        if (SortAttributes) potentiallySortedAttributes = attributes.OrderBy(x => x.Name);
        else potentiallySortedAttributes = attributes;

        foreach (var attribute in potentiallySortedAttributes)
        {
            if (attribute.Name == "/") continue;

            if (sb.Length == 0) sb.Append("new { ");

            if (TranslatorBase.CSharpKeywords.Contains(attribute.Name.ToLower())) sb.Append("@");

            sb.Append($"{TranslatorBase.Decode(attribute.Name.ToLower()).Replace('-', '_')}=\"");

            sb.Append(TranslatorBase.Decode(TranslatorBase.StripWhiteSpace(attribute.Value)));

            //if (attribute.Value != string.Empty) sb.Append(TranslatorBase.Decode(TranslatorBase.StripWhiteSpace(attribute.Value)));
            //else sb.Append(TranslatorBase.Decode(attribute.Name.ToLower()));

            sb.Append("\", ");
        }

        if (sb.Length != 0) sb.Append("}");

        return sb.ToString();
    }

    internal override void AddRecognizedSelfClosingTag(Type tag, string attributes)
    {
        Sb.AppendLine($"new {tag.Name}({attributes}),");
    }

    internal override bool AddRecognizedNonSelfClosingTagAndPotentiallyPop(Type tag, bool attributesAreEmpty, bool closesSelf, bool emptyTag, string attributes)
    {
        Sb.Append($"new {tag.Name}");

        //handles the case if tag is unrecognized self-closing or empty by using constructor
        if (!attributesAreEmpty || closesSelf || emptyTag) Sb.Append($"({attributes})");

        if (closesSelf || emptyTag) Sb.Append(",");

        Sb.AppendLine("");

        if (!closesSelf && !emptyTag) Sb.AppendLineIndentPlus("{");
        else if (closesSelf && !emptyTag) return true;

        return false;
    }

    internal override bool AddInvalidTagAndPotentiallyPop(string tagName, bool attributesAreEmpty, bool closesSelf, bool emptyTag, string attributes)
    {
        if (closesSelf && tagName.EndsWith("/")) Sb.Append($"new Tag(\"{tagName.Substring(0, tagName.Length - 1).ToLower()}\"");
        else Sb.Append($"new Tag(\"{tagName.ToLower()}\"");

        //handles the case if tag is unrecognized self-closing or empty by using constructor
        if (!attributesAreEmpty && !string.IsNullOrWhiteSpace(attributes)) Sb.AppendLine($", {attributes})");
        else Sb.AppendLine(")");

        //Old Code
        //handles the case if tag is unrecognized self-closing or empty by using constructor
        //if (!attributesAreEmpty || closesSelf || emptyTag) Sb.Append($", {attributes})");
        //if (closesSelf || emptyTag) Sb.Append(",");
        //Sb.AppendLine("");

        if (!closesSelf && !emptyTag) Sb.AppendLineIndentPlus("{");
        else if (closesSelf && !emptyTag) return true;

        return false;
    }

    internal override void CloseTag()
    {
        Sb.Indent--;
        Sb.AppendLine("},");
    }

    internal override void AddTxtTag(string text)
    {
        //Sb.Append("(Txt)");
        if (text.Contains("\n")) Sb.Append("@");
        Sb.AppendLine($"\"{TranslatorBase.Decode(text)}\",");
    }

    internal override void Finish()
    {
        Sb.Indent--;
        Sb.AppendLine("};");
    }

    internal override string GetResult()
    {
        return Sb.ToString();
    }
    #endregion

    #region Properties
    protected StringBuilderWithIndents Sb { get; } = new();
    #endregion
}