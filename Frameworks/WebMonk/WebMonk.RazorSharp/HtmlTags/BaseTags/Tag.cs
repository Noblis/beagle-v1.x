using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Supermodel.DataAnnotations;
using Supermodel.DataAnnotations.Misc;
using WebMonk.RazorSharp.Exceptions;
using WebMonk.RazorSharp.Extensions;

namespace WebMonk.RazorSharp.HtmlTags.BaseTags;

[DebuggerDisplay("{TagType}")]
public class Tag : List<IGenerateHtml>, IGenerateAndContainHtml
{
    #region Constructors
    public Tag(string? tagType, object? attributes = null)
    {
        TagType = tagType;
        Attributes = AttributesDict.FromAnonymousObject(attributes ?? new AttributesDict());
    }
    #endregion

    #region Overrides
    public virtual StringBuilderWithIndents ToHtml(StringBuilderWithIndents? sb = null)
    {
        sb ??= new StringBuilderWithIndents();

        if (ContainsInnerHtml())
        {
            sb.AppendLineIndentPlus($"<{TagType}{GenerateMyAttributesString()}>");
            foreach (var tag in this) sb = tag.ToHtml(sb);
            sb.AppendLineIndentMinus($"</{TagType}>");
        }
        else
        {
            sb.AppendLine($"<{TagType}{GenerateMyAttributesString()}></{TagType}>");
        }

        return sb;
    }
    public override string ToString()
    {
        return ToHtml().ToString().Trim();
    }
    public virtual void Add(string txt)
    {
        Add(new Txt(txt));
    }
    #endregion

    #region Linq-like methods
    public virtual int CountWhere(Func<Tag, bool> predicate)
    {
        return GetTagsInOrder().Count(predicate);
    }
    public virtual IEnumerable<Tag> Where(Func<Tag, bool> predicate)
    {
        return GetTagsInOrder().Where(predicate);
    }
    public virtual IEnumerable<Tag?> ParentsOfTagsWhere(Func<Tag, bool> predicate)
    {
        NormalizeAndFlatten();
        return GetTagsInOrder().Where(predicate).Select(x => x._parent);
    }

    public virtual Tag SingleWhere(Func<Tag, bool> predicate)
    {
        return GetTagsInOrder().Single(predicate);
    }
    public virtual Tag? SingleOrDefaultWhere(Func<Tag, bool> predicate)
    {
        return GetTagsInOrder().SingleOrDefault(predicate);
    }

    public virtual Tag FirstWhere(Func<Tag, bool> predicate)
    {
        return GetTagsInOrder().First(predicate);
    }
    public virtual Tag? FirstOrDefaultWhere(Func<Tag, bool> predicate)
    {
        return GetTagsInOrder().FirstOrDefault(predicate);
    }

    public virtual bool AnyWhere(Func<Tag, bool> predicate)
    {
        return GetTagsInOrder().Any(predicate);
    }
    public virtual bool AllWhere(Func<Tag, bool> predicate)
    {
        return GetTagsInOrder().All(predicate);
    }

    public virtual int RemoveWhere(Func<Tag, bool> predicate)
    {
        var removalCount = 0;
        var remainingChildren = new List<IGenerateHtml>();

        foreach (var iGenerateHtml in this)
        {
            if (iGenerateHtml is Tag tag)
            {
                if (predicate(tag)) removalCount++;
                else remainingChildren.Add(iGenerateHtml);
            }
            else
            {
                remainingChildren.Add(iGenerateHtml);
            }
        }

        foreach (var remainingChild in remainingChildren)
        {
            removalCount += remainingChild.RemoveWhere(predicate);
        }

        Clear();
        AddRange(remainingChildren);

        return removalCount;
    }
    public virtual int RemoveFirstWhere(Func<Tag, bool> predicate)
    {
        var removalCount = 0;
        var remainingChildren = new List<IGenerateHtml>();

        foreach (var iGenerateHtml in this)
        {
            if (iGenerateHtml is Tag tag)
            {
                if (predicate(tag) && removalCount == 0) removalCount++;
                else remainingChildren.Add(iGenerateHtml);
            }
            else
            {
                remainingChildren.Add(iGenerateHtml);
            }
        }

        if (removalCount == 0)
        {
            foreach (var remainingChild in remainingChildren)
            {
                removalCount += remainingChild.RemoveFirstWhere(predicate);
                if (removalCount > 0) break;
            }
        }

        Clear();
        AddRange(remainingChildren);

        return removalCount;
    }

    public virtual int InsertBeforeWhere(Func<Tag, bool> predicate, IGenerateHtml tags)
    {
        var insertCount = 0;

        for (var i = 0; i < Count; i++)
        {
            var iGenerateHtml = this[i];
            if (iGenerateHtml is Tag tag && predicate(tag))
            {
                Insert(i, tags);
                i++;
                insertCount++;
            }
        }

        foreach (var child in this)
        {
            insertCount += child.InsertBeforeWhere(predicate, tags);
        }

        return insertCount;
    }
    public virtual int InsertAfterWhere(Func<Tag, bool> predicate, IGenerateHtml tags)
    {
        var insertCount = 0;

        for (var i = 0; i < Count; i++)
        {
            var iGenerateHtml = this[i];
            if (iGenerateHtml is Tag tag && predicate(tag))
            {
                Insert(i + 1, tags);
                i++;
                insertCount++;
            }
        }

        foreach (var child in this)
        {
            insertCount += child.InsertAfterWhere(predicate, tags);
        }

        return insertCount;
    }

