using E_commerce.Domian;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application
{
    public interface ICommandHandler<TCommand> : IRequestHandler<TCommand, ResponseModel>
        where TCommand : ICommand
    {
    }

    public interface ICommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, ResponseModel<TResponse>>
       where TCommand : ICommand<TResponse>
    {
    }
}
