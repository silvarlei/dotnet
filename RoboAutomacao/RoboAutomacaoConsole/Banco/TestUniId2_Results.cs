using RoboAutomacaoConsole.Constants;
using RoboAutomacaoConsole.Model;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboAutomacaoConsole.Banco
{
    public class TestUniId2_Results
    {

        public  async Task<string>? BuscarRefresh(Guid userid)
        {
            using (var con = new SqlConnection(Configuration.ConnectionString))
            {
                string qry = "Select Id, RefreshTokenNew, FirstUserId FROM " +
                    $"(SELECT MAX(Id) myid FROM [testeBotDB].[dbo].[{Configuration.TableName}] " +
                    $"WHERE RefreshTokenNew != '-' AND FirstUserId = '{userid}' " +
                    $"Group by FirstUserId) aa join [testeBotDB].[dbo].[{Configuration.TableName}] on myid = id";
                var cmd = new SqlCommand(qry, con);
                cmd.CommandType = CommandType.Text;
                OpenConnection(con);
                using (SqlDataReader objReader = cmd.ExecuteReader())
                {
                    if (objReader.HasRows)
                    {
                        while (objReader.Read())
                        {
                            string item = objReader.GetString(objReader.GetOrdinal("RefreshTokenNew"));
                            return item;
                        }
                    }
                }
            }

            return string.Empty;
        }

        public static bool OpenConnection(SqlConnection sqlConnection)
        {
            var success = false;
            do
            {
                try
                {
                    sqlConnection.Open();
                    success = true;
                }
                catch (Exception)
                {
                    success = false;
                    Log.Error($"[OpenConnection] Falha ao tentar conectar no banco");
                    Thread.Sleep(5000);
                }
            } while (!success);

            return true;
        }


        public  async Task<bool> InsertAuditTable(string firstUserId, string accessTokenRedis, string refreshTokenRedis, TokenResponse tokens, string userInfo, bool compatibleAccessOldNew, bool compatibleAccessIdToken, bool compatibleUserinfoIdToken)
        {
            try
            {
                using (var openCon = new SqlConnection(Configuration.ConnectionString))
                {
                    string saveStaff = $"INSERT into {Configuration.TableName} (AccessTokenRedis,RefreshTokenRedis,AccessTokenNew,IdTokenNew,RefreshTokenNew,CompatibleAccessOldNew,CompatibleAccessIdToken,CompatibleUserInfo,FirstUserId,UserInfo,DateInserted) " +
                                       "VALUES (@accessTokenRedis,@refreshTokenRedis,@accessTokenNew,@idTokenNew,@refreshTokenNew,@compatibleAccessOldNew,@compatibleAccessIdToken,@compatibleUserInfo,@firstUserId,@userInfo,@DateInserted)";

                    using (SqlCommand querySaveStaff = new SqlCommand(saveStaff))
                    {
                        querySaveStaff.Connection = openCon;
                        querySaveStaff.Parameters.Add("@firstUserId", SqlDbType.NVarChar).Value = firstUserId;
                        querySaveStaff.Parameters.Add("@accessTokenRedis", SqlDbType.NVarChar).Value = accessTokenRedis;
                        querySaveStaff.Parameters.Add("@refreshTokenRedis", SqlDbType.NVarChar).Value = refreshTokenRedis;
                        querySaveStaff.Parameters.Add("@accessTokenNew", SqlDbType.NVarChar).Value = tokens.AccessToken;
                        querySaveStaff.Parameters.Add("@idTokenNew", SqlDbType.NVarChar).Value = tokens.TokenId;
                        querySaveStaff.Parameters.Add("@refreshTokenNew", SqlDbType.NVarChar).Value = tokens.RefreshToken;
                        querySaveStaff.Parameters.Add("@compatibleAccessOldNew", SqlDbType.Bit).Value = compatibleAccessOldNew ? 1 : 0;
                        querySaveStaff.Parameters.Add("@compatibleAccessIdToken", SqlDbType.Bit).Value = compatibleAccessIdToken ? 1 : 0;
                        querySaveStaff.Parameters.Add("@compatibleUserInfo", SqlDbType.Bit).Value = compatibleUserinfoIdToken ? 1 : 0;
                        querySaveStaff.Parameters.Add("@userInfo", SqlDbType.NVarChar).Value = userInfo;
                        querySaveStaff.Parameters.Add("@DateInserted", SqlDbType.DateTime).Value = DateTime.Now;

                        OpenConnection(openCon);

                        querySaveStaff.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[InsertAuditTable] {ex.Message}");
                return false;
            }

            return true;
        }

    }
}
