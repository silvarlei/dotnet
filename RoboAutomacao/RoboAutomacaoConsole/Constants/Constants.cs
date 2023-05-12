using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboAutomacaoConsole.Constants
{
    public static class Configuration
    {
        // https://localhost:44310
        // https://apigateway-hom.localiza.com/uniid-uniid-netcore
        //public static int ThreadsNumber { get; } = 500;

        //public static string UrlApi { get; set; } = "https://apigateway-hom.localiza.com/uniid-uniid-netcore";
        //public static string ClientId { get; set; } = "localiza_appreservas_code";
        //public static string ClientSecret { get; set; } = "665782ce-ab9b-4d5c-8389-5d7613bc0922";
        //public static string EndpointToken { get; set; } = "/auth/token";
        //public static string EndpointUserInfo { get; set; } = "/auth/userinfo";
        //public static string ConnectionString { get; } = "Server=TCR-00658\\SQLEXPRESS;Database=testeBotDB;Trusted_Connection=True;";
        //public static string TableName { get; } = "TestUniId2_Results";

        public static int ThreadsNumber { get; } = 500;

        public static string UrlApi { get; set; } = "https://api-labs-hom.localiza.com/uniid-uniid-netcore";
        public static string ClientId { get; set; } = "localiza_appreservas_code";
        public static string ClientSecret { get; set; } = "3de5ac2d-a67a-4748-b5c3-3679d47ed800";
        public static string EndpointToken { get; set; } = "/auth/token";
        public static string EndpointUserInfo { get; set; } = "/auth/userinfo";
        public static string ConnectionString { get; } = "Server=TCR-00658\\SQLEXPRESS;Database=testeBotDB;Trusted_Connection=True;";
        public static string TableName { get; } = "TestUniId2_Results";
    }
}
