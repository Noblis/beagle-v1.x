using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Supermodel.DataAnnotations;
using Supermodel.DataAnnotations.Attributes;
using Supermodel.Presentation.Mvc.Extensions;
using Supermodel.Presentation.Mvc.ModelBinding;
using Supermodel.Presentation.Mvc.Models.Mvc.Rendering;

namespace Supermodel.Presentation.Mvc.Bootstrap4.Models.Base;

public abstract class MultiSelectMvcModel : IComparable, ISupermodelModelBinder, ISupermodelEditorTemplate, ISupermodelDisplayTemplate, ISupermodelHiddenTemplate
{
    #region Nested Option class
    public class Option
    {
        public Option(string value, string label, bool isDisabled, bool selected = false)
        {
            Value = value;
            Label = label;
            IsDisabled = isDisabled;
            Selected = selected;
        }
        public string Value { get; private set; }
        public string Label { get; private set; }
        public bool IsDisabled { get; private set; }
        public bool Selected { get; set; }
        public bool IsShown => Selected || !IsDisabled;
    }
    #endregion 
        
    #region Methods
    protected virtual string GetFullLabel(Option option)
    {
        return option.IsDisabled ? $"{option.Label}{DisabledSuffix}" : option.Label;
    }
    #endregion
        
    #region IComparable implemtation
    public int CompareTo(object? obj)
    {
        var other = (MultiSelectMvcModel?)obj;
        if (other == null) return 1;
        if (Options.Count != other.Options.Count) return 1;

        foreach (var option in Options)
        {
            if (other.Options.Find(x => x.Value == option.Value && x.Label == option.Label && x.Selected == option.Selected) == null) return 1;
        }
        return 0;
    }
    #endregion

    #region ISupermodelEditorTemplate implementation
    public abstract IHtmlContent EditorTemplate<TModel>(IHtmlHelper<TModel> html, int screenOrderFrom = int.MinValue, int screenOrderTo = int.MaxValue, string? markerAttribute = null);

    public virtual IHtmlContent CheckboxListEditorTemplate<TModel>(Orientation orientation, IHtmlHelper<TModel> html, int screenOrderFrom = int.MinValue, int screenOrderTo = int.MaxValue, string? markerAttribute = null)
    {
        var prefix = html.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix;
        var name = prefix;
        var id = name.Replace(".", "_");

        var sb = new StringBuilder();

        if (orientation == Orientation.Vertical)
        {
            var i = 1;
            foreach (var option in Options.Where(x => x.IsShown))
            {
                var label = GetFullLabel(option);

                sb.AppendLine("<div class='form-check'>");

                var input = $"<input class='form-check-input' type='checkbox' value='{option.Value}' id='{id}{i}' name='{name}' {markerAttribute} />";
                if (option.Selected) input = input.Replace(" />", " checked='on' />");
                sb.AppendLine(input);

                sb.AppendLine($"<label class='form-check-label' for='{id}{i}'>{label}</label>");

                sb.AppendLine("</div>");
                i++;
            }
        }
        else
        {
            var i = 1;
            foreach (var option in Options.Where(x => x.IsShown))
            {
                var label = GetFullLabel(option);

                sb.AppendLine("<div class='form-check form-check-inline'>");

                var input = $"<input class='form-check-input' type='checkbox' value='{option.Value}' id='{id}{i}' name='{name}' {markerAttribute} />";
                if (option.Selected) input = input.Replace(" />", " checked='on' />");
                sb.AppendLine(input);

                sb.AppendLine($"<label class='form-check-label' for='{id}{i}'>{label}</label>");

                sb.AppendLine("</div>");
                i++;
            }
        }

        sb.AppendLine(html.Hidden("", "", null).GetString());
        return sb.ToHtmlString();
    }
    #endregion

    #region ISupermodelDisplayTemplate implementation
    public virtual IHtmlContent DisplayTemplate<TModel>(IHtmlHelper<TModel> html, int screenOrderFrom = int.MinValue, int screenOrderTo = int.MaxValue, string? markerAttribute = null)
    {
        return EditorTemplate(html, screenOrderFrom, screenOrderTo, markerAttribute).DisableAllControls();
    }
    #endregion

    #region ISupermodelHiddenTemplate implementation
    public virtual IHtmlContent HiddenTemplate<TModel>(IHtmlHelper<TModel> html, int screenOrderFrom = int.MinValue, int screenOrderTo = int.MaxValue, string? markerAttribute = null)
    {
        var sb = new StringBuilder();
        foreach (var option in Options)
        {
            if (option.Selected) sb.Append(html.Hidden("", option.Value));
        }
        sb.Append(html.Hidden("", "")); //blank option
        return sb.ToHtmlEncodedIHtmlContent();   
    }
    #endregion

    #region ISupermodelModelBinder implementation
    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null) throw new ArgumentNullException(nameof(bindingContext));  

        var success = true;
            
        var key = bindingContext.ModelName;
        var val = bindingContext.ValueProvider.GetValue(key);

        string[] attemptedValues;
        if (val.Length <= 0)
        {
            var displayName = bindingContext.ModelMetadata.ContainerType!.GetDisplayNameForProperty(bindingContext.ModelMetadata.PropertyName!);
                
            if (bindingContext.IsPropertyRequired())
            {
                bindingContext.ModelState.AddModelError(key, $"The {displayName} field is required");
                success = false; 
            }
            attemptedValues = [];
        }
        else
        {
            attemptedValues = val.ToArray();
        }

        bindingContext.ModelState.SetModelValue(key, val);
        var existingModel = (MultiSelectMvcModel)bindingContext.Model!;
        if (existingModel is IAsyncInit iAsyncInit && !iAsyncInit.AsyncInitialized) await iAsyncInit.InitAsync();

        //Clear out selected
        existingModel.Options.ForEach(x => x.Selected = false);
        foreach (var selectedValue in attemptedValues)
        {
            // ReSharper disable AccessToModifiedClosure
            var selectedOption = existingModel.Options.Find(x => x.Value == selectedValue);
            // ReSharper restore AccessToModifiedClosure
                
            if (selectedOption != null) selectedOption.Selected = true;
                
            if (success) bindingContext.Result = ModelBindingResult.Success(existingModel);  
            else bindingContext.Result = ModelBindingResult.Failed(); 
        }
    }
    #endregion

    #region Properties
    public List<Option> Options { get; protected set; } = [];
    public string DisabledSuffix { get; set; } = " [DISABLED]";
    #endregion
}