using System;
using System.IO;
using System.Net;
using System.ServiceModel;
using EliseAPIService;

namespace api_elise.Helper
{
    public class ApiClient
    {
        private readonly EliseWebServiceClient client;
        private readonly Session session;

        public ApiClient()
        {
            // Initialize the client
            client = new EliseWebServiceClient(EliseWebServiceClient.EndpointConfiguration.BasicHttpBinding_EliseWebService);

            // Create and populate the Session object
            session = new Session
            {
                // Confidential connection parameters
                ApplicationID = "",
                ApplicationKey = "",
                EliseVersionRequired = "",
                Instance = "",
                Language = "",
                UserLogin = "",
            };
        }

        public async Task<EliseAPIService.ResponseCompleteEliseMailResponse> GetCompleteEliseMail(string[] mailIdList)
        {
            var config = new ResponseMailConfig();
            EliseAPIService.ResponseCompleteEliseMailResponse response = null;

            try
            {
                response = await client.GetCompleteEliseMailAsync(session, mailIdList, config);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine("An error occurred: " + ex.Message);
            }
            finally
            {
                if (client.State == CommunicationState.Faulted)
                {
                    client.Abort();
                }
                else
                {
                    await client.CloseAsync();
                }
            }

            return response;
        }


    }
}
