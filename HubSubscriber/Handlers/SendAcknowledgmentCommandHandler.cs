namespace HubSubscriber.Handlers
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;

    using Managers;

    using Messages.Commands;

    using Services;

    using NServiceBus;
    public class SendAcknowledgmentCommandHandler : IHandleMessages<SendAcknowledgementCommand>
    {
        private readonly IHubConnectionManager hubConnectionManager;

        private readonly IAccessTokenService accessTokenService;

        public SendAcknowledgmentCommandHandler(
            IHubConnectionManager hubConnectionManager, 
            IAccessTokenService accessTokenService)
        {
            this.hubConnectionManager = hubConnectionManager;
            this.accessTokenService = accessTokenService;
        }

        public void Handle(SendAcknowledgementCommand sendAcknowledgementCommand)
        {
            //1 send acknowledgement back via web api
            //In production this would have a retry policy
            //the error itself will trigger an nsb retry
            if (!PostAcknowledgment(sendAcknowledgementCommand))
            {
                throw new ApplicationException("Unable to return acknowledgment to the data centre");
            }

            //2 refresh the access token for the SignalrConnection if needed
            if (hubConnectionManager.AccessTokenNeedsRefresh())
            {
                hubConnectionManager.Stop();
                hubConnectionManager.Start();
            }
        }

        private bool PostAcknowledgment(SendAcknowledgementCommand sendAcknowledgementCommand)
        {
            var token = accessTokenService.GetToken();
            var accessToken = accessTokenService.ExtractAccessToken(token);

            //add a retry policy here
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
                var auditApiAcknowledgementControllerUrl = "http://localhost:8094/api/Acknowledgement";
                using (
                    var httpResponse =
                        httpClient.PostAsJsonAsync(new Uri(auditApiAcknowledgementControllerUrl), sendAcknowledgementCommand)
                            .Result)
                {
                    return httpResponse.IsSuccessStatusCode;
                }
            }
        }
    }
}