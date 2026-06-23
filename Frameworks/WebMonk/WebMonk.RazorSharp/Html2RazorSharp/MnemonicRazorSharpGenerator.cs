using System;
using System.Collections.Generic;
using System.Linq;
using WebMonk.RazorSharp.HtmlTags;
using WebMonk.RazorSharp.HtmlTags.BaseTags;
using Attribute = BrowserEmulator.Attribute;

namespace WebMonk.RazorSharp.Html2RazorSharp;

public class MnemonicRazorSharpGenerator : GeneratorOfIGenerateHtml<Action<Tag>, HtmlStack>
{
    #region Constructors
    public MnemonicRazorSharpGenerator(bool sortAttributes = false, bool generateInvalidTags = false)
        : base(sortAttributes, generateInvalidTags) { }
    #endregion

    #region Overrides
    internal override void Initialize()
    {
        Hs.AppendAndPush(new Tags());
    }

    internal override Action<Tag> GenerateAttributes(List<Attribute> attributes)
    {
        return tag =>
        {
            foreach (var attribute in attributes)
            {
                var name = TranslatorBase.Decode(attribute.Name.ToLower()).Replace('-', '_');

                var value = TranslatorBase.Decode(attribute.Value != string.Empty ? TranslatorBase.StripWhiteSpace(attribute.Value) : attribute.Name.ToLower());

                tag.Attributes.Add(name, value);
            }
        };
    }

    internal override void AddRecognizedSelfClosingTag(Type tag, Action<Tag> attributes)
    {
        //not a logic check, just a pre-casting check for sanity
        if (!typeof(SelfClosingTag).IsAssignableFrom(tag)) throw new ArgumentException("Type passed as a self-closing tag is not a self-closing tag"); 
            
        var tagToAdd = (SelfClosingTag)tag.GetConstructors()[0].Invoke([null]);
            
        Hs.Append(tagToAdd);

        attributes.Invoke(tagToAdd);
    }

    internal override bool AddRecognizedNonSelfClosingTagAndPotentiallyPop(Type tag, bool attributesAreEmpty, bool closesSelf, bool emptyTag, Action<Tag> attributes)
    {
        if (!typeof(Tag).IsAssignableFrom(tag)) throw new ArgumentException("Type passed as a tag is not a tag");

        var constructor = tag.GetConstructors().Single();
            
        var parameters = constructor.GetParameters();

        var objArr = new object?[parameters.Length];

        for (var i = 0; i < objArr.Length; i++)
        { 
            if(parameters[i].HasDefaultValue)
            {
                objArr[i] = parameters[i].DefaultValue;
            }
            else if (parameters[i].ParameterType.IsValueType) 
            {
                objArr[i] = Activator.CreateInstance(parameters[i].ParameterType);
            }
            else
            {
                objArr[i] = null;
            }
        }

        var tagToAdd = (Tag) tag.GetConstructors().Last().Invoke(objArr);

        var result = false;

        //NEW CODE
        if (closesSelf)
        {
            Hs.Append(tagToAdd);
            result = true;
        }
        else
        {
            Hs.AppendAndPush(tagToAdd);
        }
        //OLD CODE
        //if (!closesSelf && !emptyTag)
        //{
        //    Hs.AppendAndPush(tagToAdd);
        //}
        //else if (closesSelf && !emptyTag)
        //{
        //    Hs.Append(tagToAdd);
        //    result = true;
        //}

        attributes.Invoke(tagToAdd);

        return result;
    }

    internal override bool AddInvalidTagAndPotentiallyPop(string tagName, bool attributesAreEmpty, bool closesSelf, bool emptyTag, Action<Tag> attributes)
    {
        //NEW CODE
        Tag tagToAdd;
        if (closesSelf)
        {
            if (tagName.EndsWith("/")) tagToAdd = new SelfClosingTag(tagName.Substring(0, tagName.Length - 1).ToLower());
            else tagToAdd = new SelfClosingTag(tagName.ToLower());
        }
        else
        {
            tagToAdd = new Tag(tagName.ToLower());
        }
        //OLD CODE
        //var tagToAdd = closesSelf && tagName.EndsWith("/") ? new SelfClosingTag(tagName.Substring(0, tagName.Length - 1).ToLower()) : new Tag(tagName.ToLower());

        var result = false;
        
        //NEW CODE
        if (closesSelf)
        {
            Hs.Append(tagToAdd);
            result = true;
        }
        else
        {
            Hs.AppendAndPush(tagToAdd);
        }
        //OLD CODE
        //if (!closesSelf && !emptyTag)
        //{
        //    Hs.AppendAndPush(tagToAdd);
        //}
        //else if (closesSelf)
        //{
        //    Hs.Append(tagToAdd);
        //    result = true;
        //}

        attributes.Invoke(tagToAdd);
            
        return result;
    }

    internal override void CloseTag()
    {
        Hs.Pop();
    }

    internal override void AddTxtTag(string text)
    {
        Hs.Append(new Txt(TranslatorBase.DecodeNoReplace(text)));
    }

    internal override void Finish()
    {
        Hs.Pop();
    }

    internal override HtmlStack GetResult()
    {
        return Hs;
    }
    #endregion

    #region Properties
    protected HtmlStack Hs { get; }= new();
    #endregion
}