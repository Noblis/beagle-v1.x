using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Supermodel.DataAnnotations.Attributes;
using Supermodel.DataAnnotations.Exceptions;
using Supermodel.DataAnnotations.Expressions;
using Supermodel.DataAnnotations.Misc;
using Supermodel.Presentation.WebMonk.Extensions;
using Supermodel.Presentation.WebMonk.Models;
using Supermodel.ReflectionMapper;
using WebMonk.Context;
using WebMonk.Exceptions;
using WebMonk.Extensions;
using WebMonk.Misc;
using WebMonk.ModeBinding;
using WebMonk.RazorSharp.HtmlTags;
using WebMonk.RazorSharp.HtmlTags.BaseTags;
using WebMonk.Rendering.Templates;
using WebMonk.Rendering.Views;
using WebMonk.ValueProviders;

namespace Supermodel.Presentation.WebMonk.Bootstrap4.Models;

public static partial class Bs4
{
    public class BinaryFileMvcModel : BinaryFileModelBase, ISelfModelBinder, IEditorTemplate, IDisplayTemplate, IHiddenTemplate
    {
        #region ISelfModelBinder implementation
        public virtual Task<object?> BindMeAsync(Type rootType, List<IValueProvider> valueProviders)
        {
            var prefix = HttpContext.Current.PrefixManager.CurrentPrefix;

            if (string.IsNullOrEmpty(prefix)) throw new WebMonkException("prefix is not set");
            var name = prefix.ToHtmlName();

            var binaryContent = valueProviders.GetValueOrDefault<byte[]>(name).GetCastValue<byte[]>();
            var fileName = valueProviders.GetValueOrDefault<string>($"{name}{IValueProvider.FileNameSuffix}").GetCastValue<string>();

            //if we are submitting nothing, do not change the original data of the component            
            if (!string.IsNullOrEmpty(fileName) && binaryContent != null && binaryContent.Length > 0)
            {
                BinaryContent = binaryContent;
                FileName = fileName;
            }

            //Because this is not a IUIComponentWithValue, we have to validate Required attribute here
            if (string.IsNullOrEmpty(FileName) || BinaryContent == null || BinaryContent.Length == 0)
            {
                if (prefix.StartsWith($"{Config.InlinePrefix}.", StringComparison.OrdinalIgnoreCase)) prefix = prefix.Substring(Config.InlinePrefix.Length + 1);
                    
                var propertyInfo = rootType.GetPropertyByFullName(prefix);
                if (propertyInfo.GetAttribute<RequiredAttribute>() != null)
                {
                    var label = rootType.GetDisplayNameForProperty(prefix);
                    HttpContext.Current.ValidationResultList.Add(new ValidationResult($"The {label} field is required",
                        [name]));
                }
            }

            return Task.FromResult((object?)this);
        }
        #endregion

        #region IEditorTemplate implemtation
        public virtual IGenerateHtml EditorTemplate(int screenOrderFrom = int.MinValue, int screenOrderTo = int.MaxValue, object? attributes = null)
        {
            var result = new HtmlStack();

            if (!string.IsNullOrEmpty(ScaffoldingSettings.CRUDBinaryFileChooseCssClass)) HtmlAttributesAsDict.AddOrAppendCssClass(ScaffoldingSettings.CRUDBinaryFileChooseCssClass);

            result.Append(Render.FilePickerForModel(BinaryContent, HtmlAttributesAsDict));

            if (!string.IsNullOrEmpty(FileName) && HttpContext.Current.ValidationResultList.IsValid)
            {
                var (id, parentId, controller, pn) = GetIdParentIdControllerPropertyName();

                result.Append(new Br());
                result.AppendAndPush(new Div(new { @class="btn-group" }));

                var downloadLabel = new Tags
                {
                    new Span(new { @class="oi oi-cloud-download" }),
                    new Txt($"&nbsp;{FileName}"),
                };
                    
                var qs = HttpContext.Current.HttpListenerContext.Request.QueryString;
                qs.Add("parentId", parentId.ToString());
                qs.Add("pn", pn);
                result.Append(Render.ActionLink(downloadLabel, controller, "BinaryFile", id, qs, new { @class=ScaffoldingSettings.CRUDBinaryFileDownloadCssClass }));

                if (!HttpContext.Current.PrefixManager.CurrentParent!.GetType().GetProperty(pn)!.HasAttribute<RequiredAttribute>())
                {
                    var deleteLabel = new Tags
                    {
                        new Span(new { @class="oi oi-trash" }),
                        new Txt("&nbsp;Delete File"),
                    };
                    result.Append(Render.RESTfulActionLink(deleteLabel, HttpMethod.Delete, controller, "BinaryFile", id, qs, new { @class=ScaffoldingSettings.CRUDBinaryFileDeleteCssClass }, "This will permanently delete the file. Are you sure?", false));
                }

                result.Pop<Div>();
            }

            return result;
        }
        #endregion

        #region IDisplayTemplate implementation
        public virtual IGenerateHtml DisplayTemplate(int screenOrderFrom = int.MinValue, int screenOrderTo = int.MaxValue, object? attributes = null)
        {
            var result = new Tags();
            if (!string.IsNullOrEmpty(FileName) && HttpContext.Current.ValidationResultList.IsValid)
            {
                var (id, parentId, controller, pn) = GetIdParentIdControllerPropertyName();
                result.Add(Render.ActionLink(FileName!, controller, "BinaryFile", id, new QueryStringDict { { "parentId", parentId.ToString() }, { "pn", pn } }));
            }
            return result;
        }
        #endregion

        #region IHiddenTemplate implementation
        public virtual IGenerateHtml HiddenTemplate(int screenOrderFrom = int.MinValue, int screenOrderTo = int.MaxValue, object? attributes = null)
        {
            throw new SupermodelException("BinaryFileMvcModel cannot be used as hidden.");
        }
        #endregion

        #region Protected Helper Methods
        protected (long?, long?, string, string) GetIdParentIdControllerPropertyName()
        {
            var valueProviders = HttpContext.Current.ValueProviderManager.GetCachedValueProvidersList();
            var id = ((IViewModelForEntity)HttpContext.Current.PrefixManager.CurrentParent!).Id; //valueProviders.GetValueOrDefault<long?>("id").GetNewValue<long?>();
            var parentId = valueProviders.GetValueOrDefault<long?>("parentId").GetCastValue<long?>();

            var controller = HttpContext.Current.PrefixManager.CurrentContextControllerName;  //HttpContext.Current.RouteManager.GetControllerFromRoute();

            var pn = HttpContext.Current.PrefixManager.CurrentPrefix.Replace($"{Config.InlinePrefix}.", "");

            return (id, parentId, controller, pn);
        }
        #endregion

        #region Properties
        public object HtmlAttributesAsObj { set => HtmlAttributesAsDict = AttributesDict.FromAnonymousObject(value); }
        public AttributesDict HtmlAttributesAsDict { get; set; } = new();
        #endregion
    }
}