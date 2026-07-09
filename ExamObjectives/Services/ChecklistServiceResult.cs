namespace ExamObjectives.Services
{
    public class ChecklistServiceResult
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; } = StatusCodes.Status200OK;
        public string Message { get; set; } = "";

        public static ChecklistServiceResult Ok(string message)
        {
            return new ChecklistServiceResult
            {
                Success = true,
                StatusCode = StatusCodes.Status200OK,
                Message = message
            };
        }

        public static ChecklistServiceResult BadRequest(string message)
        {
            return new ChecklistServiceResult
            {
                Success = false,
                StatusCode = StatusCodes.Status400BadRequest,
                Message = message
            };
        }

        public static ChecklistServiceResult NotFound(string message)
        {
            return new ChecklistServiceResult
            {
                Success = false,
                StatusCode = StatusCodes.Status404NotFound,
                Message = message
            };
        }
    }
}