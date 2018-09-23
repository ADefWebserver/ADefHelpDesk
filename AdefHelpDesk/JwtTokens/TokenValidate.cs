using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdefHelpDeskBase.CustomTokenProvider
{
    public class TokenValidate
    {
        #region public static string GetSecretKey(string DefaultConnection)
        public static string GetSecretKey(string DefaultConnection)
        {
            string SecretKey = "tempKey*****************##############";

            using (var conn = new SqlConnection(DefaultConnection))
            {
                var sql = "SELECT TOP (1) ISNULL([SettingValue],'') FROM [dbo].[ADefHelpDesk_Settings] where [SettingName] = 'ApplicationGUID'";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    using (var adapter = new SqlDataAdapter(cmd))
                    {
                        var resultTable = new DataTable();
                        adapter.Fill(resultTable);

                        if (resultTable.Rows.Count > 0)
                        {
                            string TempSecretKey = Convert.ToString(resultTable.Rows[0].ItemArray[0]).Replace("-", "") + CreateRandomKey(10);

                            // Only set if found
                            if (!(TempSecretKey == null || TempSecretKey.Trim() == ""))
                            {
                                SecretKey = TempSecretKey;
                            }
                        }
                    }
                }
            }

            return SecretKey;
        }
        #endregion

        #region public static string GetApplicationGUID(string DefaultConnection)
        public static string GetApplicationGUID(string DefaultConnection)
        {
            string ApplicationGUID = "";

            using (var conn = new SqlConnection(DefaultConnection))
            {
                var sql = "SELECT TOP (1) ISNULL([SettingValue],'') FROM [dbo].[ADefHelpDesk_Settings] where [SettingName] = 'ApplicationGUID'";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    using (var adapter = new SqlDataAdapter(cmd))
                    {
                        var resultTable = new DataTable();
                        adapter.Fill(resultTable);

                        if (resultTable.Rows.Count > 0)
                        {
                            ApplicationGUID = Convert.ToString(resultTable.Rows[0].ItemArray[0]);
                        }
                    }
                }
            }

            return ApplicationGUID;
        }
        #endregion

        #region public static bool ValidateUser(string DefaultConnection, string applicationGUID, string username, string password)
        public static bool ValidateUser(string DefaultConnection, string applicationGUID, string username, string password)
        {
            bool boolValid = false;

            // Check the applicationGUID
            if(applicationGUID != GetApplicationGUID(DefaultConnection))
            {
                boolValid = false;
                return boolValid;
            }

            using (var conn = new SqlConnection(DefaultConnection))
            {
                using (var cmd = new SqlCommand())
                {
                    cmd.CommandText = "SELECT TOP (1) [Id] FROM [dbo].[ADefHelpDesk_ApiSecurity] where [Username] = @Username and [Password] = @Password and [IsActive] = 1";
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;

                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@Password", password);

                    using (var adapter = new SqlDataAdapter(cmd))
                    {
                        var resultTable = new DataTable();
                        adapter.Fill(resultTable);

                        if (resultTable.Rows.Count > 0)
                        {
                            boolValid = true;
                        }
                    }
                }
            }

            return boolValid;
        }
        #endregion

        // Utility

        #region CreateRandomKey
        public static string CreateRandomKey(int KeyLength)
        {
            const string valid = "012389ABCDEFGHIJKLMN4567OPQRSTUVWXYZ";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < KeyLength--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }
        #endregion
    }
}
