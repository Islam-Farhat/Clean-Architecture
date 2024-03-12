using E_commerce.Domian;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application
{
    internal interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, ResponseModel<TResponse>>
        where TQuery : IQuery<TResponse>
    {
    }
}
