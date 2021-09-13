using NetCoreDAL;
using System.Threading.Tasks;

namespace NetCoreBLL.Interface
{
    public interface IHome
    {
        Task<Response> GetUsers();
    }
}
