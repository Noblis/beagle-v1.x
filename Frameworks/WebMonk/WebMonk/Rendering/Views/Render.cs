using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Supermodel.DataAnnotations.Attributes;
using Supermodel.DataAnnotations.Exceptions;
using Supermodel.DataAnnotations.Expressions;
using WebMonk.Context;
using WebMonk.Exceptions;
using WebMonk.Extensions;
using WebMonk.HttpRequestHandlers.Controllers;
using WebMonk.Misc;
using WebMonk.RazorSharp.HtmlTags;
using WebMonk.RazorSharp.HtmlTags.BaseTags;
using WebMonk.Rendering.Templates;
using WebMonk.Results;
using WebMonk.ValueProviders;

namespace WebMonk.Rendering.Views;

public static class Render
{
    #region Helper Class
    public static class Helper
    {
        #region UrlForMvcActions
        public static string UrlForMvcAction<T>(Expression<Action<T>> action, NameValueCollection? queryString)  where T : MvcController
        {
            return UrlForMvcAction(action, queryString?.ToQueryStringDictionary());
        }
        public static string UrlForMvcAction<T>(Expression<Action<T>> action, QueryStringDict? queryStringDict = null)  where T : MvcController
        {
            var (controllerName, actionName, id, updatedQueryStringDict) = GetControllerActionIdQs(action, queryStringDict);
            return UrlForMvcAction(controllerName, actionName, id, updatedQueryStringDict);
        }

        public static string UrlForMvcAction(string controller, string action, string? id, NameValueCollection? queryString)
        {
            return UrlForMvcAction(controller, action, id, queryString?.ToQueryStringDictionary());
        }
        public static string UrlForMvcAction(string controller, string action, string? id = null, QueryStringDict? queryStringDict = null)
        {
            queryStringDict ??= new QueryStringDict();
            var optionalActionPart = string.IsNullOrEmpty(action) ? "" : $"/{action}";
            var optionalIdPart = string.IsNullOrEmpty(id) ? "" : $"/{id}";
            return $"/{controller.ToLower()}{optionalActionPart.ToLower()}{optionalIdPart.ToLower()}{queryStringDict.ToUrlEncodedNameValuePairs()}";
        }
        #endregion

        #region UrlForApiActions
        public static string UrlForApiAction<T>(Expression<Action<T>> action, NameValueCollection? queryString)  where T : ApiController
        {
            return UrlForApiAction(action, queryString?.ToQueryStringDictionary());
        }
        public static string UrlForApiAction<T>(Expression<Action<T>> action, QueryStringDict? queryStringDict = null)  where T : ApiController
        {
            var (controllerName, actionName, id, updatedQueryStringDict) = GetControllerActionIdQs(action, queryStringDict, true);
            return UrlForApiAction(controllerName, actionName, id, updatedQueryStringDict);
        }

        public static string UrlForApiAction(string controller, string action, string? id, NameValueCollection? queryString)
        {
            return UrlForApiAction(controller, action, id, queryString?.ToQueryStringDictionary());
        }
        public static string UrlForApiAction(string controller, string action, string? id = null, QueryStringDict? queryStringDict = null)
        {
            queryStringDict ??= new QueryStringDict();
            var optionalActionPart = string.IsNullOrEmpty(action) ? "" : $"/{action}";
            var optionalIdPart = string.IsNullOrEmpty(id) ? "" : $"/{id}";
            return $"/api/{controller.ToLower()}{optionalActionPart.ToLower()}{optionalIdPart.ToLower()}{queryStringDict.ToUrlEncodedNameValuePairs()}";
        }
            
        public static (string, string, string, QueryStringDict queryStringDict) GetControllerActionIdQs<T>(Expression<Action<T>> action, QueryStringDict? queryStringDict, bool isForApiController = false) where T : ControllerBase
        {
            var methodExpression = action.Body as MethodCallExpression;
            if (methodExpression == null) throw new InvalidOperationException("Expression must be a method call.");
            if (methodExpression.Object != action.Parameters[0]) throw new InvalidOperationException("Method call must target lambda argument.");
            if (!IsActionMethod(methodExpression.Method)) throw new InvalidOperationException("Expression must be a call to a valid action method.");

            var controllerName = isForApiController ? typeof(T).GetApiControllerName() : typeof(T).GetMvcControllerName();

            var actionName = methodExpression.Method.Name;
                
            if (actionName.StartsWith("Get")) actionName = actionName["Get".Length..];
            else if (actionName.StartsWith("Head")) actionName = actionName["Head".Length..];
            else if (actionName.StartsWith("Post")) actionName = actionName["Post".Length..];
            else if (actionName.StartsWith("Put")) actionName = actionName["Put".Length..];
            else if (actionName.StartsWith("Delete")) actionName = actionName["Delete".Length..];
            else if (actionName.StartsWith("Connect")) actionName = actionName["Connect".Length..];
            else if (actionName.StartsWith("Options")) actionName = actionName["Options".Length..];
            else if (actionName.StartsWith("Trace")) actionName = actionName["Trace".Length..];
            else if (actionName.StartsWith("Patch")) actionName = actionName["Patch".Length..];
            else throw new WebMonkException($"Action Method '{actionName}' musty start with a valid HTTP Method");

            if (actionName.EndsWith("Async")) actionName = actionName[..^5];

            string id = "";

            queryStringDict ??= new QueryStringDict();

            var parameters = methodExpression.Method.GetParameters();
            for (var i = 0; i < parameters.Length; i++)
            {
                var param = parameters[i];
                var argumentExpression = methodExpression.Arguments[i];
                var argument = Expression.Lambda(argumentExpression).Compile().DynamicInvoke();

                if (param.Name.ToLower() == "id")
                {
                    id = argument.ToString();
                }
                else
                {
                    if (argument != null) queryStringDict.Add(param.Name, argument.ToString());
                }
            }

            return (controllerName, actionName, id, queryStringDict);
        }
        #endregion

        #region Parsing Expressions for Routing
        // ReSharper disable once UnusedParameter.Local
        public static string GetPropertyName<TModel, TValue>(TModel model, Expression<Func<TModel, TValue>> expression)
        {
            if (expression.Body.NodeType == ExpressionType.Convert)
            {
                var body = (MemberExpression)((UnaryExpression)expression.Body).Operand;
                var propertyName = body.Member is PropertyInfo ? body.Member.Name : "";
                if (body.Expression.NodeType == ExpressionType.Parameter) return propertyName;
                return $"{GetExpressionName(body.Expression)}.{propertyName}";
            }
            else if (expression.Body.NodeType == ExpressionType.MemberAccess)
            {
                var body = (MemberExpression)expression.Body;
                var propertyName = body.Member is PropertyInfo ? body.Member.Name : "";
                if (body.Expression.NodeType == ExpressionType.Parameter) return propertyName;
                return $"{GetExpressionName(body.Expression)}.{propertyName}";
            }
            else if (expression.Body.NodeType == ExpressionType.Call)
            {
                var body = (MethodCallExpression)expression.Body;
                if (body.Method.Name != "get_Item") throw new ArgumentException("Expression must describe a property or an indexer", nameof(expression));
                return GetExpressionName(body);
            }
            else if (expression.Body.NodeType == ExpressionType.ArrayIndex)
            {
                var body = (BinaryExpression)expression.Body;
                return GetExpressionName(body);
            }
            else if (expression.Body.NodeType == ExpressionType.Parameter)
            {
                var body = (ParameterExpression)expression.Body;
                return GetExpressionName(body);
            }                
            else
            {
                throw new ArgumentException("Expression must describe a property or an indexer", nameof(expression));
            }
        }
        private static string GetExpressionName(Expression expression)
        {
            if (expression.NodeType == ExpressionType.Parameter) return "";

            if (expression.NodeType == ExpressionType.MemberAccess)
            {
                var memberExpression = (MemberExpression)expression;
                return $"{GetExpressionName(memberExpression.Expression)}{memberExpression.Member.Name}";
            }

            if (expression.NodeType == ExpressionType.Call)
            {
                var methodCallExpression = (MethodCallExpression)expression;
                var indexExpression = methodCallExpression.Arguments[0];
                var indexExpressionResult = Expression.Lambda(indexExpression).Compile().DynamicInvoke();
                return $"{GetExpressionName(methodCallExpression.Object!)}[{indexExpressionResult}]";
            }

            if (expression.NodeType == ExpressionType.ArrayIndex)
            {
                var binaryExpression = (BinaryExpression)expression;
                var indexExpression = binaryExpression.Right;
                var indexExpressionResult = Expression.Lambda(indexExpression).Compile().DynamicInvoke();
                return $"{GetExpressionName(binaryExpression.Left)}[{indexExpressionResult}]";
            }

            throw new Exception("Invalid Expression '" + expression + "'");
        }
        #endregion

