namespace AZUCENA_11262026.Exceptions
{
    public class FileSizeLimitExceededException : Exception
    {
        public FileSizeLimitExceededException(string message)
            : base(message)
        {
        }
    }
}