    public virtual int InsertBeforeFirstWhere(Func<Tag, bool> predicate, IGenerateHtml tags)
    {
        var insertCount = 0;

        for (var i = 0; i < Count; i++)
        {
            var iGenerateHtml = this[i];
            if (iGenerateHtml is Tag tag && predicate(tag))
            {
                Insert(i, tags);
                insertCount++;
                return insertCount; //1
            }
        }

        foreach (var child in this)
        {
            insertCount += child.InsertBeforeFirstWhere(predicate, tags);
            if (insertCount > 0) return insertCount; //1
        }

        return insertCount; //0
    }
    public virtual int InsertAfterFirstWhere(Func<Tag, bool> predicate, IGenerateHtml tags)
    {
        var insertCount = 0;

        for (var i = 0; i < Count; i++)
        {
            var iGenerateHtml = this[i];
            if (iGenerateHtml is Tag tag && predicate(tag))
            {
                Insert(i + 1, tags);
                insertCount++;
                return insertCount; //1
            }
        }

        foreach (var child in this)
        {
            insertCount += child.InsertAfterFirstWhere(predicate, tags);
            if (insertCount > 0) return insertCount; //1
        }

        return insertCount; //0
    }
    #endregion

    #region Methods
    public Tag AddOrUpdateAttr(object? additionalAttributes)
    {
        if (additionalAttributes == null) return this;
        var additionalAttributesDict = AttributesDict.FromAnonymousObject(additionalAttributes);
        foreach (var key in additionalAttributesDict.Keys)
        {
            Attributes[key] = additionalAttributesDict[key];
        }
        return this;
    }

    //if updating the next two methods, update the same methods in HtmlAttrAttribute class
    public string GenerateMyAttributesString()
    {
        if (Attributes.All(x => x.Value == null)) return "";
        var sb = new StringBuilder(" ");
        foreach (var pair in Attributes.Where(x => x.Value != null))
        {
            sb.Append($"{pair.Key.Replace("_", "-").HtmlEncode().Trim()}=\"{pair.Value?.HtmlAttributeEncode().Trim()}\" ");
        }
        return $" {sb.ToString().Trim()}";
    }

    public IGenerateHtml FillBodySectionWith(IGenerateHtml replacement)
    {
        FillSectionWith(BodySectionPlaceholder.BodySection, replacement);
        return this;
    }
    public IGenerateHtml FillSectionWith(string sectionId, IGenerateHtml replacement)
    {
        if (!TryFillSectionWith(sectionId, replacement)) throw new RazorSharpException($"Unable to find placeholder with uniqueIdentifier = '{sectionId}'");
        return this;
    }
    public bool TryFillSectionWith(string sectionId, IGenerateHtml replacement)
    {
        for(var i = 0; i < Count; i++)
        {
            if (this[i] is SectionPlaceholder placeholder)
            {
                if (placeholder.UniqueIdentifier == sectionId)
                {
                    this[i] = replacement;
                    return true;
                }
            }
            if (this[i].TryFillSectionWith(sectionId, replacement)) return true;
        }
        return false;
    }

    public IGenerateHtml DisableAllControls()
    {
        if (TagType == "fieldset" || TagType == "input" || TagType == "textarea" || TagType == "select") AddOrUpdateAttr(new { disabled = "disabled" });
        foreach (var html in this) html.DisableAllControls();
        return this;
    }
    public IGenerateHtml DisableAllControlsIf(bool condition)
    {
        if (condition) DisableAllControls();
        return this;
    }
    public bool ContainsInnerHtml()
    {
        return Count > 0;
    }
        
    public virtual IEnumerable<Tag> GetTagsInOrder()
    {
        var tagsList = new List<Tag> { this };
        tagsList.AddRange(this.SelectMany(x => x.GetTagsInOrder()));
        return tagsList;
    }
    public virtual List<Tag> NormalizeAndFlatten()
    {
        var childrenList = new List<Tag>();
        foreach (var iGenerateHtml in this)
        {
            if (iGenerateHtml is Tag tag)
            {
                tag.NormalizeAndFlatten();
                childrenList.Add(tag);
                tag._parent = this;
            }
            else
            {
                var childrenTags = iGenerateHtml.NormalizeAndFlatten();
                foreach (var childrenTag in childrenTags) childrenTag._parent = this;
                childrenList.AddRange(childrenTags);
            }
        }

        Clear();
        AddRange(childrenList);

        return [this];
    }
    #endregion

    #region Properties
    public string? Id
    {
        get => Attributes.ContainsKey("id") ? Attributes["id"] : null;
        set => Attributes["id"] = value;
    }

    public string? TagType { get; set; }
    public AttributesDict Attributes { get; }

    protected internal Tag? _parent;
    #endregion
}