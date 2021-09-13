using NetCoreDAL;
using NetCoreDAL.Models;
using System.Threading.Tasks;

namespace NetCoreBLL.Interface
{
    public interface IAccount
    {
        Task<Response> SignIn(SignIn signIn);
        Task<Response> SignUp(SignUp signUp);
    }
}
