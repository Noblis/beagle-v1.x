using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Supermodel.Presentation.Mvc.Bootstrap4.Models.Base;
using Supermodel.Presentation.Mvc.Bootstrap4.SuperHtmlHelpers;
using Supermodel.Presentation.Mvc.Bootstrap4.TagHelpers.Base;
using Supermodel.Presentation.Mvc.HtmlHelpers;

namespace Supermodel.Presentation.Mvc.Bootstrap4.TagHelpers;

[HtmlTargetElement("super-bs4-crud-children-list", Attributes = "items, parent-id, child-controller-type", TagStructure = TagStructure.WithoutEndTag)]
public class SuperBs4CRUDChildrenListTagHelper : TagHelperDerivedFromHtmlHelperBase
{
    #region Constructors
    public SuperBs4CRUDChildrenListTagHelper(IHtmlHelper<dynamic> htmlHelper) : base(htmlHelper){ }
    #endregion

    #region Overrides
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = null;
        output.Content.SetHtmlContent(_htmlHelper.Super().Bs4().CRUDChildrenList(Items, ChildControllerType, ParentId, Title, SkipAddNew, SkipDelete, ViewOnly));
    }
    #endregion

    #region Properties
    public IEnumerable<MvcModelForEntityCore> Items { get; set; } = new List<MvcModelForEntityCore>();
    public long ParentId { get; set; } = 0;
    public string Title { get; set; } = "";

    public bool SkipAddNew { get; set; } = false;
    public bool SkipDelete { get; set; } = false;
    public bool ViewOnly { get; set; } = false;

    public Type ChildControllerType { get; set; } = null!;
    #endregion
}