using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Supermodel.DataAnnotations.Attributes;
using Supermodel.DataAnnotations.Expressions;
using Supermodel.ReflectionMapper;
using WebMonk.Context;
using WebMonk.Exceptions;
using WebMonk.Extensions;
using WebMonk.ModeBinding;
using WebMonk.RazorSharp.HtmlTags;
using WebMonk.RazorSharp.HtmlTags.BaseTags;
using WebMonk.Rendering.Templates;
using WebMonk.Rendering.Views;
using WebMonk.ValueProviders;

namespace Supermodel.Presentation.WebMonk.Bootstrap4.Models.Base;

public abstract class MultiSelectMvcModel : IComparable, ISelfModelBinder, IEditorTemplate, IDisplayTemplate, IHiddenTemplate
{
    #region Nested Option class
    public class Option(string value, string label, bool isDisabled, bool selected = false)
    {
        public string Value { get; private set; } = value;
        public string Label { get; private set; } = label;
        public bool IsDisabled { get; private set; } = isDisabled;
        public bool Selected { get; set; } = selected;
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

    #region IEditorTemplate implementation
    public abstract IGenerateHtml EditorTemplate(int screenOrderFrom = int.MinValue, int screenOrderTo = int.MaxValue, object? attributes = null);
    public virtual IGenerateHtml CheckboxListEditorTemplate(Orientation orientation, int screenOrderFrom = int.MinValue, int screenOrderTo = int.MaxValue, object? attributes = null)
    {
        var prefix = HttpContext.Current.PrefixManager.CurrentPrefix;
        var id = prefix.ToHtmlId();
        var name = id.ToHtmlName();

        var html = new HtmlStack();

        if (orientation == Orientation.Vertical)
        {
            var i = 1;
            foreach (var option in Options.Where(x => x.IsShown))
            {
                var label = GetFullLabel(option);

                html.AppendAndPush(new Div(new { @class = "form-check" }));

                var input = html.Append(new Input(new { @class = "form-check-input", type = "checkbox", value = option.Value, id = $"{id}{i}", name }));
                input.AddOrUpdateAttr(attributes);
                if (option.Selected) input.Attributes.Add("checked", "on");

                html.Append(new Label(new { @class = "form-check-label", @for = $"{id}{i}" }) { new Txt(label) });

                html.Pop<Div>();
                i++;
            }
        }
        else
        {
            var i = 1;
            foreach (var option in Options.Where(x => x.IsShown))
            {
                var label = GetFullLabel(option);

                html.AppendAndPush(new Div(new { @class = "form-check form-check-inline" }));

                var input = html.Append(new Input(new { @class = "form-check-input", type = "checkbox", value = option.Value, id = $"{id}{i}", name }));
                input.AddOrUpdateAttr(attributes);
                if (option.Selected) input.Attributes.Add("checked", "on");

                html.Append(new Label(new { @class = "form-check-label", @for = $"{id}{i}" }) { new Txt(label) });

                html.Pop<Div>();
                i++;
            }
        }

        html.Append(Render.HiddenForModel(""));
        return html;
    }
    #endregion

    #region IDisplayTemplate implementation
    public virtual IGenerateHtml DisplayTemplate(int screenOrderFrom = int.MinValue, int screenOrderTo = int.MaxValue, object? attributes = null)
    {
        return EditorTemplate(screenOrderFrom, screenOrderTo, attributes).DisableAllControls();
    }
    #endregion

    #region IHiddenTemplate implementation
    public virtual IGenerateHtml HiddenTemplate(int screenOrderFrom = int.MinValue, int screenOrderTo = int.MaxValue, object? attributes = null)
    {
        var html = new HtmlStack();
            
        foreach (var option in Options)
        {
            if (option.Selected) html.Append(Render.HiddenForModel(option.Value, attributes));
        }
        html.Append(Render.HiddenForModel("", attributes));

        return html;
    }
    #endregion

    #region ISelfModelBinder implementation
    public Task<object?> BindMeAsync(Type rootType, List<IValueProvider> valueProviders)
    {
        var prefix = HttpContext.Current.PrefixManager.CurrentPrefix;

        if (string.IsNullOrEmpty(prefix)) throw new WebMonkException("prefix is not set");
        var name = prefix.ToHtmlName();

        var attemptedValues = valueProviders.GetValueOrDefault<List<string>>(name).GetCastValue<List<string>>();
        attemptedValues.Remove(""); //remove blank option. it must be always present

        if (attemptedValues.Count < 1)
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

        Options.ForEach(x => x.Selected = false);
        foreach (var selectedValue in attemptedValues)
        {
            var selectedOption = Options.Find(x => x.Value == selectedValue);
            if (selectedOption != null) selectedOption.Selected = true;
        }
        return Task.FromResult((object?)this);
    }
    #endregion

    #region Properties
    public List<Option> Options { get; protected set; } = [];
    public string DisabledSuffix { get; set; } = " [DISABLED]";
    #endregion
}