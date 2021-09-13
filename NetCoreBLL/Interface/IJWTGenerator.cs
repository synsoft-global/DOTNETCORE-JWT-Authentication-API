using System;
using NetCoreDAL.Models;
using System.Threading.Tasks;

namespace NetCoreBLL.Interface
{
    public interface IJWTGenerator
    {
        Task<Object> GenerateJwtToken(User user);
    }
}
