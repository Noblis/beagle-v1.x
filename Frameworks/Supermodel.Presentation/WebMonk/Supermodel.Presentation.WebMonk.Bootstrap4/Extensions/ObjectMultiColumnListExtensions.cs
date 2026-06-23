using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Supermodel.DataAnnotations.Attributes;
using Supermodel.DataAnnotations.Exceptions;
using Supermodel.Presentation.WebMonk.Bootstrap4.Models;
using Supermodel.Presentation.WebMonk.Extensions;
using Supermodel.Presentation.WebMonk.Models;
using Supermodel.ReflectionMapper;
using WebMonk.Context;
using WebMonk.Extensions;
using WebMonk.RazorSharp.HtmlTags;
using WebMonk.RazorSharp.HtmlTags.BaseTags;
using WebMonk.Rendering.Templates;
using WebMonk.Rendering.Views;

namespace Supermodel.Presentation.WebMonk.Bootstrap4.Extensions;

public static class ObjectMultiColumnListExtensions
{
    #region Read Only
    public static IGenerateHtml ToReadOnlyHtmlTableHeader(this object me)
    {
        var result = new HtmlStack();

        var propertyInfos = me.GetType().GetListPropertyInfosInOrder().ToList();
        if (!propertyInfos.Any()) throw new SupermodelException("ListMvcModelT must have at least one property marked with [ListColumn] attribute");
        foreach (var propertyInfo in propertyInfos)
        {
            var listColumnAttr = propertyInfo.GetCustomAttribute<ListColumnAttribute>();
            var header = listColumnAttr!.Header ?? propertyInfo.Name.InsertSpacesBetweenWords(); //all properties we get from GetPropertiesInOrder will contain ListColumnAttribute
            var orderBy = listColumnAttr.OrderBy;
            var orderByDesc = listColumnAttr.OrderByDesc;
            var title = propertyInfo.GetAttribute<TooltipAttribute>()?.Tooltip;
            result.Append(new Bs4.SortableColumnHeader(header, orderBy, orderByDesc, title));
        }
        return result;
    }
    public static IGenerateHtml ToReadOnlyHtmlTableRow(this object me)
    {
        var result = new HtmlStack();

        var propertyInfos = me.GetType().GetListPropertyInfosInOrder().ToList();
        if (!propertyInfos.Any()) throw new SupermodelException("ListMvcModelT must have at least one property marked with [ListColumn] attribute");
        foreach (var propertyInfo in propertyInfos)
        {
            result.AppendAndPush(new Td());
            var propertyObj = me.PropertyGet(propertyInfo.Name);
            if (propertyObj != null)
            {
                //We do special handling for IHtmlContent type to allow for HTML context in columns while still being secure
                if (propertyObj is IGenerateHtml htmlContent)
                {
                    result.Append(htmlContent);
                }
                else
                {
                    if (propertyObj is IDisplayTemplate) result.Append(Render.Display(me, propertyInfo.Name));
                    else result.Append(new Txt(propertyObj.GetDescription()));
                }
            }
            result.Pop<Td>();
        }
        return result;
    }
    #endregion

