using System.Net.Http.Headers;
using HR.LeaveManagement.MVC.Contracts;

namespace HR.LeaveManagement.MVC.Services.Base
{
    /// <summary>
    ///  some base functionality all our "Endpoint-Services" should have
    /// </summary>
    public class BaseHttpService
    {
        protected readonly ILocalStorageService _localStorage;
        
        protected IClient _client;

        public BaseHttpService(IClient client, ILocalStorageService localStorage)
        {
            _client = client;
            _localStorage = localStorage;
        }

        /// <summary>
        /// base error handling
        /// </summary>
        protected Response<Guid> ConvertApiExceptions<Guid>(ApiException ex)
        {
            return ex.StatusCode switch
            {
                400 => new Response<Guid>() { Message = "Validation errors have occured.", ValidationErrors = ex.Response, Success = false },
                404 => new Response<Guid>() { Message = "The requested item could not be found.", Success = false },
                _ => new Response<Guid>() { Message = "Something went wrong, please try again.", Success = false }
            };
        }

        // read the jwt from localstorage
        protected void AddBearerToken()
        {
            if (_localStorage.Exists("token"))
                _client.HttpClient.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", _localStorage.GetStorageValue<string>("token"));
        }
    }
}