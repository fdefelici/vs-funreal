using System.Collections.Generic;

namespace FUnreal
{
    public class FUnrealGenericResult
    {
        public static FUnrealGenericResult Failure(string message)
        {
            var r = new FUnrealGenericResult();
            r.Messages.Add(message);
            r.IsSuccess = false;
            return r;
        }

        public static FUnrealGenericResult Success()
        {
            var r = new FUnrealGenericResult();
            r.IsSuccess = true;
            return r;
        }

        public bool IsSuccess { get; private set; }

        public bool IsFailure { get => !IsSuccess; }
        public List<string> Messages { get; private set; }

        public FUnrealGenericResult()
        {
            IsSuccess = false;
            Messages = new List<string>();
        }

        public static FUnrealGenericResult operator +(FUnrealGenericResult a, FUnrealGenericResult b)
        {
            var r = new FUnrealGenericResult();
            r.IsSuccess = a.IsSuccess & b.IsSuccess;
            r.Messages.AddRange(a.Messages);
            r.Messages.AddRange(b.Messages);
            return r;
        }
    }

}
