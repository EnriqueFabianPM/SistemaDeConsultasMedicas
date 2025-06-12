using WebServices.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Net.Mail;
using System.Net;

public interface IViewRenderService
{
    Task<string> RenderToStringAsync(string viewName, Email model);
    void SendEmail(Email data);
}

public class EmailServices : IViewRenderService
{
    private readonly ICompositeViewEngine _viewEngine;
    private readonly ITempDataProvider _tempDataProvider;
    private readonly IServiceProvider _serviceProvider;

    public EmailServices(
        ICompositeViewEngine viewEngine,
        ITempDataProvider tempDataProvider,
        IServiceProvider serviceProvider)
    {
        _viewEngine = viewEngine;
        _tempDataProvider = tempDataProvider;
        _serviceProvider = serviceProvider;
    }

    public async Task<string> RenderToStringAsync(string viewName, Email model)
    {
        var httpContext = new DefaultHttpContext { RequestServices = _serviceProvider };
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

        using var sw = new StringWriter();
        var viewResult = _viewEngine.FindView(actionContext, viewName, false);

        if (viewResult.View == null)
        {
            throw new ArgumentNullException($"{viewName} no fue encontrado.");
        }

        var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
        {
            Model = model
        };
        var tempData = new TempDataDictionary(actionContext.HttpContext, _tempDataProvider);

        var viewContext = new ViewContext(
            actionContext,
            viewResult.View,
            viewData,
            tempData,
            sw,
            new HtmlHelperOptions()
        );

        await viewResult.View.RenderAsync(viewContext);
        return sw.ToString();
    }

    //Manejo de servicios de correos-----------------------------------------------------------------------------------------------------------
    public void SendEmail(Email data)
    {
        if (data != null)
        {
            if(data.user != null)
            {
                // Configurar el cliente SMTP
                SmtpClient clienteSmtp = new SmtpClient("smtp.gmail.com");
                clienteSmtp.Port = 587;
                clienteSmtp.Credentials = new NetworkCredential("perezmedellinenriquefabian@gmail.com", "inwqkdvoubvdugcv"); // Contraseña de aplicación Fabian
                clienteSmtp.EnableSsl = true;

                // Crear y enviar el correo
                MailMessage email = new MailMessage();
                email.From = new MailAddress("perezmedellinenriquefabian@gmail.com");
                email.Subject = data.subject;
                email.Body = data.body;
                email.IsBodyHtml = true;
                //email.To.Add(data.user.Email);
                email.To.Add("21017@virtual.utsc.edu.mx");

                //Mandar el correo
                clienteSmtp.Send(email);
            }
        }
    }
}