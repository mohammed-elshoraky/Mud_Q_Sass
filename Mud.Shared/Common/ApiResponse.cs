using System.Text.Json.Serialization;

namespace Mud.Shared.Common
{
    public class ApiResponse<T>
    {
        [JsonPropertyOrder(1)] public bool Success { get; set; }
        [JsonPropertyOrder(2)] public string Message { get; set; } = string.Empty;
        [JsonPropertyOrder(3)] public T? Data { get; set; }
        [JsonPropertyOrder(4)] public List<string>? Errors { get; set; }
        [JsonPropertyOrder(5)] public int StatusCode { get; set; }
        [JsonPropertyOrder(6)] public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public static class ApiResponse
    {
        public static ApiResponse<T> Success<T>(T data, string message = "تمت العملية بنجاح", int statusCode = 200)
            => new() { Success = true, Message = message, Data = data, StatusCode = statusCode };

        public static ApiResponse<T> Error<T>(string message, List<string>? errors = null, int statusCode = 400)
            => new() { Success = false, Message = message, Errors = errors ?? new List<string> { message }, StatusCode = statusCode };

        public static ApiResponse<T> NotFound<T>(string message = "الموارد غير موجودة") => Error<T>(message, null, 404);
        public static ApiResponse<T> Unauthorized<T>(string message = "غير مصرح لك") => Error<T>(message, null, 401);
        public static ApiResponse<T> Forbidden<T>(string message = "ممنوع") => Error<T>(message, null, 403);
        public static ApiResponse<T> ServerError<T>(string message = "حدث خطأ داخلي") => Error<T>(message, null, 500);

        public static ApiResponse<T> ValidationError<T>(Dictionary<string, string[]> validationErrors)
        {
            var errors = validationErrors
                .SelectMany(kvp => kvp.Value.Select(v => $"{kvp.Key}: {v}"))
                .ToList();
            return Error<T>("فشل التحقق من صحة البيانات", errors, 400);
        }
    }
}