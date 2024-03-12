using E_commerce.Domian;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Domian
{
    public class ResponseModel<T> : ResponseModel
    {
        public new T Data { get; set; }

        public ResponseModel(bool isSuccess = false, T data = default, object errorMessage = null)
        {
            this.IsSuccess = isSuccess;
            if (errorMessage == null)
                this.ErrorMessege = isSuccess ? "Success" : "Error";
            else
                this.ErrorMessege = errorMessage;
            this.Data = data;
        }

        public static ResponseModel<T> Success(T data) => new(isSuccess: true, data: data, errorMessage: null);
    }
}
