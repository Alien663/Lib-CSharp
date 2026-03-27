using MimeKit;

namespace Alien.Common.Mail.Models;

public class MailDto
{
    public required string Sender { get; set; }
    public required List<string> To { get; set; }
    public List<string> CC { get; set; } = new List<string>();
    public List<string> BCC { get; set; } = new List<string>();
    public required string Subject { get; set; }
    public string Body { get; set; } = "";

    private MimeMessage? _message;
    
    public MimeMessage message
    {
        get
        {
            if (_message == null)
            {
                _message = new MimeMessage();
            }
            
            _message.Subject = this.Subject;
            _message.Sender = new MailboxAddress(Sender, Sender);
            
            // 清除現有的收件人，避免重複添加
            _message.To.Clear();
            _message.Cc.Clear();
            _message.Bcc.Clear();
            
            foreach (var item in To) _message.To.Add(new MailboxAddress(item, item));
            foreach (var item in CC) _message.Cc.Add(new MailboxAddress(item, item));
            foreach (var item in BCC) _message.Bcc.Add(new MailboxAddress(item, item));
            
            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = this.Body;
            
            _message.Body = bodyBuilder.ToMessageBody();
            return _message;
        }
        private set { _message = value; }
    }

    public void setPicture(string ID, string FilePath, string Mime)
    {
        if (!File.Exists(FilePath))
        {
            throw new ArgumentException("File not found", nameof(FilePath));
        }
        var bodyBuilder = new BodyBuilder();
        bodyBuilder.HtmlBody = this.Body;

        if(!bodyBuilder.HtmlBody.Contains($"cid:{ID}"))
        {
            throw new ArgumentException("ID not found in body", nameof(ID));
        }

        bodyBuilder.LinkedResources.Add(new MimePart(Mime)
        {
            ContentId = ID,
            ContentTransferEncoding = ContentEncoding.Base64,
            FileName = Path.GetFileName(FilePath),
            ContentDisposition = new ContentDisposition(ContentDisposition.Inline)
            {
                IsAttachment = false
            }
        });
        
        // 更新 Body 而不是直接操作 message
        this.Body = bodyBuilder.HtmlBody;
    }
    public void setPicture(MailPictureModel picture)
    {
        if (!File.Exists(picture.FilePath))
        {
            throw new ArgumentException("File not found", nameof(picture.FilePath));
        }
        var bodyBuilder = new BodyBuilder();
        bodyBuilder.HtmlBody = this.Body;
        if (!bodyBuilder.HtmlBody.Contains($"cid:{picture.ID}"))
        {
            throw new ArgumentException("ID not found in body", nameof(picture.ID));
        }
        bodyBuilder.LinkedResources.Add(new MimePart(picture.Mime)
        {
            ContentId = picture.ID,
            ContentTransferEncoding = ContentEncoding.Base64,
            FileName = Path.GetFileName(picture.FilePath),
            ContentDisposition = new ContentDisposition(ContentDisposition.Inline)
            {
                IsAttachment = false
            }
        });
        
        // 更新 Body 而不是直接操作 message
        this.Body = bodyBuilder.HtmlBody;
    }
    public void setPicture(List<MailPictureModel> pictures)
    {
        foreach (MailPictureModel picture in pictures)
        {
            setPicture(picture);
        }
    }

    public void setAttachment(string FilePath)
    {
        if (!File.Exists(FilePath))
        {
            throw new ArgumentException("File not found", nameof(FilePath));
        }
        var bodyBuilder = new BodyBuilder();
        bodyBuilder.HtmlBody = this.Body;
        bodyBuilder.Attachments.Add(FilePath);
        this.Body = bodyBuilder.HtmlBody;
    }
}
