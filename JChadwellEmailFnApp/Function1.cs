using Azure;
using Azure.Communication.Email;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace JChadwellEmailFnApp;

public class Function1
{
    private readonly ILogger<Function1> _logger;

    public Function1(ILogger<Function1> logger)
    {
        _logger = logger;
    }

    [Function("Function1")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        return new OkObjectResult("Hello Again 2 Welcome to Azure Functions!");
    }

    [Function("SendEmail")]
    //public static async Task<IActionResult> RunSendEmail([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req, ILogger log)
    public async Task<IActionResult> RunSendEmail([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req, ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request.");

        // fisrt check request url query parameters
        string sender = req.Query["sender"];
        _logger.LogInformation("sender: " + sender);
        string recipient = req.Query["recipient"];
        _logger.LogInformation("recipient: " + recipient);
        string subject = req.Query["subject"];
        _logger.LogInformation("subject: " + subject);
        string htmlContent = req.Query["htmlContent"];
        _logger.LogInformation("htmlContent: " + htmlContent);
        string textContent = req.Query["textContent"];
        _logger.LogInformation("textContent: " + textContent);


        // parse request body to EmailBody class and check for any missing parameters in request body
        var content = await new StreamReader(req.Body).ReadToEndAsync();
        EmailClientBody body = JsonConvert.DeserializeObject<EmailClientBody>(content);

        //_logger.LogInformation("name: " + emailmsg.name);
        //_logger.LogInformation("email: " + emailmsg.email);
        //_logger.LogInformation("message: " + emailmsg.message);

        sender = body.sender ?? sender;
        _logger.LogInformation("sender: " + sender);
        recipient = body.recipient ?? recipient;
        _logger.LogInformation("recipient: " + recipient);
        subject = body.subject ?? subject;
        _logger.LogInformation("subject: " + subject);
        htmlContent = body.htmlContent ?? htmlContent;
        _logger.LogInformation("htmlContent: " + htmlContent);
        textContent = body.textContent ?? textContent;
        _logger.LogInformation("textContent: " + textContent);

        // need validation added

        //var senderEmailAddress = Environment.GetEnvironmentVariable("JChadwellArtAdminEmailAddress");
        //_logger.LogInformation("senderEmailAddress: " + senderEmailAddress);

        var emailClient = new EmailClient(Environment.GetEnvironmentVariable("AzureCommunicationServicesConnectionString"));
        _logger.LogInformation("emailClient: " + emailClient);
        try
        {
            //Send Email
            var selfEmailSendOperation = await emailClient.SendAsync(
                wait: WaitUntil.Completed,
                senderAddress: sender,
                recipientAddress: recipient,
                subject: subject,
                //subject: $"New message in the website from {name} ({email})",
                htmlContent: htmlContent,
                plainTextContent: textContent); // Use the received HTML content
                                                //htmlContent: "<html><body>" + name + " with email address " + email + " sent the following message: <br />" + message + "</body></html>");
            _logger.LogInformation($"Email sent with message ID: {selfEmailSendOperation.Id} and status: {selfEmailSendOperation.Value.Status}");

            return new OkObjectResult($"Emails sent.");
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError($"Sending email operation failed with error code: {ex.ErrorCode}, message: {ex.Message}");
            return new ConflictObjectResult("Error sending email");
        }
        //return new OkObjectResult("Welcome to Azure Functions!");
    }


    [Function("JChadwellDevContactMeSendEmail")]
    public async Task<IActionResult> RunJChadwellDevContactMeSendEmail([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req)
    {
        _logger.LogInformation("JChadwellDevContactMeSendEmail Function Triggered.");
        string name = req.Query["name"];
        _logger.LogInformation("name: " + name);
        string email = req.Query["email"];
        _logger.LogInformation("email: " + email);
        string message = req.Query["message"];
        _logger.LogInformation("message: " + message);
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic data = JsonConvert.DeserializeObject(requestBody);

        name = name ?? data?.name;
        _logger.LogInformation("name: " + name);
        email = email ?? data?.email;
        _logger.LogInformation("email: " + email);
        message = message ?? data?.message;
        _logger.LogInformation("message: " + message);
        var myEmailAddress = Environment.GetEnvironmentVariable("myEmailAddress");
        _logger.LogInformation("myEmailAddress: " + myEmailAddress);
        var senderEmailAddress = Environment.GetEnvironmentVariable("senderEmailAddress");
        _logger.LogInformation("senderEmailAddress: " + senderEmailAddress);
        var emailClient = new EmailClient(Environment.GetEnvironmentVariable("AzureCommunicationServicesConnectionString"));
        _logger.LogInformation("emailClient: " + emailClient);
        try
        {
            //Email to notify myself
            var selfEmailSendOperation = await emailClient.SendAsync(
                wait: WaitUntil.Completed,
                senderAddress: senderEmailAddress,
                recipientAddress: myEmailAddress,
                subject: $"New message in the website from {name} ({email})",
                htmlContent: "<html><body>" + name + " with email address " + email + " sent the following message: <br />" + message + "</body></html>");
            _logger.LogInformation($"Email sent with message ID: {selfEmailSendOperation.Id} and status: {selfEmailSendOperation.Value.Status}");
            //Email to notify the contact
            var contactEmailSendOperation = await emailClient.SendAsync(
                wait: WaitUntil.Completed,
                senderAddress: senderEmailAddress,
                recipientAddress: email,
                subject: $"Email sent. Thank you for reaching out.",
                htmlContent: "Hello " + name + " thank you for your message. Will try to get back you as soon as possible.");
            _logger.LogInformation($"Email sent with message ID: {contactEmailSendOperation.Id} and status: {contactEmailSendOperation.Value.Status}");
            return new OkObjectResult($"Emails sent.");
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError($"Sending email operation failed with error code: {ex.ErrorCode}, message: {ex.Message}");
            return new ConflictObjectResult("Error sending email");
        }
    }




}