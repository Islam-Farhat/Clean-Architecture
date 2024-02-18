using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application
{
    public class ResponseModel<T>
    {
        public bool IsSuccess { get; set; }
        public object? ErrorMessege { get; set; }
        public T Data { get; set; }
        public ResponseModel(bool isSuccess = false, T data = default, object errorMessage = null)
        {
            this.IsSuccess = isSuccess;
            if (errorMessage == null)
                this.ErrorMessege = isSuccess ? "Success" : "Error";
            else
                this.ErrorMessege = errorMessage;
            this.Data = data;
        }
    }
}
