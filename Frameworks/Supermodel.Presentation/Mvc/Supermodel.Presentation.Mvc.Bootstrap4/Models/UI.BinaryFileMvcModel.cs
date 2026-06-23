using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Supermodel.DataAnnotations.Attributes;
using Supermodel.DataAnnotations.Exceptions;
using Supermodel.DataAnnotations.Misc;
using Supermodel.Presentation.Mvc.Context;
using Supermodel.Presentation.Mvc.Extensions;
using Supermodel.Presentation.Mvc.HtmlHelpers;
using Supermodel.Presentation.Mvc.ModelBinding;
using Supermodel.Presentation.Mvc.Models;
using Supermodel.Presentation.Mvc.Models.Mvc.Rendering;
using Supermodel.ReflectionMapper;

namespace Supermodel.Presentation.Mvc.Bootstrap4.Models;

public static partial class Bs4
{
    public class BinaryFileMvcModel : BinaryFileModelBase, ISupermodelModelBinder, ISupermodelEditorTemplate, ISupermodelDisplayTemplate, ISupermodelHiddenTemplate
    {
        #region ISupermodelModelBinder implementation
        public virtual Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var success = true;

            if (bindingContext == null) throw new ArgumentNullException(nameof(bindingContext));

            var rawFile = bindingContext.HttpContext.Request.Form.Files[bindingContext.ModelName];

            var file = (BinaryFileMvcModel)ReflectionHelper.CreateType(GetType());
            if (rawFile == null || rawFile.Length == 0 || string.IsNullOrEmpty(rawFile.FileName))
            {
                var originalValue = (BinaryFileMvcModel)bindingContext.Model!;
                if (originalValue.IsEmpty)
                {
                    file.FileName = "";
                    file.BinaryContent = [];

                    if (bindingContext.IsPropertyRequired())
                    {
                        var displayName = bindingContext.ModelMetadata.ContainerType!.GetDisplayNameForProperty(bindingContext.ModelMetadata.PropertyName!);
                        bindingContext.ModelState.AddModelError(bindingContext.ModelName, $"The {displayName} field is required");
                        success = false;
                    }
                }
                else
                {
                    file = originalValue;
                }
            }
            else
            {
                file.FileName = Path.GetFileName(rawFile.FileName);
                file.BinaryContent = new byte[rawFile.Length];
                    
                using var stream = rawFile.OpenReadStream();
                var bytesRead = stream.Read(file.BinaryContent, 0, (int)rawFile.Length);
                if (bytesRead != rawFile.Length) throw new SupermodelException("This should never happen: bytesRead != rawFile.Length");
            }
                
            if (success) bindingContext.Result = ModelBindingResult.Success(file);  
            else bindingContext.Result = ModelBindingResult.Failed(); 

            return Task.CompletedTask;
        }
        #endregion

