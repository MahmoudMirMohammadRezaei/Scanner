﻿using Newtonsoft.Json.Linq;

namespace NAPS2.ImportExport.Email.Oauth;

public class OutlookWebEmailProvider : IEmailProvider
{
    private readonly OutlookWebOauthProvider _outlookWebOauthProvider;

    public OutlookWebEmailProvider(OutlookWebOauthProvider outlookWebOauthProvider)
    {
        _outlookWebOauthProvider = outlookWebOauthProvider;
    }

    public async Task<bool> SendEmail(EmailMessage emailMessage, ProgressHandler progress = default)
    {
        var messageObj = new JObject
        {
            { "Subject", emailMessage.Subject },
            { "Body", new JObject
            {
                { "ContentType", "Text" },
                { "Content", emailMessage.BodyText }
            }},
            { "ToRecipients", Recips(emailMessage, EmailRecipientType.To) },
            { "CcRecipients", Recips(emailMessage, EmailRecipientType.Cc) },
            { "BccRecipients", Recips(emailMessage, EmailRecipientType.Bcc) },
            { "Attachments", new JArray(emailMessage.Attachments.Select(attachment => new JObject
            {
                { "@odata.type", "#Microsoft.OutlookServices.FileAttachment" },
                { "Name", attachment.AttachmentName },
                { "ContentBytes", Convert.ToBase64String(File.ReadAllBytes(attachment.FilePath)) }
            }))}
        };
        var respUrl = await _outlookWebOauthProvider.UploadDraft(messageObj.ToString(), progress);

        // Open the draft in the user's browser
        ProcessHelper.OpenUrl(respUrl + "&ispopout=0");

        return true;
    }

    private JToken Recips(EmailMessage message, EmailRecipientType type)
    {
        return new JArray(message.Recipients.Where(recip => recip.Type == type).Select(recip => new JObject
        {
            { "EmailAddress", new JObject
            {
                { "Address", recip.Address },
                { "Name", recip.Name }
            }}
        }));
    }
}