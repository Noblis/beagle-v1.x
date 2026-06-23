using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using BrowserEmulator;
using WebMonk.RazorSharp.HtmlTags;
using WebMonk.RazorSharp.HtmlTags.BaseTags;
using Attribute = BrowserEmulator.Attribute;

namespace WebMonk.RazorSharp.Html2RazorSharp;

public class Translator<TAttribute, TResult> : TranslatorBase where TResult : class
{
    #region Constructors
    internal Translator(string html, GeneratorOfIGenerateHtml<TAttribute, TResult> generator, string wrapperKey) 
    {
        HtmlStr = html;
        BePage = new BrowserEmulatorParser(HtmlStr);
        Generator = generator;
        WrapperKey = wrapperKey;
        ToRazorSharp();
    }
    #endregion

    #region Methods
    public TResult ToRazorSharp()
    {
        try
        {
            return ToRazorSharpUtil();
        }
        catch (Exception e)
        {
            throw new UserFacingMessageException($"Line {BePage.GetCurrentLineNumber()}: {e.Message}");
        }
    }

    protected TResult ToRazorSharpUtil()
    {
        if (RazorSharp != null) return RazorSharp;

        var tagStack = new Stack<Tuple<string, bool, int>>();
        Generator.Initialize();

        ParserPoint = BePage.GetParserPoint();

        var beTag = BePage.ProcessTag();
        AttributeList? futureBeTag = null;

        void Cycle(bool last = false) //not a "true" method because this requires context of current scope
        {
            string text;
            ParserPoint = BePage.GetParserPoint();
            if (!last) text = BePage.GetTextToNextTag(out futureBeTag);
            else text = string.Empty;
            var tagType = MatchTag(beTag);
            AppendTag(tagStack, beTag, tagType, text, futureBeTag);
            beTag = futureBeTag;
        }
            
        try
        {
            while(true) Cycle();
        }
        catch (EOFException)
        {
            Cycle(true);

            if (tagStack.Count != 0)
            {
                var exSb = new StringBuilder();
                foreach (var tuple in tagStack)
                {
                    exSb.Append($"\r\n</{tuple.Item1}>, opened on line {tuple.Item3}.");
                }
                throw new ArgumentException($"Html does not close all tags that are opened. Missing tags are: {exSb}");
            }

            Generator.Finish();
        }

        RazorSharp = Generator.GetResult();
        if (RazorSharp == null) throw new NullReferenceException($"{nameof(RazorSharp)} == null: this should never happen");
        return RazorSharp;
    }

    protected void AppendTag(Stack<Tuple<string, bool, int>> tagStack, AttributeList beTag, Type tagType, string text, AttributeList? futureBeTag)
    {
        //skip doctype, comments
        if (beTag.Name.StartsWith("!--") || beTag.Name.ToUpper().StartsWith("!DOCTYPE")) return;

        //put together info about attributes on tag
        var attributes = GenerateAttribute(beTag, out int attributeCount);
        var attributesAreEmpty = attributeCount == 0 || (beTag.Count == 1 && beTag[^1].Name == "/");

        //handle the case of recognized self-closing tags
        if (typeof(SelfClosingTag).IsAssignableFrom(tagType))
        {
            //handling for not having a corresponding closing tag
            if (!(beTag.List.Count >= 1 && beTag.List[beTag.Count - 1] is Attribute attribute && attribute.Name == "/"))
            {
                var parserPoint = BePage.GetParserPoint();

                if (string.IsNullOrEmpty(HttpUtility.HtmlDecode(text.Trim())) && futureBeTag != null && futureBeTag.Name.ToLower() == "/" + beTag.Name.ToLower())
                {
                    tagStack.Push(new Tuple<string, bool, int>(beTag.Name, false, BePage.GetCurrentLineNumberForParserPoint(ParserPoint)));
                }

                BePage.SetParserPoint(parserPoint);
            }
                
            Generator.AddRecognizedSelfClosingTag(tagType, attributes);
        }

        //handle the case of non-self-closing opening tags
        else if (!IsClosing(beTag.Name) && tagType.Name != nameof(Tag) && !typeof(SelfClosingTag).IsAssignableFrom(tagType))
        {
            //checks if this is an unrecognized closing tag
            var closesSelf = beTag.Name.EndsWith("/");

            //checks if tag has no txt or other content
            var emptyTag = futureBeTag != null && futureBeTag.Name.ToLower() == "/" + beTag.Name.ToLower() && text == string.Empty;
                
            tagStack.Push(new Tuple<string, bool, int>(beTag.Name, closesSelf || emptyTag, BePage.GetCurrentLineNumberForParserPoint(ParserPoint)));
                
            var pop = Generator.AddRecognizedNonSelfClosingTagAndPotentiallyPop(tagType, attributesAreEmpty, closesSelf, emptyTag, attributes);
            if (pop) tagStack.Pop();
        }

        else if(beTag.Name == WrapperKey || beTag.Name == "/" + WrapperKey)
        {
            //do nothing
        }

        //handle case of invalid opening tags
        else if (!IsClosing(beTag.Name) && tagType.Name == nameof(Tag))
        {
            if(!Generator.GenerateInvalidTags) throw new ArgumentException($"Invalid tag <{beTag.Name}>");

            //checks if this is an unrecognized closing tag
            var closesSelf = beTag.Name.EndsWith("/") || (beTag.Count > 0 && beTag[^1].Name == "/");

            //checks if tag has no txt or other content
            var emptyTag = futureBeTag != null && futureBeTag.Name.ToLower() == "/" + beTag.Name.ToLower() && text == string.Empty;

            tagStack.Push(new Tuple<string, bool, int>(beTag.Name, closesSelf || emptyTag, BePage.GetCurrentLineNumberForParserPoint(ParserPoint)));
                
            var pop = Generator.AddInvalidTagAndPotentiallyPop(beTag.Name, attributesAreEmpty, closesSelf, emptyTag, attributes);
            if (pop) tagStack.Pop();
        }

        //handle case of closing tags
        else if (IsClosing(beTag.Name))
        {
            var expectedOpeningTag = tagStack.Pop();
            if (!expectedOpeningTag.Item2)
            {
                if('/' + expectedOpeningTag.Item1.ToLower() != beTag.Name.ToLower()) throw new ArgumentException($"Expected Closing Tag Matching <{expectedOpeningTag.Item1}>, opened on line {expectedOpeningTag.Item3}");
                    
                //makes sure to only close non-self-closing tags
                if (!typeof(SelfClosingTag).IsAssignableFrom(MatchTag(beTag, true))) Generator.CloseTag();
            }
        }

        //add txt tags
        if (!string.IsNullOrEmpty(text)) Generator.AddTxtTag(text);
    }

