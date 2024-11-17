namespace DigitalSeal.Core.Exceptions
{
    public class ValidationException : Exception
    {
        protected ValidationException(string? message, IEnumerable<string>? errors = null,
            ExceptionLevel level = ExceptionLevel.Error)
            : base(message)
        {
            Level = level;
            Errors = errors?.ToList();
        }


        public ValidationException(string message)
            : base(message)
        {
            Level = ExceptionLevel.Error;
            Errors = [message];
        }


        public List<string>? Errors { get; set; }
        public ExceptionLevel Level { get; set; }


        public static ValidationException Error(string error) 
            => new(string.Empty, [error], ExceptionLevel.Error);

        public static ValidationException Error(IEnumerable<string> errors)
            => new(string.Empty, errors, ExceptionLevel.Error);

        public static ValidationException Error(string message, IEnumerable<string> errors) 
            => new(message, errors, ExceptionLevel.Error);

        public static ValidationException Warning(string error) 
            => new(string.Empty, [error], ExceptionLevel.Warning);

        public static ValidationException Warning(string message, IEnumerable<string> errors) 
            => new(message, errors, ExceptionLevel.Warning);

        public enum ExceptionLevel { Error, Warning }
    }
}
