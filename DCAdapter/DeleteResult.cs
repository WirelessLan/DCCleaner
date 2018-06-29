namespace DCAdapter
{
    public class DeleteResult
    {
        public bool Success { get; } = false;

        public string ErrorMessage { get; } = "";

        public DeleteResult(bool success, string msg)
        {
            Success = success;
            ErrorMessage = msg;
        }
    }
}