    protected TAttribute GenerateAttribute(AttributeList beTag, out int attributeCount)
    {
        var attributes = new List<Attribute>();

        foreach (var attributeObj in beTag.List)
        {
            if (attributeObj is Attribute attribute)
            {
                if (attribute.Name != "/") attributes.Add(attribute);
            }
            else
            {
                throw new SystemException("Unable to cast Object to Attribute while generating RazorSharp Html attributes");
            }
        }

        attributeCount = attributes.Count;

        return Generator.GenerateAttributes(attributes);
    }
    #endregion

    #region Properties
    protected string HtmlStr { get; }
    protected TResult? RazorSharp { get; set; }
    protected string WrapperKey { get; }
    protected int ParserPoint { get; set; }
    protected BrowserEmulatorParser BePage { get; }
    protected GeneratorOfIGenerateHtml<TAttribute, TResult> Generator { get; }
    #endregion

}

public abstract class TranslatorBase
{
    #region Methods
    internal static string StripWhiteSpace(string str)
    {
        return Regex.Replace(str, @"\s+", " ");
    }

    internal static string Decode(string str)
    {
        return HttpUtility.HtmlDecode(str).Replace("\"", "\"\"");
    }

    internal static string DecodeNoReplace(string str)
    {
        return HttpUtility.HtmlDecode(str);
    }

    internal static bool IsClosing(string tag)
    {
        return tag[0] == '/';
    }

    protected Type MatchTag(AttributeList beTag, bool closing = false)
    {
        var correctNamespace = typeof(Div).Namespace;
        var tagCandidates = typeof(Div).Assembly.GetTypes().Where(x => x.Namespace == correctNamespace).Select(x => x);
        var name = Decode(beTag.Name).Replace('-', '_').ToLower();
        if (closing) name = name.Substring(1);
        else if (name.EndsWith("/")) name = name.Substring(0, name.Length - 1);
        foreach (var tagCandidate in tagCandidates)
        {
            if (name == tagCandidate.Name.ToLower())
            {
                return tagCandidate;
            }
        }

        return typeof(Tag);
    }

    public static Translator<string, string> CreateTextual(string html, bool sortAttributes = false, bool generateInvalidTags = false)
    { 
        var guid = "x-" + Guid.NewGuid();
        html = $"<{guid}>{html}</{guid}>";
        return Create(html, new TextualRazorSharpGenerator(sortAttributes, generateInvalidTags), guid);
    }

    public static Translator<Action<Tag>, HtmlStack> CreateMnemonic(string html, bool sortAttributes = false, bool generateInvalidTags = false)
    {
        var guid = "x-" + Guid.NewGuid();
        html = $"<{guid}>{html}</{guid}>";
        return Create(html, new MnemonicRazorSharpGenerator(sortAttributes, generateInvalidTags), guid);
    }

    public static Translator<TAttribute, TResult> Create<TAttribute, TResult>(string html, GeneratorOfIGenerateHtml<TAttribute, TResult> generator, string wrapperKey) where TResult : class
    {
        return new Translator<TAttribute, TResult>(html, generator, wrapperKey);
    }
    #endregion

    #region Properties
    internal static List<string> CSharpKeywords { get; } = @"abstract	as	base	bool	
                                                break	byte	case	catch	
                                                char	checked	class	const	
                                                continue	decimal	default	delegate	
                                                do	double	else	enum	
                                                event	explicit	extern	false	
                                                finally	fixed	float	for	
                                                foreach	goto	if	implicit	
                                                in	int	interface	internal	
                                                is	lock	long	namespace	
                                                new	null	object	operator	
                                                out	override	params	private	
                                                protected	public	readonly	ref	
                                                return	sbyte	sealed	short	
                                                sizeof	stackalloc	static	string	
                                                struct	switch	this	throw	
                                                true	try	typeof	uint	
                                                ulong	unchecked	unsafe	ushort	
                                                using	virtual	void	volatile	
                                                while".ToLower().Split(null).ToList();
    #endregion
}