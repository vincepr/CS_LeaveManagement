namespace HR.LeaveManagement.MVC.Services.Base
{
    // wraps around all requests we expect indicating success/error messages
    public class Response<T>
    {
        public string Message { get; set; }
        public string ValidationErrors { get; set; }
        public bool Success { get; set; }
        public T Data { get; set; }
    }
}