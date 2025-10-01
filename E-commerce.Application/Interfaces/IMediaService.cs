using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.Interfaces
{
    public interface IMediaService
    {
        Task<Result<string>> UploadImage(string imageBase64, string path);
    }
}