        #region AttemptedValue Helper
        public static string AdjustIfInvalid<TModel, TValue>(string? value, TModel model, Expression<Func<TModel, TValue>> propertyExpression)
        {
            var propertyName = GetPropertyName(model, propertyExpression);
            return AdjustIfInvalid(value, propertyName);
        }
        public static string AdjustIfInvalid(string? value, string expression)
        {
            value ??= "";

            if (HttpContext.Current.ValidationResultList.IsValid) return value;
                
            var prefix = HttpContext.Current.PrefixManager.CurrentPrefix;

            if (!string.IsNullOrEmpty(prefix) && !string.IsNullOrEmpty(expression)) prefix = $"{prefix}.";
            var id = $"{prefix}{expression}".ToHtmlId();
            var name = id.ToHtmlName();

            var valueProviders = HttpContext.Current.ValueProviderManager.GetCachedValueProvidersList();
            if (valueProviders != null) 
            {
                var propertyValueResult = valueProviders.GetValueOrDefault(name);
                value=propertyValueResult.UpdateInternal(value);
            }

            return value;
        }
        #endregion

        #region IsActionMethod
        public static bool IsActionMethod(MethodInfo methodInfo)
        {
            if (methodInfo.ReturnType.IsGenericType)
            {
                //check that method name ends with Async
                if (!methodInfo.Name.EndsWith("Async")) return false;

                //check that return type is Task<T> where T derives from ActionResult
                if (methodInfo.ReturnType.GetGenericTypeDefinition() != typeof(Task<>)) return false;
                if (!typeof(ActionResult).IsAssignableFrom(methodInfo.ReturnType.GenericTypeArguments[0])) return false;
            }
            else
            {
                if (methodInfo.Name.EndsWith("Async")) return false;

                //check that return type derives from ActionResult
                if (!typeof(ActionResult).IsAssignableFrom(methodInfo.ReturnType)) return false;
            }

            if (!methodInfo.Name.StartsWith("Get") && 
                !methodInfo.Name.StartsWith("Post") &&
                !methodInfo.Name.StartsWith("Put") &&
                !methodInfo.Name.StartsWith("Delete") &&
                !methodInfo.Name.StartsWith("Patch") &&
                !methodInfo.Name.StartsWith("Head") &&
                !methodInfo.Name.StartsWith("Connects") &&
                !methodInfo.Name.StartsWith("Options") &&
                !methodInfo.Name.StartsWith("Trace")) return false;
            return true;
        }
        #endregion
    }
    #endregion

    #region Render Label Methods
    public static Label LabelFor<TModel, TValue>(TModel model, Expression<Func<TModel, TValue>> propertyExpression, string? label = null, object? attributes = null)
    {
        var propertyName = Helper.GetPropertyName(model, propertyExpression);
        return Label(model, propertyName, label, attributes);
    }
    public static Label Label<TModel>(TModel model, string expression, string? label = null, object? attributes = null)
    {
        var prefix = HttpContext.Current.PrefixManager.CurrentPrefix;

        if (model == null) throw new ArgumentNullException(nameof(model));

        if (!string.IsNullOrEmpty(prefix) && !string.IsNullOrEmpty(expression)) prefix = $"{prefix}.";
        var id = $"{prefix}{expression}".ToHtmlId();

        label ??= model.GetType().GetDisplayNameForProperty(expression);
            
        var labelTag = new Label(new { @for=id }) { new Txt( label) };
        labelTag.AddOrUpdateAttr(attributes);
        return labelTag;
    }
    #endregion

    #region Render Validation Methods
    public static IGenerateHtml ValidationSummary(object? ulAttributes = null, object? liAttributes = null)
    {
        if (HttpContext.Current.ValidationResultList.IsValid) return new Tags();

        var ulTag = new Ul(ulAttributes)
        { 
            new CodeBlock(() => 
            { 
                var errorMessages = HttpContext.Current.ValidationResultList.Select(x => x.ErrorMessage);
                var tags = new Tags();
                foreach (var errorMessage in errorMessages)
                {
                    tags.Add(new Li(liAttributes) { new Txt(errorMessage!) });
                }
                return tags;
            })
        };
        return ulTag;
    }
    public static IGenerateHtml ValidationMessageFor<TModel, TValue>(TModel model, Expression<Func<TModel, TValue>> propertyExpression, object? attributes = null, bool returnDiv = false)
    {
        var propertyName = Helper.GetPropertyName(model, propertyExpression);
        return ValidationMessage(model, propertyName, attributes, returnDiv);
    }
    public static IGenerateHtml ValidationMessage<TModel>(TModel model, string expression, object? attributes = null, bool returnDiv = false)
    {
        if (HttpContext.Current.ValidationResultList.IsValid) return new Tags();
            
        var prefix = HttpContext.Current.PrefixManager.CurrentPrefix;

        if (model == null) throw new ArgumentNullException(nameof(model));

        if (!string.IsNullOrEmpty(prefix) && !string.IsNullOrEmpty(expression)) prefix = $"{prefix}.";
        var id = $"{prefix}{expression}".ToHtmlId();
        var name = id.ToHtmlName();

        var errors = HttpContext.Current.ValidationResultList.GetAllErrorsFor(name);
        if (!errors.Any()) return new Tags();

        if (returnDiv)
        {
            var divTag = new Span(new { data_valmsg_for=id }) { new Txt( errors.First()) };
            divTag.AddOrUpdateAttr(attributes);
            return divTag;
        }
        else
        {
            var spanTag = new Span(new { data_valmsg_for=id }) { new Txt( errors.First()) };
            spanTag.AddOrUpdateAttr(attributes);
            return spanTag;
        }
    }
    #endregion

