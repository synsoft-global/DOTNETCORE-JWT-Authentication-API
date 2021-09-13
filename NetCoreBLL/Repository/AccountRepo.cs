using System;
using System.Data;
using System.Linq;
using System.Net;
using NetCoreDAL;
using NetCoreDAL.Models;
using System.Data.SqlClient;
using System.Threading.Tasks;
using NetCoreBLL.Interface;
using Microsoft.Extensions.Configuration;

namespace NetCoreBLL.Repository
{
    public class AccountRepo : IAccount
    {
        private readonly IConfiguration _configure;
        private readonly IJWTGenerator _jWTGenerator;
        private readonly Response _response;
        private SQLManager objSQL;
        private SqlCommand objCmd;
        private DataTable dtResult;

        public AccountRepo(IConfiguration configure, IJWTGenerator jWTGenerator)
        {
            _configure = configure;
            _jWTGenerator = jWTGenerator;
            _response = new Response();
        }

        /// <summary>
        /// It is used for validate user into DB & & generat jwt token
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<Response> SignIn(SignIn signIn)
        {
            try
            {
                objSQL = new SQLManager(_configure);
                objCmd = new SqlCommand("spLogin");
                objCmd.Parameters.AddWithValue("@userName", signIn.userName);
                objCmd.Parameters.AddWithValue("@password", signIn.password);
                dtResult = new DataTable();
                dtResult = objSQL.FetchDT(objCmd);
                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    var objUser = (from DataRow dr in dtResult.Rows
                                   select new User()
                                   {
                                       userID = Convert.ToInt64(dr["userId"]),
                                       name = dr["name"].ToString()
                                   }).FirstOrDefault();
                    if (objUser != null)
                    {
                        _response.Status = (int)HttpStatusCode.Created;
                        _response.Data = await _jWTGenerator.GenerateJwtToken(objUser);
                        return _response;
                    }
                    else
                    {
                        _response.Status = (int)HttpStatusCode.Unauthorized;
                        _response.Data = Enumerable.Empty<dynamic>();
                        return _response;
                    }
                }
                else
                {
                    _response.Status = (int)HttpStatusCode.Unauthorized;
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
        /// It is used for add user into DB & & generat jwt token
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<Response> SignUp(SignUp signUp)
        {
            try
            {
                objSQL = new SQLManager(_configure);
                objCmd = new SqlCommand("spAddUser");
                objCmd.Parameters.AddWithValue("@name", signUp.name);
                objCmd.Parameters.AddWithValue("@mobileNo", signUp.mobileNo);
                objCmd.Parameters.AddWithValue("@userName", signUp.mobileNo);
                objCmd.Parameters.AddWithValue("@password", signUp.password);
                objCmd.Parameters.AddWithValue("@createdDate", DateTime.UtcNow);
                objCmd.Parameters.AddWithValue("@isActive", 1);
                dtResult = new DataTable();
                dtResult = objSQL.FetchDT(objCmd);
                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (dtResult.Rows[0]["statusCode"].ToString() == "1")
                    {
                        var objUser = (from DataRow dr in dtResult.Rows
                                       select new User()
                                       {
                                           userID = Convert.ToInt64(dr["userId"]),
                                           name = dr["name"].ToString()
                                       }).FirstOrDefault();
                        _response.Status = (int)HttpStatusCode.Created;
                        _response.Message = dtResult.Rows[0]["msg"].ToString();
                        _response.Data = await _jWTGenerator.GenerateJwtToken(objUser);
                        return _response;
                    }
                    else
                    {
                        _response.Status = (int)HttpStatusCode.UnprocessableEntity;
                        _response.Message = dtResult.Rows[0]["msg"].ToString();
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