    #region Editable
    public static IGenerateHtml ToEditableHtmlTableHeader(this IViewModelForEntity me)
    {
        var result = new HtmlStack();

        var propertyInfos = me.GetType().GetDetailPropertyInfosInOrder().ToList();
        if (!propertyInfos.Any()) throw new SupermodelException("DetailMvcModelT must have at least one property if used with Editable List");
        foreach (var propertyInfo in propertyInfos)
        {
            string header;
            string? orderBy, orderByDesc;
            var listColumnAttr = propertyInfo.GetCustomAttribute<ListColumnAttribute>();
            if (listColumnAttr != null)
            {
                header = listColumnAttr.Header ?? me.GetType().GetDisplayNameForProperty(propertyInfo.Name);
                orderBy = listColumnAttr.OrderBy;
                orderByDesc = listColumnAttr.OrderByDesc;
            }
            else
            {
                header = me.GetType().GetDisplayNameForProperty(propertyInfo.Name);
                orderBy = null;
                orderByDesc = null;
            }

            var title = propertyInfo.GetAttribute<TooltipAttribute>()?.Tooltip;
            var requiredLabel = !propertyInfo.HasAttribute<NoRequiredLabelAttribute>() && (propertyInfo.HasAttribute<RequiredAttribute>() || propertyInfo.HasAttribute<ForceRequiredLabelAttribute>());
            result.Append(new Bs4.SortableColumnHeader(header, orderBy, orderByDesc, title, requiredLabel));
        }
        return result;
    }
    public static IGenerateHtml ToEditableHtmlTableRow(this IViewModelForEntity me, long? parentId, bool editable, bool selected)
    {
        var result = new HtmlStack();

        var propertyInfos = me.GetType().GetDetailPropertyInfosInOrder().ToList();
        if (!propertyInfos.Any()) throw new SupermodelException("DetailMvcModelT must have at least one property if used with Editable List");

        var isFirst = true;
        var showValidationSummary = !HttpContext.Current.ValidationResultList.IsValid && selected;
        var validationSummaryPlaceholder = new HtmlStack();
        foreach (var propertyInfo in propertyInfos)
        {
            var idAttr = selected ? new { id="SelectedRow" } : null;
            result.AppendAndPush(new Td(idAttr));

            if (editable && isFirst)
            {
                result.Append(new Input(new { id="Id", name="Id", type="hidden", value = me.Id })).DisableAllControlsIf(!selected);
                if (parentId != null) result.Append(new Input(new { id=$"{Config.InlinePrefix}.parentId".ToHtmlId(), name = $"{Config.InlinePrefix}.parentId".ToHtmlName(), type="hidden", value = parentId })).DisableAllControlsIf(!selected);
                result.Append(new Input(new { id="IsInline", name="IsInline", type="hidden", value="true" })).DisableAllControlsIf(!selected);
                if (!me.IsNewModel()) result.Append(Render.HttpMethodOverride(HttpMethod.Put)).DisableAllControlsIf(!selected);
            }
            var propertyObj = me.PropertyGet(propertyInfo.Name);
            if (propertyObj != null)
            {
                //We do special handling for IHtmlContent type to allow for HTML context in columns while still being secure
                if (propertyObj is IGenerateHtml htmlContent)
                {
                    result.Append(htmlContent);
                }
                else
                {
                    if (editable)
                    {
                        if (!propertyInfo.HasAttribute<DisplayOnlyAttribute>())
                        {
                            var tags = result.Append(Render.Editor(me, propertyInfo.Name)).DisableAllControlsIf(!selected);
                            foreach (var tag in tags.GetTagsInOrder())
                            {
                                if (tag is Input input &&
                                    input.Attributes.ContainsKey("type") &&
                                    (input.Attributes["type"] == "checkbox" || input.Attributes["type"] == "radio") &&
                                    input.Attributes.ContainsKey("checked") &&
                                    input.Attributes["checked"] == "checked")
                                {
                                    input.Attributes["checked"] = null;
                                    input.Attributes["data-checked"] = "checked";
                                }
                            }
                            if (selected && !HttpContext.Current.ValidationResultList.IsValid) 
                            {
                                var msg = Render.ValidationMessage(me, propertyInfo.Name, new { @class=Bs4.ScaffoldingSettings.InlineValidationErrorCssClass }, true);
                                if (!(msg is Tags msgTags && msgTags.Count == 0)) showValidationSummary = false;
                                result.Append(msg);
                            }
                        }
                        else
                        {
                            result.Append(Render.Display(me, propertyInfo.Name));
                        }
                    }
                    else
                    {
                        if (propertyObj is IDisplayTemplate) result.Append(Render.Display(me, propertyInfo.Name));
                        else result.Append(new Txt(propertyObj.GetDescription()));
                    }
                }
            }
            if (editable && isFirst)
            {
                result.Append(validationSummaryPlaceholder);
                isFirst = false;
            }
            result.Pop<Td>();
        }

        if (showValidationSummary)
        {
            validationSummaryPlaceholder.AppendAndPush(new Div(new { @class=$"col-sm-12 {Bs4.ScaffoldingSettings.ValidationSummaryCssClass}" }));
            validationSummaryPlaceholder.Append(Render.ValidationSummary());
            validationSummaryPlaceholder.Pop<Div>();
        }

        return result;
    }
    #endregion
}