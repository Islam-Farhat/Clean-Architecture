namespace E_commerce.Domian
{
    public class ResponseModel
    {
        public bool IsSuccess { get; set; }
        public object? ErrorMessege { get; set; }
        public object Data { get; set; }
        public ResponseModel(bool isSuccess = false, object data = default, object errorMessage = null)
        {
            this.IsSuccess = isSuccess;
            if (errorMessage == null)
                this.ErrorMessege = isSuccess ? "Success" : "Error";
            else
                this.ErrorMessege = errorMessage;
            this.Data = data;
        }

        public static ResponseModel Success() => new(isSuccess: true, errorMessage: null);

        public static ResponseModel Failure(object error) => new(false, errorMessage: error);
    }
}
