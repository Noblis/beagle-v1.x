using System.Collections.Generic;
using System.Threading.Tasks;
using WebMonk.Context.WMHttpListenerObjects;
using WebMonk.ValueProviders;

namespace WebMonk.Context;

public class ValueProviderManager : IValueProviderManager
{
    #region Constructors
    public ValueProviderManager(IHttpListenerContext httpListenerContext)
    {
        HttpListenerContext = httpListenerContext;
    }
    #endregion
        
    #region Methods
    public virtual async Task<List<IValueProvider>> GetValueProvidersListAsync()
    { 
        _valueProviders ??=
        [
            await new MessageBodyValueProvider().InitAsync(HttpListenerContext.Request).ConfigureAwait(false),
            await new QueryStringValueProvider().InitAsync(HttpListenerContext.Request).ConfigureAwait(false)
        ];
        return _valueProviders;
    }

    public virtual List<IValueProvider>? GetCachedValueProvidersList()
    {
        return _valueProviders;
    }
    protected List<IValueProvider>? _valueProviders;
    #endregion

    #region Properties
    public IHttpListenerContext HttpListenerContext { get; }
    #endregion
}