        #region ISupermodelEditorTemplate implemtation
        public virtual IHtmlContent EditorTemplate<TModel>(IHtmlHelper<TModel> html, int screenOrderFrom = int.MinValue, int screenOrderTo = int.MaxValue, string? markerAttribute = null)
        {
            if (!(html.ViewData.Model is BinaryFileMvcModel)) throw new InvalidCastException(ReflectionHelper.GetCurrentContext() + " is called for a model of type different from BinaryFileFormModel.");
            var model = (BinaryFileMvcModel)(object)html.ViewData.Model;

            var fileInputName = html.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix;
            var fileInputId = fileInputName.Replace(".", "_");

            var result = new StringBuilder();

            if (!string.IsNullOrEmpty(ScaffoldingSettings.CRUDBinaryFileChooseCssClass)) HtmlAttributesAsDict.AddOrAppendCssClass(ScaffoldingSettings.CRUDBinaryFileChooseCssClass);

            result.AppendLine(string.Format("<input type=\"file\" name=\"{0}\" id=\"{1}\" " + UtilsLib.GenerateAttributesString(HtmlAttributesAsDict) + "/>", fileInputName, fileInputId));

            if (!string.IsNullOrEmpty(model.FileName) && html.ViewData.ModelState.IsValid)
            {
                var propName = html.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix.Replace($"{Config.InlinePrefix}.", "");
                    
                var route = RequestHttpContext.Current.Request.Query.ToRouteValueDictionary();
                route.AddOrUpdateWith("id", html.ViewContext.RouteData.Values["id"]);
                route.AddOrUpdateWith("parentId", html.ViewContext.HttpContext.Request.Query["parentId"]);
                route.AddOrUpdateWith("pn", propName);
                    
                result.AppendLine("<br />");
                result.AppendLine("<div class='btn-group'>");

                // ReSharper disable once Mvc.ActionNotResolved
                result.AppendLine(html.Super().ActionLinkHtmlContent(("<span class=\"oi oi-cloud-download\"></span>&nbsp;" + HttpUtility.HtmlEncode(model.FileName)).ToHtmlString(), "BinaryFile", route, new { @class = ScaffoldingSettings.CRUDBinaryFileDownloadCssClass }).GetString());
                    
                var outerHtml = (IHtmlHelper)html.ViewContext.ViewData["OuterHtml"]!;
                if (!outerHtml.ViewData.Model!.GetType().GetProperty(propName)!.HasAttribute<RequiredAttribute>())
                {
                    var controllerName = RequestHttpContext.Current.Request.RouteValues["controller"]!.ToString();
                    var linkStr = html.Super().RESTfulActionLinkHtmlContent(HttpMethod.Delete, "<span class=\"oi oi-trash\"></span>&nbsp;Delete File".ToHtmlString(), "BinaryFile", controllerName!, route, HtmlHelper.AnonymousObjectToHtmlAttributes(new { @class = ScaffoldingSettings.CRUDBinaryFileDeleteCssClass }), "This will permanently delete the file. Are you sure?", false).GetString();                         
                    result.AppendLine(linkStr);
                }

                result.AppendLine("</div>");
            }
                
            return result.ToHtmlString();
        }
        #endregion

        #region ISupermodelDisplayTemplate implementation
        public virtual IHtmlContent DisplayTemplate<TModel>(IHtmlHelper<TModel> html, int screenOrderFrom = int.MinValue, int screenOrderTo = int.MaxValue, string? markerAttribute = null)
        {
            if (!(html.ViewData.Model is BinaryFileMvcModel)) throw new InvalidCastException(ReflectionHelper.GetCurrentContext() + " is called for a model of type different from BinaryFileFormModel.");
            var model = (BinaryFileMvcModel)(object)html.ViewData.Model;

            var result = new StringBuilder();
            if (!string.IsNullOrEmpty(model.FileName) && html.ViewData.ModelState.IsValid)
            {
                var route = new 
                { 
                    id = html.ViewContext.RouteData.Values["id"],
                    parentId = html.ViewContext.HttpContext.Request.Query["parentId"], 
                    pn = html.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix.Replace($"{Config.InlinePrefix}.", "")
                };
                    
                // ReSharper disable Mvc.ActionNotResolved
                result.AppendLine(html.ActionLink(model.FileName, "BinaryFile", route).GetString());
                // ReSharper restore Mvc.ActionNotResolved
            }
            return result.ToHtmlString();
        }
        #endregion

        #region ISupermodelHiddenTemplate implementation
        public virtual IHtmlContent HiddenTemplate<TModel>(IHtmlHelper<TModel> html, int screenOrderFrom = int.MinValue, int screenOrderTo = int.MaxValue, string? markerAttribute = null)
        {
            throw new SupermodelException("BinaryFileMvcModel cannot be used as hidden.");
        }
        #endregion

        #region Properties
        public object HtmlAttributesAsObj { set => HtmlAttributesAsDict = AttributesDict.FromAnonymousObject(value); }
        public AttributesDict HtmlAttributesAsDict { get; set; } = new();
        #endregion
    }
}