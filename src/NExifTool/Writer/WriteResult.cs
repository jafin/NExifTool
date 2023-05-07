using System.IO;


namespace NExifTool.Writer
{
    public class WriteResult
    {
        public bool Success { get; private set; }
        public string ErrorMessage { get; set; }
        public Stream Output { get; private set; }


        public WriteResult(bool success, Stream output, string errorMessage = null)
        {
            Success = success;
            ErrorMessage = errorMessage;
            Output = output;
        }
    }
}
