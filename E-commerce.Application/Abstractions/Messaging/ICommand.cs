using E_commerce.Domian;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application
{
    public interface ICommand : IRequest<ResponseModel>
    {
    }
    //public interface ICommand<TResponse> : IRequest<ResponseModel<TResponse>>
    //{
    //}
}
