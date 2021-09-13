using System;
using System.Linq;
using System.Data;
using NetCoreBLL.Interface;
using NetCoreDAL;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Net;

namespace NetCoreBLL.Repository
{
    public class HomeRepo : IHome
    {
        private readonly IConfiguration _configure;
        private readonly Response _response;
        private SQLManager objSQL;
        private SqlCommand objCmd;
        private DataTable dtResult;

        public HomeRepo(IConfiguration configure)
        {
            _configure = configure;
            _response = new Response();
        }

        /// <summary>
        /// It is used for get all users
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<Response> GetUsers()
        {
            try
            {
                objSQL = new SQLManager(_configure);
                objCmd = new SqlCommand("spGetUsers");
                dtResult = new DataTable();
                dtResult = objSQL.FetchDT(objCmd);
                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    var objUser = (from DataRow dr in dtResult.Rows
                                   select new
                                   {
                                       userID = Convert.ToInt64(dr["userId"]),
                                       name = dr["name"].ToString(),
                                       mobileNo = dr["mobileNo"].ToString(),
                                       userName = dr["UserName"].ToString(),
                                       isActive = Convert.ToBoolean(dr["isActive"]),
                                       createdDate = dr["createdDate"].ToString()
                                   }).ToList();
                    if (objUser != null)
                    {
                        _response.Status = (int)HttpStatusCode.OK;
                        _response.Data = objUser;
                        return _response;
                    }
                    else
                    {
                        _response.Status = (int)HttpStatusCode.UnprocessableEntity;
                        _response.Data = Enumerable.Empty<dynamic>();
                        return _response;
                    }
                }
                else
                {
                    _response.Status = (int)HttpStatusCode.UnprocessableEntity;
                    _response.Data = Enumerable.Empty<dynamic>();
                    return _response;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                dtResult.Dispose();
                Dispose();
            }
        }

        /// <summary>
        /// It is used for dispose all sql objects
        /// </summary>
        private void Dispose()
        {
            objCmd.Dispose();
            objSQL.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
