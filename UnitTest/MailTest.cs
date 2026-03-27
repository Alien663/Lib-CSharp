using NUnit.Framework;
using System.Collections.Generic;
using Alien.Common.Mail;
using Alien.Common.Mail.Models;
using System;

namespace TestMyLib;

[TestFixture]
public class MailTest
{
    private string htmlBody = @"Hi Leon,<br>Help me to do the unit test of mail module<br><img src='cid:logo1'></img><br><img src='cid:logo2'></img><br>Test Send Mail<br>";
    [OneTimeSetUp]
    [Test, Order(1)]
    public void CallMailSuccess()
    {
        #region Arrange
        var fakeClient = new FakeSmtpClientWrapper();
        var config = new MailConfigDto { SMTPServer = "smtp.demo.com" };
        var mail = new MailDto
        {
            Sender = "test@demo.com",
            To = new List<string> { "test@demo.com" },
            CC = new List<string> { "test@demo.com" },
            BCC = new List<string> { "test@demo.com" },
            Subject = "Test",
            Body = htmlBody,
        };
        #endregion

        #region Act
        mail.setPicture(ID: "logo1", FilePath: @".\icon.png", Mime: "img/png");
        mail.setPicture(new List<MailPictureModel>
        {
            new MailPictureModel
            {
                ID = "logo2",
                FilePath = @".\icon.png"
            }
        });
        mail.setAttachment(@".\icon.png");
        fakeClient.Send(mail);
        #endregion

        #region Assert
        Assert.That(fakeClient.Sent, Is.True);
        #endregion
    }

    [Test, Order(2)]
    public void CallMailFail_WrongPictureSetting()
    {
        #region Arrange
        var fakeClient = new FakeSmtpClientWrapper();
        var config = new MailConfigDto { SMTPServer = "smtp.demo.com" };
        var mail = new MailDto
        {
            Sender = "test@demo.com",
            To = new List<string> { "test@demo.com" },
            Subject = "Test",
        };
        #endregion

        #region Act
        #endregion

        #region Assert
        Assert.Throws<ArgumentException>(() => mail.setPicture(ID: "WrongID", FilePath: @".\icon.png", Mime: "img/png"), "File not found");
        Assert.Throws<ArgumentException>(() => mail.setPicture(new List<MailPictureModel>
        {
            new MailPictureModel
            {
                ID = "logo2",
                FilePath = @".\WrongPath.png"
            }
        }));
        #endregion
    }

    [Test, Order(3)]
    public void CallMailFail_WrongAttachement()
    {
        #region Arrange
        var fakeClient = new FakeSmtpClientWrapper();
        var config = new MailConfigDto { SMTPServer = "smtp.demo.com" };
        var mail = new MailDto
        {
            Sender = "test@demo.com",
            To = new List<string> { "test@demo.com" },
            Subject = "Test",
        };
        #endregion

        #region Act
        #endregion

        #region Assert
        Assert.Throws<ArgumentException>(() => mail.setAttachment(@".\WrongPath.png"), "File not found");
        #endregion
    }

    public class FakeSmtpClientWrapper : ISmtpClientWrapper
    {
        public bool Sent = false;
        public void Send(MailDto dto) => Sent = true;
        public void Send(IEnumerable<MailDto> mails) => Sent = true;
    }
}
