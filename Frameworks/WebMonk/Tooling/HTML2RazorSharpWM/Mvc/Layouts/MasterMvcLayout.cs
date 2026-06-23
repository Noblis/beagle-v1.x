using WebMonk.RazorSharp.HtmlTags;
using WebMonk.RazorSharp.HtmlTags.BaseTags;
using WebMonk.Rendering.Views;

namespace HTML2RazorSharpWM.Mvc.Layouts;

public class MasterMvcLayout : IMvcLayout
{
    #region Methods
    public IGenerateHtml RenderDefaultLayout()
    {
        var html = new Html
        {
            new Head
            {
                new Title { new Txt("Web Monk Test Project") },
                new Link(new { rel="stylesheet", href="https://stackpath.bootstrapcdn.com/bootstrap/4.4.1/css/bootstrap.min.css", integrity="sha384-Vkoo8x4CGsO3+Hhxv8T/Q5PaXtkKtu6ug5TOeNV6gBiFeWPGFN9MuhOf23Q9Ifjh", crossorigin="anonymous" }),
                new Link(new { rel="stylesheet", href="/css/site.css" }),
                new Link(new { type="image/x-icon", rel="shortcut icon", href="/images/favicon.ico" }),
            },
            new Body
            {
                new Script(new { src="https://code.jquery.com/jquery-3.5.1.min.js", integrity="sha256-9/aliU8dGd2tb6OSsuzixeV4y/faTqgFtohetphbbj0=", crossorigin="anonymous" }),
                new Script(new { src="https://cdn.jsdelivr.net/npm/popper.js@1.16.0/dist/umd/popper.min.js", integrity="sha384-Q6E9RHvbIyZFJoft+2mJbHaEWldlvI9IOYy5n3zV9zzTtmI3UksdQRVvoxMfooAo", crossorigin="anonymous" }),
                new Script(new { src="https://stackpath.bootstrapcdn.com/bootstrap/4.4.1/js/bootstrap.min.js", integrity="sha384-wfSDF2E50Y2D1uUdj0O3uMBJnjuUD4Ih7YwaYd1iqfktj0Uod8GCExl3Og8ifwB6", crossorigin="anonymous" }),
                new Script(new { type="text/javascript", src="/js/site.js" }),
                new Script(new { type="text/javascript", src="/js/jquery-linenumbers.js" }),

                new Div(new { id="body", style="margin: 20px !important;" })
                {
                    new BodySectionPlaceholder()
                }
            }
        };

        return html;
    }
    #endregion
}