    #region Render Editor Methods
    public static IGenerateHtml EditorForModel<TModel>(TModel model, object? attributes = null)
    {
        return Editor(model, "", attributes);
    }
    public static IGenerateHtml EditorFor<TModel, TValue>(TModel model, Expression<Func<TModel, TValue>> propertyExpression, object? attributes = null)
    {
        var propertyName = Helper.GetPropertyName(model, propertyExpression);
        return Editor(model, propertyName, attributes);
    }
    public static IGenerateHtml Editor<TModel>(TModel model, string expression, object? attributes = null)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));

        var prefix = HttpContext.Current.PrefixManager.CurrentPrefix;

        if (!string.IsNullOrEmpty(prefix) && !string.IsNullOrEmpty(expression)) prefix = $"{prefix}.";
        var id = $"{prefix}{expression}".ToHtmlId();
        var name = id.ToHtmlName();

        var (propertyInfo, propertyType, propertyValue) = model.GetPropertyInfoPropertyTypeAndValueByFullName(expression);
        if (typeof(IEditorTemplate).IsAssignableFrom(propertyType))
        {
            using(HttpContext.Current.PrefixManager.NewPrefix(expression, model))
            {
                propertyValue ??= Activator.CreateInstance(propertyType);
                if (propertyValue is IEditorTemplate template) return template.EditorTemplate();
                else throw new WebMonkException("This should never happen: propertyValue is not IEditorTemplate");
            }
        }

        //IGenerateHtml
        if(typeof(IGenerateHtml).IsAssignableFrom(propertyType))
        {
            return (IGenerateHtml)(propertyValue ?? new Tags());
        }

        Tag editorTag;
        Tag? hiddenInputTag = null;

        //strings
        if (typeof(string).IsAssignableFrom(propertyType))
        {
            var attr = propertyInfo?.GetCustomAttribute<DataTypeAttribute>(true);
            if (attr == null)
            {
                editorTag = new Input(new { type="text", id, name, value=Helper.AdjustIfInvalid(propertyValue?.ToString(), expression) });
            }
            else
            {
                switch(attr.DataType)
                {
                    case DataType.Password: editorTag = new Input(new { type="password", id, name, value=Helper.AdjustIfInvalid(propertyValue?.ToString(), expression)}); break;
                    case DataType.EmailAddress: editorTag = new Input(new { type="email", id, name, value=Helper.AdjustIfInvalid(propertyValue?.ToString(), expression)}); break;
                    case DataType.MultilineText: editorTag = new Textarea(new { id, name}){ new Txt(Helper.AdjustIfInvalid(propertyValue?.ToString(), expression))}; break;
                    case DataType.PhoneNumber: editorTag = new Input(new { type="tel", id, name, value=Helper.AdjustIfInvalid(propertyValue?.ToString(), expression)}); break;
                    default: editorTag = new Input(new { type="text", id, name, value=Helper.AdjustIfInvalid(propertyValue?.ToString(), expression)}); break;
                }
            }
        }
            
        //integer types
        else if (typeof(int).IsAssignableFrom(propertyType) ||
                 typeof(int?).IsAssignableFrom(propertyType) ||
                 typeof(uint).IsAssignableFrom(propertyType) ||
                 typeof(uint?).IsAssignableFrom(propertyType) ||
                 typeof(long).IsAssignableFrom(propertyType) ||
                 typeof(long?).IsAssignableFrom(propertyType) ||
                 typeof(ulong).IsAssignableFrom(propertyType) ||
                 typeof(ulong?).IsAssignableFrom(propertyType) ||
                 typeof(short).IsAssignableFrom(propertyType) ||
                 typeof(short?).IsAssignableFrom(propertyType) ||
                 typeof(ushort).IsAssignableFrom(propertyType) ||
                 typeof(ushort?).IsAssignableFrom(propertyType) ||
                 typeof(byte).IsAssignableFrom(propertyType) ||
                 typeof(byte?).IsAssignableFrom(propertyType) ||
                 typeof(sbyte).IsAssignableFrom(propertyType) ||
                 typeof(sbyte?).IsAssignableFrom(propertyType))
        {
            editorTag = new Input(new { type="number", id, name, value=Helper.AdjustIfInvalid(propertyValue?.ToString(), expression)});
        }
            
        //floating point types
        else if (typeof(double).IsAssignableFrom(propertyType) ||
                 typeof(double?).IsAssignableFrom(propertyType) ||
                 typeof(float).IsAssignableFrom(propertyType) ||
                 typeof(float?).IsAssignableFrom(propertyType) ||
                 typeof(decimal).IsAssignableFrom(propertyType) ||
                 typeof(decimal?).IsAssignableFrom(propertyType))
        {
            editorTag = new Input(new { type="number", step="any", id, name, value=Helper.AdjustIfInvalid(propertyValue?.ToString(), expression)});
        }

        //booleans (special case)
        else if (typeof(bool).IsAssignableFrom(propertyType) ||
                 typeof(bool?).IsAssignableFrom(propertyType))
        {
            //this preserves the attempted values for special case of bool
            bool @checked;
            if (!HttpContext.Current.ValidationResultList.IsValid)
            {
                var valueProviders = HttpContext.Current.ValueProviderManager.GetCachedValueProvidersList() ?? throw new WebMonkException("This should never happen: valueProviders == null");
                var propertyValueResult = valueProviders.GetValueOrDefault<bool?>(name);
                @checked = propertyValueResult.GetCastValue<bool?>() == true;
            }
            else
            {
                @checked = (bool?)propertyValue == true;
            }

            editorTag = new Input(new { type="checkbox", id, name, value="true" } );
            if (@checked) editorTag.Attributes.Add("checked", "on");

            hiddenInputTag = new Input(new { type="hidden", name, value="false" });
        }

        //DateTime
        else if (typeof(DateTime).IsAssignableFrom(propertyType) ||
                 typeof(DateTime?).IsAssignableFrom(propertyType))
        {
            editorTag = new Input(new { type="datetime-local", id, name, value=Helper.AdjustIfInvalid(((DateTime?)propertyValue)?.ToString("yyyy-MM-ddTHH:mm"), expression)});
        }

        //enums
        else if (typeof(Enum).IsAssignableFrom(propertyType) ||
                 Nullable.GetUnderlyingType(propertyType)?.IsEnum == true)
        {
            editorTag = new Input(new { type="text", id, name, value=Helper.AdjustIfInvalid(propertyValue?.ToString(), expression)});
        }

        //byte array
        else if (typeof(byte[]).IsAssignableFrom(propertyType))
        {
            editorTag = new Input(new { type="file", id, name});
        }

        //Guid
        else if (typeof(Guid).IsAssignableFrom(propertyType))
        {
            editorTag = new Input(new { type="text", id, name, value=Helper.AdjustIfInvalid(propertyValue?.ToString(), expression)});
        }
            
        //catch-all for primitive types
        else if (propertyType.IsPrimitive)
        {
            editorTag = new Input(new { type="text", id, name, value=Helper.AdjustIfInvalid(propertyValue?.ToString(), expression)});
        }
            
        //catch-all for complex types
        else
        {
            return new Tags();
        }

        editorTag.AddOrUpdateAttr(attributes);

        if (hiddenInputTag == null) return editorTag;
        else return new Tags { editorTag, hiddenInputTag };
    }
    #endregion
        
    #region Render Display Methods
    public static IGenerateHtml DisplayForModel<TModel>(TModel model, object? attributes = null)
    {
        return Display(model, "", attributes);
    }
    public static IGenerateHtml DisplayFor<TModel, TValue>(TModel model, Expression<Func<TModel, TValue>> propertyExpression, object? attributes = null)
    {
        var propertyName = Helper.GetPropertyName(model, propertyExpression);
        return Display(model, propertyName, attributes);
    }
    public static IGenerateHtml Display<TModel>(TModel model, string expression, object? attributes = null)
    {
        var prefix = HttpContext.Current.PrefixManager.CurrentPrefix;

        if (model == null) throw new ArgumentNullException(nameof(model));

        if (!string.IsNullOrEmpty(prefix) && !string.IsNullOrEmpty(expression)) prefix = $"{prefix}.";
        var id = $"{prefix}{expression}".ToHtmlId();

        var (_, propertyType, propertyValue) = model.GetPropertyInfoPropertyTypeAndValueByFullName(expression);
        if (typeof(IDisplayTemplate).IsAssignableFrom(propertyType))
        {
            using(HttpContext.Current.PrefixManager.NewPrefix(expression, model))
            {
                propertyValue ??= Activator.CreateInstance(propertyType);
                if (propertyValue is IDisplayTemplate template) return template.DisplayTemplate();
                else throw new WebMonkException("This should never happen: propertyValue is not IDisplayTemplate");
            }
        }

        //IGenerateHtml
        if(typeof(IGenerateHtml).IsAssignableFrom(propertyType))
        {
            return (IGenerateHtml)(propertyValue ?? new Tags());
        }

        var displayTag = new Span(new { id });
        displayTag.AddOrUpdateAttr(attributes);

        //strings
        if (typeof(string).IsAssignableFrom(propertyType))
        {
            displayTag.Add(new Txt(propertyValue?.ToString() ?? ""));
        }
            
        //integer types
        else if (typeof(int).IsAssignableFrom(propertyType) ||
                 typeof(int?).IsAssignableFrom(propertyType) ||
                 typeof(uint).IsAssignableFrom(propertyType) ||
                 typeof(uint?).IsAssignableFrom(propertyType) ||
                 typeof(long).IsAssignableFrom(propertyType) ||
                 typeof(long?).IsAssignableFrom(propertyType) ||
                 typeof(ulong).IsAssignableFrom(propertyType) ||
                 typeof(ulong?).IsAssignableFrom(propertyType) ||
                 typeof(short).IsAssignableFrom(propertyType) ||
                 typeof(short?).IsAssignableFrom(propertyType) ||
                 typeof(ushort).IsAssignableFrom(propertyType) ||
                 typeof(ushort?).IsAssignableFrom(propertyType) ||
                 typeof(byte).IsAssignableFrom(propertyType) ||
                 typeof(byte?).IsAssignableFrom(propertyType) ||
                 typeof(sbyte).IsAssignableFrom(propertyType) ||
                 typeof(sbyte?).IsAssignableFrom(propertyType))
        {
            displayTag.Add(new Txt(propertyValue?.ToString() ?? ""));
        }
            
        //floating point types
        else if (typeof(double).IsAssignableFrom(propertyType) ||
                 typeof(double?).IsAssignableFrom(propertyType) ||
                 typeof(float).IsAssignableFrom(propertyType) ||
                 typeof(float?).IsAssignableFrom(propertyType) ||
                 typeof(decimal).IsAssignableFrom(propertyType) ||
                 typeof(decimal?).IsAssignableFrom(propertyType))
        {
            displayTag.Add(new Txt(propertyValue?.ToString() ?? ""));
        }

        //booleans
        else if (typeof(bool).IsAssignableFrom(propertyType) ||
                 typeof(bool?).IsAssignableFrom(propertyType))
        {
            displayTag.Add(new Txt((bool?)propertyValue == true ? "Yes" : "No" ));
        }

        //DateTime
        else if (typeof(DateTime).IsAssignableFrom(propertyType) ||
                 typeof(DateTime?).IsAssignableFrom(propertyType))
        {
            displayTag.Add(new Txt(propertyValue?.ToString() ?? ""));
        }

        //enums
        else if (typeof(Enum).IsAssignableFrom(propertyType) ||
                 Nullable.GetUnderlyingType(propertyType)?.IsEnum == true)
        {
            displayTag.Add(new Txt(propertyValue?.ToString() ?? ""));
        }

        //byte array
        else if (typeof(byte[]).IsAssignableFrom(propertyType))
        {
            return new Tags();
        }

        //Guid
        else if (typeof(Guid).IsAssignableFrom(propertyType))
        {
            displayTag.Add(new Txt(propertyValue?.ToString() ?? ""));
        }

        //catch-all for primitive types
        else if (propertyType.IsPrimitive)
        {
            displayTag.Add(new Txt(propertyValue?.ToString() ?? ""));
        }

        //catch-all for complex types
        else
        {
            return new Tags();
        }

        displayTag.AddOrUpdateAttr(attributes);
        return displayTag;        
    }
    #endregion

    #region Render Hidden Methods
    public static IGenerateHtml HiddenForModel<TModel>(TModel model, object? attributes = null)
    {
        return Hidden(model, "", attributes);
    }
    public static IGenerateHtml HiddenFor<TModel, TValue>(TModel model, Expression<Func<TModel, TValue>> propertyExpression, object? attributes = null)
    {
        var propertyName = Helper.GetPropertyName(model, propertyExpression);
        return Hidden(model, propertyName, attributes);
    }
    public static IGenerateHtml Hidden<TModel>(TModel model, string expression, object? attributes = null)
    {
        var prefix = HttpContext.Current.PrefixManager.CurrentPrefix;

        if (model == null) throw new ArgumentNullException(nameof(model));

        if (!string.IsNullOrEmpty(prefix) && !string.IsNullOrEmpty(expression)) prefix = $"{prefix}.";
        var id = $"{prefix}{expression}".ToHtmlId();
        var name = id.ToHtmlName();

        var (_, propertyType, propertyValue) = model.GetPropertyInfoPropertyTypeAndValueByFullName(expression);
        if (typeof(IHiddenTemplate).IsAssignableFrom(propertyType))
        {
            using(HttpContext.Current.PrefixManager.NewPrefix(expression, model))
            {
                propertyValue ??= Activator.CreateInstance(propertyType);
                if (propertyValue is IHiddenTemplate template) return template.HiddenTemplate();
                else throw new WebMonkException("This should never happen: propertyValue is not IEditorTemplate");
            }
        }
            
        //IGenerateHtml
        if(typeof(IGenerateHtml).IsAssignableFrom(propertyType))
        {
            return (IGenerateHtml)(propertyValue ?? new Tags());
        }

        Tag hiddenTag;

        //strings
        if (typeof(string).IsAssignableFrom(propertyType))
        {
            hiddenTag = new Input(new { type="hidden", id, name, value=propertyValue?.ToString()??""});
        }
            
        //integer types
        else if (typeof(int).IsAssignableFrom(propertyType) ||
                 typeof(int?).IsAssignableFrom(propertyType) ||
                 typeof(uint).IsAssignableFrom(propertyType) ||
                 typeof(uint?).IsAssignableFrom(propertyType) ||
                 typeof(long).IsAssignableFrom(propertyType) ||
                 typeof(long?).IsAssignableFrom(propertyType) ||
                 typeof(ulong).IsAssignableFrom(propertyType) ||
                 typeof(ulong?).IsAssignableFrom(propertyType) ||
                 typeof(short).IsAssignableFrom(propertyType) ||
                 typeof(short?).IsAssignableFrom(propertyType) ||
                 typeof(ushort).IsAssignableFrom(propertyType) ||
                 typeof(ushort?).IsAssignableFrom(propertyType) ||
                 typeof(byte).IsAssignableFrom(propertyType) ||
                 typeof(byte?).IsAssignableFrom(propertyType) ||
                 typeof(sbyte).IsAssignableFrom(propertyType) ||
                 typeof(sbyte?).IsAssignableFrom(propertyType))
        {
            hiddenTag = new Input(new { type="hidden", id, name, value=propertyValue?.ToString()??""});
        }
            
        //floating point types
        else if (typeof(double).IsAssignableFrom(propertyType) ||
                 typeof(double?).IsAssignableFrom(propertyType) ||
                 typeof(float).IsAssignableFrom(propertyType) ||
                 typeof(float?).IsAssignableFrom(propertyType) ||
                 typeof(decimal).IsAssignableFrom(propertyType) ||
                 typeof(decimal?).IsAssignableFrom(propertyType))
        {
            hiddenTag = new Input(new { type="hidden", id, name, value=propertyValue?.ToString()??""});
        }

        //booleans
        else if (typeof(bool).IsAssignableFrom(propertyType) ||
                 typeof(bool?).IsAssignableFrom(propertyType))
        {
            hiddenTag = new Input(new { type="hidden", id, name, value=((bool?)propertyValue == true).ToString().ToLower()});
        }

        //DateTime
        else if (typeof(DateTime).IsAssignableFrom(propertyType) ||
                 typeof(DateTime?).IsAssignableFrom(propertyType))
        {
            hiddenTag = new Input(new { type="hidden", id, name, value=((DateTime?)propertyValue)?.ToString("yyyy-MM-ddTHH:mm")??""});
        }

        //enums
        else if (typeof(Enum).IsAssignableFrom(propertyType) ||
                 Nullable.GetUnderlyingType(propertyType)?.IsEnum == true)
        {
            hiddenTag = new Input(new { type="hidden", id, name, value=propertyValue?.ToString()??""});
        }

        //byte array
        else if (typeof(byte[]).IsAssignableFrom(propertyType))
        {
            return new Tags();
        }

        //Guid
        else if (typeof(Guid).IsAssignableFrom(propertyType))
        {
            hiddenTag = new Input(new { type="hidden", id, name, value=propertyValue?.ToString()??""});
        }

        //catch-all for primitive types
        else if (propertyType.IsPrimitive)            
        {
            hiddenTag = new Input(new { type="hidden", id, name, value=propertyValue?.ToString()??""});
        }

        //catch-all for complex types
        else
        {
            return new Tags();
        }

        hiddenTag.AddOrUpdateAttr(attributes);
        return hiddenTag;
    }
    #endregion

    #region Render Form Controls
    public static Input TextBoxForModel<TModel>(TModel model, object? attributes = null, string? format = null)
    {
        return TextBox(model, "", attributes, format);
    }
    public static Input TextBoxFor<TModel, TValue>(TModel model, Expression<Func<TModel, TValue>> propertyExpression, object? attributes = null, string? format = null)
    {
        var propertyName = Helper.GetPropertyName(model, propertyExpression);
        return TextBox(model, propertyName, attributes, format);
    }
    public static Input TextBox<TModel>(TModel model, string expression, object? attributes = null, string? format = null)
    {
        return InputHelper("text", model, expression, attributes, format);
    }
        
    public static Input PasswordForModel(string model, object? attributes = null)
    {
        return Password(model, "", attributes);
    }
    public static Input PasswordFor<TModel, TValue>(TModel model, Expression<Func<TModel, TValue>> propertyExpression, object? attributes = null)
    {
        var propertyName = Helper.GetPropertyName(model, propertyExpression);
        return Password(model, propertyName, attributes);
    }
    public static Input Password<TModel>(TModel model, string expression, object? attributes = null)
    {
        return InputHelper("password", model, expression, attributes, null);
    }

    private static Input InputHelper<TModel>(string inputType, TModel model, string expression, object? attributes, string? format)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));

        var prefix = HttpContext.Current.PrefixManager.CurrentPrefix;

        if (!string.IsNullOrEmpty(prefix) && !string.IsNullOrEmpty(expression)) prefix = $"{prefix}.";
        var id = $"{prefix}{expression}".ToHtmlId();
        var name = id.ToHtmlName();

        var (_, _, propertyValue) = model.GetPropertyInfoPropertyTypeAndValueByFullName(expression);

        //this preserves the attempted values
        if (!HttpContext.Current.ValidationResultList.IsValid)
        {
            var valueProviders = HttpContext.Current.ValueProviderManager.GetCachedValueProvidersList();
            if (valueProviders != null) 
            {
                var propertyValueResult = valueProviders.GetValueOrDefault(name);
                propertyValue = propertyValueResult.UpdateInternal(propertyValue);
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(format)) propertyValue = string.Format(format, propertyValue);
            else propertyValue = propertyValue?.ToString() ?? "";
        }

        var tag = (Input)new Input(new { type=inputType, id, name, value=propertyValue } ).AddOrUpdateAttr(attributes);
        return tag;
    }

    public static Input RadioButtonForModel(string? model, string value, object? attributes = null)
    {
        return RadioButton(model, "", value, attributes);
    }
    public static Input RadioButtonFor<TModel, TValue>(TModel model, Expression<Func<TModel, TValue>> propertyExpression, string value, object? attributes = null)
    {
        var propertyName = Helper.GetPropertyName(model, propertyExpression);
        return RadioButton(model, propertyName, value, attributes);
    }
    public static Input RadioButton<TModel>(TModel model, string expression, string value, object? attributes = null)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));

        var prefix = HttpContext.Current.PrefixManager.CurrentPrefix;

        if (!string.IsNullOrEmpty(prefix) && !string.IsNullOrEmpty(expression)) prefix = $"{prefix}.";
        var id = $"{prefix}{expression}".ToHtmlId();
        var name = id.ToHtmlName();

        var (_, _, propertyValue) = model.GetPropertyInfoPropertyTypeAndValueByFullName(expression);

        //this preserves the attempted values
        if (!HttpContext.Current.ValidationResultList.IsValid)
        {
            var valueProviders = HttpContext.Current.ValueProviderManager.GetCachedValueProvidersList();
            if (valueProviders != null) 
            {
                var propertyValueResult = valueProviders.GetValueOrDefault(name);
                propertyValue = propertyValueResult.UpdateInternal(propertyValue);
            }
        }
        else
        {
            propertyValue = propertyValue?.ToString() ?? "";
        }

        var tag = new Input(new { type="radio", id, name, value } );
        if (propertyValue?.ToString() == value) tag.AddOrUpdateAttr(new { @checked = "checked" });
            
        tag.AddOrUpdateAttr(attributes);

        return tag;            
    }

    public static Tags CheckBoxForModel(bool model, object? attributes = null)
    {
        return CheckBox(model, "", attributes);
    }
    public static Tags CheckBoxFor<TModel, TValue>(TModel model, Expression<Func<TModel, TValue>> propertyExpression, object? attributes = null)
    {
        var propertyName = Helper.GetPropertyName(model, propertyExpression);
        return CheckBox(model, propertyName, attributes);
    }
    public static Tags CheckBox<TModel>(TModel model, string expression, object? attributes = null)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));

        var prefix = HttpContext.Current.PrefixManager.CurrentPrefix;

        if (!string.IsNullOrEmpty(prefix) && !string.IsNullOrEmpty(expression)) prefix = $"{prefix}.";
        var id = $"{prefix}{expression}".ToHtmlId();
        var name = id.ToHtmlName();

        var (_, _, propertyValue) = model.GetPropertyInfoPropertyTypeAndValueByFullName(expression);

        //this preserves the attempted values for special case of bool
        bool @checked;
        if (!HttpContext.Current.ValidationResultList.IsValid)
        {
            var valueProviders = HttpContext.Current.ValueProviderManager.GetCachedValueProvidersList() ?? throw new WebMonkException("This should never happen: valueProviders == null");
            var propertyValueResult = valueProviders.GetValueOrDefault<bool?>(name);
            @checked = propertyValueResult.GetCastValue<bool?>() == true;
        }
        else
        {
            @checked = (bool?)propertyValue == true;
        }

        var tag = new Input(new { type="checkbox", id, name, value="true" } );
        if (@checked) tag.Attributes.Add("checked", "on");
        tag.AddOrUpdateAttr(attributes);

        return [tag, new Input(new { type = "hidden", name, value = "false" })];
    }

    public static Input FilePickerForModel(byte[]? model, object? attributes = null)
    {
        return FilePicker(model, "", attributes);
    }
    public static Input FilePickerFor<TModel, TValue>(TModel model, Expression<Func<TModel, TValue>> propertyExpression, object? attributes = null)
    {
        var propertyName = Helper.GetPropertyName(model, propertyExpression);
        return FilePicker(model, propertyName, attributes);
    }
    // ReSharper disable once UnusedParameter.Global
    public static Input FilePicker<TModel>(TModel model, string expression, object? attributes = null)
    {
        var prefix = HttpContext.Current.PrefixManager.CurrentPrefix;

        if (!string.IsNullOrEmpty(prefix) && !string.IsNullOrEmpty(expression)) prefix = $"{prefix}.";
        var id = $"{prefix}{expression}".ToHtmlId();
        var name = id.ToHtmlName();

        return (Input)new Input(new { type="file", id, name}).AddOrUpdateAttr(attributes);
    }

    public static Textarea TextAreaForModel(string model, object? attributes = null, string? format = null)
    {
        return TextArea(model, "", attributes, format);
    }
    public static Textarea TextAreaFor<TModel, TValue>(TModel model, Expression<Func<TModel, TValue>> propertyExpression, object? attributes = null, string? format = null)
    {
        var propertyName = Helper.GetPropertyName(model, propertyExpression);
        return TextArea(model, propertyName, attributes, format);
    }
    public static Textarea TextArea<TModel>(TModel model, string expression, object? attributes = null, string? format = null)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));

        var prefix = HttpContext.Current.PrefixManager.CurrentPrefix;

        if (!string.IsNullOrEmpty(prefix) && !string.IsNullOrEmpty(expression)) prefix = $"{prefix}.";
        var id = $"{prefix}{expression}".ToHtmlId();
        var name = id.ToHtmlName();

        var (_, _, propertyValue) = model.GetPropertyInfoPropertyTypeAndValueByFullName(expression);

        //this preserves the attempted values
        if (!HttpContext.Current.ValidationResultList.IsValid)
        {
            var valueProviders = HttpContext.Current.ValueProviderManager.GetCachedValueProvidersList();
            if (valueProviders != null) 
            {
                var propertyValueResult = valueProviders.GetValueOrDefault(name);
                propertyValue = propertyValueResult.UpdateInternal(propertyValue);
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(format)) propertyValue = string.Format(format, propertyValue);
            else propertyValue = propertyValue?.ToString() ?? "";
        }

        var tag = (Textarea)new Textarea(new { id, name, rows = 3, cols = 20 } )
        {
            new Txt((string)(propertyValue ?? throw new WebMonkException("propertyValue == null")))
        }.AddOrUpdateAttr(attributes);
        return tag;
    }

    public static IGenerateHtml DropdownListForModel(string? model, IEnumerable<SelectListItem> options, object? attributes = null)
    {
        return DropdownList(model, "", options, attributes);
    }
    public static IGenerateHtml DropdownListFor<TModel, TValue>(TModel model, Expression<Func<TModel, TValue>> propertyExpression, IEnumerable<SelectListItem> options, object? attributes = null)
    {
        var propertyName = Helper.GetPropertyName(model, propertyExpression);
        return DropdownList(model, propertyName, options, attributes);
    }
    public static IGenerateHtml DropdownList<TModel>(TModel model, string expression, IEnumerable<SelectListItem> options, object? attributes = null)
    {
        if (model == null && !string.IsNullOrEmpty(expression)) throw new ArgumentNullException(nameof(model));

        var prefix = HttpContext.Current.PrefixManager.CurrentPrefix;

        if (!string.IsNullOrEmpty(prefix) && !string.IsNullOrEmpty(expression)) prefix = $"{prefix}.";
        var id = $"{prefix}{expression}".ToHtmlId();
        var name = id.ToHtmlName();

        object? propertyValue;
        if (model == null) 
        {
            if (!string.IsNullOrEmpty(expression)) throw new ArgumentNullException(nameof(model));
            propertyValue = "";
        }
        else 
        {
            (_, _, propertyValue) = model.GetPropertyInfoPropertyTypeAndValueByFullName(expression);
        }
            
        //this preserves the attempted values
        if (!HttpContext.Current.ValidationResultList.IsValid)
        {
            var valueProviders = HttpContext.Current.ValueProviderManager.GetCachedValueProvidersList();
            if (valueProviders != null) 
            {
                var propertyValueResult = valueProviders.GetValueOrDefault(name);
                propertyValue = propertyValueResult.UpdateInternal(propertyValue);
            }
        }

        var select = new HtmlStack();
        //var select = new HtmlStack(new Select(new { id, name }).AddOrUpdateAttr(attributes));
        select.AppendAndPush(new Select(new { id, name }).AddOrUpdateAttr(attributes));
        var optionGroups = options.GroupBy(x => x.OptionGroup).ToArray();
        foreach (var optionGroup in optionGroups)
        {
            if (optionGroup.Key != null) select.AppendAndPush(new Optgroup(new { label = optionGroup.Key }));
            foreach (var selectOption in optionGroup)
            {
                var option = select.Append(new Option(new { value=selectOption.Value }){ new Txt(selectOption.Label)});
                if (propertyValue?.ToString() == selectOption.Value) option.AddOrUpdateAttr(new { selected="selected" });
            }
            if (optionGroup.Key != null) select.Pop<Optgroup>();
        }
        select.Pop<Select>();
        return select;
    }
    #endregion

    #region Render Http Overrdie Methods
    public static IGenerateHtml HttpMethodOverride(HttpMethod httpMethod)
    {
        string httpMethodStr;
        switch (httpMethod)
        {
            case HttpMethod.Put:
                httpMethodStr = "PUT";
                break;
            case HttpMethod.Delete:
                httpMethodStr = "DELETE";
                break;
            default:
                throw new ArgumentException("Invalid Http Verb", nameof(httpMethod));
        }
        return HttpMethodOverride(httpMethodStr);
    }
    public static IGenerateHtml HttpMethodOverride(string httpMethod)
    {
        if (string.IsNullOrEmpty(httpMethod)) throw new ArgumentNullException(nameof(httpMethod));
            
        if (string.Equals(httpMethod, "GET", StringComparison.OrdinalIgnoreCase) || 
            string.Equals(httpMethod, "POST", StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("Invalid Http Method", nameof(httpMethod));

        return new Input(new { type="hidden", name="X-HTTP-Method-Override", value=httpMethod });
    }
    #endregion

    #region Render ActionLink Methods
    public static IGenerateHtml ActionLink<T>(string label, Expression<Action<T>> action, object? attributes = null, bool generateInline = false) where T : MvcController
    {
        // ReSharper disable once RedundantCast
        return ActionLink(label, action, (QueryStringDict?)null, attributes, generateInline);
    }
    //private static IGenerateHtml ActionLink<T>(string label, Expression<Action<T>> action, NameValueCollection queryString, object? attributes = null) where T : MvcController
    //{
    //    return ActionLink(label, action, queryString.ToQueryStringDictionary(), attributes, generateInline);
    //}
    private static IGenerateHtml ActionLink<T>(string label, Expression<Action<T>> action, QueryStringDict? queryStringDict = null, object? attributes = null, bool generateInline = false) where T : MvcController
    {
        var (controllerName, actionName, id, updatedQueryStringDict) = Helper.GetControllerActionIdQs(action, queryStringDict);
        return ActionLinkStrId(label, controllerName, actionName, id, updatedQueryStringDict, attributes, generateInline);
    }

    public static IGenerateHtml ActionLink(string label, string controller, string action, long? id, object? attributes = null, bool generateInline = false)
    {
        return ActionLinkStrId(label, controller, action, id?.ToString(), attributes, generateInline);
    }
    public static IGenerateHtml ActionLinkStrId(string label, string controller, string action, string? id, object? attributes = null, bool generateInline = false)
    {
        return ActionLinkStrId(label, controller, action, id, (QueryStringDict?)null, attributes, generateInline);
    }
        
    public static IGenerateHtml ActionLink(string label, string controller, string action, long? id, NameValueCollection queryString, object? attributes = null, bool generateInline = false)
    {
        return ActionLinkStrId(label, controller, action, id?.ToString(), queryString, attributes, generateInline);

    }
    public static IGenerateHtml ActionLinkStrId(string label, string controller, string action, string? id, NameValueCollection queryString, object? attributes = null, bool generateInline = false)
    {
        return ActionLinkStrId(label, controller, action, id, queryString.ToQueryStringDictionary(), attributes, generateInline);
    }
        
    public static IGenerateHtml ActionLink(string label, string controller, string action, long? id, QueryStringDict? queryStringDict = null, object? attributes = null, bool generateInline = false)
    {
        return ActionLinkStrId(label, controller, action, id?.ToString(), queryStringDict, attributes, generateInline);

    }
    public static IGenerateHtml ActionLinkStrId(string label, string controller, string action, string? id, QueryStringDict? queryStringDict = null, object? attributes = null, bool generateInline = false)
    {
        var labelTags = new Tags { new Txt(label) };
        return ActionLinkStrId(labelTags, controller, action, id, queryStringDict, attributes, generateInline);
    }
    #endregion

    #region Render ActionLink with Html Content Methods
    public static IGenerateHtml ActionLink<T>(IGenerateHtml label, Expression<Action<T>> action, object? attributes = null, bool generateInline = false) where T : MvcController
    {
        var (controllerName, actionName, id, updatedQueryStringDict) = Helper.GetControllerActionIdQs(action, null);
        return ActionLinkStrId(label, controllerName, actionName, id, updatedQueryStringDict, attributes, generateInline);
    }
    public static IGenerateHtml ActionLink<T>(IGenerateHtml label, Expression<Action<T>> action, NameValueCollection queryString, object? attributes = null, bool generateInline = false) where T : MvcController
    {
        return ActionLink(label, action, queryString.ToQueryStringDictionary(), attributes, generateInline);
    }
    public static IGenerateHtml ActionLink<T>(IGenerateHtml label, Expression<Action<T>> action, QueryStringDict? queryStringDict = null, object? attributes = null, bool generateInline = false) where T : MvcController
    {
        var (controllerName, actionName, id, updatedQueryStringDict) = Helper.GetControllerActionIdQs(action, queryStringDict);
        return ActionLinkStrId(label, controllerName, actionName, id, updatedQueryStringDict, attributes, generateInline);
    }

    public static IGenerateHtml ActionLink(IGenerateHtml label, string controller, string action, int? id, object? attributes = null, bool generateInline = false)
    {
        return ActionLinkStrId(label, controller, action, id?.ToString(), attributes, generateInline);
    }
    public static IGenerateHtml ActionLinkStrId(IGenerateHtml label, string controller, string action, string? id, object? attributes = null, bool generateInline = false)
    {
        return ActionLinkStrId(label, controller, action, id, (QueryStringDict?)null, attributes, generateInline);
    }
        
    public static IGenerateHtml ActionLink(IGenerateHtml label, string controller, string action, long? id, NameValueCollection queryString, object? attributes = null, bool generateInline = false)
    {
        return ActionLinkStrId(label, controller, action, id?.ToString(), queryString, attributes, generateInline);
    }
    public static IGenerateHtml ActionLinkStrId(IGenerateHtml label, string controller, string action, string? id, NameValueCollection queryString, object? attributes = null, bool generateInline = false)
    {
        return ActionLinkStrId(label, controller, action, id, queryString.ToQueryStringDictionary(), attributes, generateInline);
    }
        
    public static IGenerateHtml ActionLink(IGenerateHtml label, string controller, string action, long? id, QueryStringDict? queryStringDict = null, object? attributes = null, bool generateInline = false)
    {
        return ActionLinkStrId(label, controller, action, id?.ToString(), queryStringDict, attributes, generateInline);
    }
    public static IGenerateHtml ActionLinkStrId(IGenerateHtml label, string controller, string action, string? id, QueryStringDict? queryStringDict = null, object? attributes = null, bool generateInline = false)
    {
        var url = Helper.UrlForMvcAction(controller, action, id, queryStringDict);
        var aTag = new A(new { href = url }, generateInline) { label };
        aTag.AddOrUpdateAttr(attributes);
        return aTag;
    }
    #endregion

    #region Render RESTful ActionLink Methods
    public static IGenerateHtml RESTfulActionLink<T>(string label, HttpMethod httpMethod, Expression<Action<T>> action, object? attributes = null, string? confMsg = null, bool isButton = true) where T : MvcController
    {
        return RESTfulActionLink(label, httpMethod, action, (NameValueCollection?)null, attributes, confMsg, isButton);
    }
    public static IGenerateHtml RESTfulActionLink<T>(string label, HttpMethod httpMethod, Expression<Action<T>> action, NameValueCollection? queryString, object? attributes = null, string? confMsg = null, bool isButton = true) where T : MvcController
    {
        return RESTfulActionLink(label, httpMethod, action, queryString?.ToQueryStringDictionary(), attributes, confMsg, isButton);
    }
    public static IGenerateHtml RESTfulActionLink<T>(string label, HttpMethod httpMethod, Expression<Action<T>> action, QueryStringDict? queryStringDict = null, object? attributes = null, string? confMsg = null, bool isButton = true) where T : MvcController
    {
        var (controllerName, actionName, id, updatedQueryStringDict) = Helper.GetControllerActionIdQs(action, queryStringDict);
        return RESTfulActionLinkStrId(label, httpMethod, controllerName, actionName, id, updatedQueryStringDict, attributes, confMsg, isButton);
    }

    public static IGenerateHtml RESTfulActionLink(string label, HttpMethod httpMethod, string controller, string action, long? id, object? attributes = null, string? confMsg = null, bool isButton = true)
    {
        return RESTfulActionLinkStrId(label, httpMethod, controller, action, id?.ToString(), attributes);
    }
    public static IGenerateHtml RESTfulActionLinkStrId(string label, HttpMethod httpMethod, string controller, string action, string? id, object? attributes = null, string? confMsg = null, bool isButton = true)
    {
        return RESTfulActionLinkStrId(label, httpMethod, controller, action, id, (QueryStringDict?)null, attributes, confMsg, isButton);
    }
        
    public static IGenerateHtml RESTfulActionLink(string label, HttpMethod httpMethod, string controller, string action, long? id, NameValueCollection queryString, object? attributes = null, string? confMsg = null, bool isButton = true)
    {
        return RESTfulActionLinkStrId(label, httpMethod, controller, action, id?.ToString(), queryString, attributes, confMsg, isButton);
    }
    public static IGenerateHtml RESTfulActionLinkStrId(string label, HttpMethod httpMethod, string controller, string action, string? id, NameValueCollection queryString, object? attributes = null, string? confMsg = null, bool isButton = true)
    {
        return RESTfulActionLinkStrId(label, httpMethod, controller, action, id, queryString.ToQueryStringDictionary(), attributes, confMsg, isButton);
    }
        
    public static IGenerateHtml RESTfulActionLink(string label, HttpMethod httpMethod, string controller, string action, long? id, QueryStringDict? queryStringDict = null, object? attributes = null, string? confMsg = null, bool isButton = true)
    {
        return RESTfulActionLinkStrId(label, httpMethod, controller, action, id?.ToString(), queryStringDict, attributes);
    }
    public static IGenerateHtml RESTfulActionLinkStrId(string label, HttpMethod httpMethod, string controller, string action, string? id, QueryStringDict? queryStringDict = null, object? attributes = null, string? confMsg = null, bool isButton = true)
    {
        var url = Helper.UrlForMvcAction(controller, action, id, queryStringDict);
        return RESTfulActionLink(label, httpMethod, url, attributes, confMsg, isButton);
    }

    private static IGenerateHtml RESTfulActionLink(string label, HttpMethod httpMethod, string url, object? attributes, string? confMsg, bool isButton)
    {
        var labelTags = new Tags { new Txt(label) };
        return RESTfulActionLink(labelTags, httpMethod, url, attributes, confMsg, isButton);
    }
    #endregion

    #region Render RESTful ActionLink with Html Content Methods
    public static IGenerateHtml RESTfulActionLink<T>(IGenerateHtml label, HttpMethod httpMethod, Expression<Action<T>> action, object? attributes = null, string? confMsg = null, bool isButton = true) where T : MvcController
    {
        return RESTfulActionLink(label, httpMethod, action, (NameValueCollection?)null, attributes, confMsg, isButton);
    }
    public static IGenerateHtml RESTfulActionLink<T>(IGenerateHtml label, HttpMethod httpMethod, Expression<Action<T>> action, NameValueCollection? queryString, object? attributes = null, string? confMsg = null, bool isButton = true) where T : MvcController
    {
        return RESTfulActionLink(label, httpMethod, action, queryString?.ToQueryStringDictionary(), attributes, confMsg, isButton);
    }
    public static IGenerateHtml RESTfulActionLink<T>(IGenerateHtml label, HttpMethod httpMethod, Expression<Action<T>> action, QueryStringDict? queryStringDict = null, object? attributes = null, string? confMsg = null, bool isButton = true) where T : MvcController
    {
        var (controllerName, actionName, id, updatedQueryStringDict) = Helper.GetControllerActionIdQs(action, queryStringDict);
        return RESTfulActionLinkStrId(label, httpMethod, controllerName, actionName, id, updatedQueryStringDict, attributes, confMsg, isButton);
    }

    public static IGenerateHtml RESTfulActionLink(IGenerateHtml label, HttpMethod httpMethod, string controller, string action, long? id, object? attributes = null, string? confMsg = null, bool isButton = true)
    {
        return RESTfulActionLinkStrId(label, httpMethod, controller, action, id?.ToString(), attributes, confMsg, isButton);
    }
    public static IGenerateHtml RESTfulActionLinkStrId(IGenerateHtml label, HttpMethod httpMethod, string controller, string action, string? id, object? attributes = null, string? confMsg = null, bool isButton = true)
    {
        return RESTfulActionLinkStrId(label, httpMethod, controller, action, id, (NameValueCollection?)null, attributes, confMsg, isButton);
    }
        
    public static IGenerateHtml RESTfulActionLink(IGenerateHtml label, HttpMethod httpMethod, string controller, string action, long? id, NameValueCollection? queryString, object? attributes = null, string? confMsg = null, bool isButton = true)
    {
        return RESTfulActionLinkStrId(label, httpMethod, controller, action, id?.ToString(), queryString, attributes, confMsg, isButton);
    }
    public static IGenerateHtml RESTfulActionLinkStrId(IGenerateHtml label, HttpMethod httpMethod, string controller, string action, string? id, NameValueCollection? queryString, object? attributes = null, string? confMsg = null, bool isButton = true)
    {
        return RESTfulActionLinkStrId(label, httpMethod, controller, action, id, queryString?.ToQueryStringDictionary(), attributes, confMsg, isButton);
    }
        
    public static IGenerateHtml RESTfulActionLink(IGenerateHtml label, HttpMethod httpMethod, string controller, string action, long? id, QueryStringDict? queryStringDict = null, object? attributes = null, string? confMsg = null, bool isButton = true)
    {
        return RESTfulActionLinkStrId(label, httpMethod, controller, action, id?.ToString(), queryStringDict, attributes, confMsg, isButton);
    }
    public static IGenerateHtml RESTfulActionLinkStrId(IGenerateHtml label, HttpMethod httpMethod, string controller, string action, string? id, QueryStringDict? queryStringDict = null, object? attributes = null, string? confMsg = null, bool isButton = true)
    {
        var url = Helper.UrlForMvcAction(controller, action, id, queryStringDict);
        return RESTfulActionLink(label, httpMethod, url, attributes, confMsg, isButton);
    }

    private static IGenerateHtml RESTfulActionLink(IGenerateHtml label, HttpMethod httpMethod, string url, object? attributes, string? confMsg, bool isButton)
    {
        string httpMethodOverride;
        switch (httpMethod)
        {
            case HttpMethod.Delete:
                httpMethodOverride = "Delete";
                break;
            case HttpMethod.Head:
                httpMethodOverride = "Head";
                break;
            case HttpMethod.Put:
                httpMethodOverride = "Put";
                break;
            case HttpMethod.Post:
                httpMethodOverride = "Post";
                break;
            default:
                throw new SupermodelException("Unsupported HttpVerb in ActionLinkFormContent");
        }

        Tag tag;
        if (isButton) tag = new Button(new { type="button" }) { label };
        else tag = new A(new { href = "#" }) { label };

        if (confMsg != null) tag.AddOrUpdateAttr(new { onclick = $"supermodel_restfulLinkToUrlWithConfirmation('{url}', '{httpMethodOverride}', '{confMsg}')"});
        else tag.AddOrUpdateAttr(new { onclick = $"supermodel_restfulLinkToUrlWithConfirmation('{url}', '{httpMethodOverride}')"} );
            
        tag.AddOrUpdateAttr(attributes);

        return tag;
    }
    #endregion
}