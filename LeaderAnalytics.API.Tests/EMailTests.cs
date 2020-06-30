using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using LeaderAnalytics.API.Domain;
using System.Net;

namespace LeaderAnalytics.API.Tests
{
    public class EMailTests : BaseTest
    {
        [Test]
        public async Task can_send_email()
        {
            EmailMsg msg = new EmailMsg { To = "leaderanalytics@outlook.com", Msg = $"can_send_email test @  {DateTime.UtcNow.ToLongDateString()}"};
            HttpResponseMessage response = await apiClient.PostAsync("api/Message/SendEmail", new StringContent(JsonSerializer.Serialize(msg), Encoding.UTF8, "application/json"));
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        }
    }
}
