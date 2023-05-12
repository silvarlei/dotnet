using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using RoboAutomacaoConsole.Constants;
using RoboAutomacaoConsole.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace RoboAutomacaoConsole
{
    public  class Metodos
    {

        public Metodos()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("logs\\AuditoriaResults.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            Log.Information("Olá, sou robô de teste do UniID !");
            Log.Information("Bora testar tudo !");
        }

        public async void Test2(string refresh)
        {
            Log.Information($"[Test2][Iniciado]");

            // Configuração do número de threads simultâneas
            var totalThreads = Configuration.ThreadsNumber;

            // Busca IDs de usuários
            var userIds = new List<Guid>();
            userIds.Add(new Guid()); 
            var userNumber = 0;

           var primeiroResult = await ClientTestFirst(userIds[userNumber], refresh);

            var ts = new Thread[totalThreads];
            for (int i = 0; i < totalThreads; i++)
            {
                ts[i] = new Thread(async () =>
                {
                    int temp = i;
                    //var refresh = string.Empty;
                    var result = false;

                    while (!result && userNumber < userIds.Count)
                    {

                        refresh = await new Banco.TestUniId2_Results().BuscarRefresh(userIds[userNumber]);

                        result = await ClientTest(userIds[userNumber], refresh);

                        userNumber++;
                    }
                });
                ts[i].Start();
                Log.Information($"[Test2] Thread: {i} iniciada");
                Thread.Sleep(1000);
            }
            for (int i = 0; i < totalThreads; i++)
            {
                ts[i].Join();
            }
        }

        public async Task<bool> ClientTest(Guid userId, string refreshToken)
        {
            Log.Information($"[ClientTest][Iniciado] UserId: {userId}");
            int count = 0;
            while (!refreshToken.IsNullOrEmpty())
            {
                // Utiliza refresh_token para buscar novos tokens
                var tokens = await new API().GetTokens(refreshToken);
                if (tokens is null)
                {
                    // Falha ao tentar utilizar refresh_token
                    // await InsertAuditTable(userId.ToString(), "-", refreshToken);
                    Log.Warning($"[ClientTest] Refresh_token inválido. UserId: {userId}, refresh_token: {refreshToken}");
                    return false;
                }

                // Extrai dados do access_token novo
                var accessTokenUserNew = await ExtractAccessToken(tokens.AccessToken);

                // Extrai dados do id_token novo
                var idTokenUser = await ExtractIdToken(tokens.TokenId);

                // Compara dados dos tokens
                var resultAccess = false;
                var resultIdAccess = false;

                if (accessTokenUserNew is not null)
                {
                    resultAccess = CompareAccessTokens(accessTokenUserNew, accessTokenUserNew);

                    if (idTokenUser is not null)
                        resultIdAccess = CompareAccessToIdToken(accessTokenUserNew, idTokenUser);
                }

                // --- UserInfo ---
                // Busca dados UserInfo
                var userInfoJson = await new API().SendUserInfo(tokens.AccessToken);

                // Compara dados dos tokens com dados do UserInfo
                var userInfo = await ExtractUserInfo(userInfoJson);
                var resultUserinfoIdtoken = false;
                if (userInfoJson.IsNullOrEmpty() || userInfo is null)
                    Log.Error($"[ClientTest] Falha ao converter resposta userInfo. Comparação não pode ser realizada");
                else
                    resultUserinfoIdtoken = CompareUserinfoAndIdtoken(userInfo, idTokenUser);

                // Grava informações de testes
                await new Banco.TestUniId2_Results().InsertAuditTable(userId.ToString(), "-", refreshToken, tokens, userInfoJson, resultAccess, resultIdAccess, resultUserinfoIdtoken);

                count += 1;

                // Utiliza novo refresh_token gerado
                refreshToken = tokens.RefreshToken;
            }

            Log.Information($"[ClientTest][Encerrado] UserId: {userId}. Count loop: {count}");
            return true;
        }

        public async Task<bool> ClientTestFirst(Guid userId, string refreshToken)
        {
            Log.Information($"[ClientTest][Iniciado] UserId: {userId}");
            int count = 0;
            if (!refreshToken.IsNullOrEmpty())
            {
                // Utiliza refresh_token para buscar novos tokens
                var tokens = await new API().GetTokens(refreshToken);
                if (tokens is null)
                {
                    // Falha ao tentar utilizar refresh_token
                    // await InsertAuditTable(userId.ToString(), "-", refreshToken);
                    Log.Warning($"[ClientTest] Refresh_token inválido. UserId: {userId}, refresh_token: {refreshToken}");
                    return false;
                }

                // Extrai dados do access_token novo
                var accessTokenUserNew = await ExtractAccessToken(tokens.AccessToken);

                // Extrai dados do id_token novo
                var idTokenUser = await ExtractIdToken(tokens.TokenId);

                // Compara dados dos tokens
                var resultAccess = false;
                var resultIdAccess = false;

                if (accessTokenUserNew is not null)
                {
                    resultAccess = CompareAccessTokens(accessTokenUserNew, accessTokenUserNew);

                    if (idTokenUser is not null)
                        resultIdAccess = CompareAccessToIdToken(accessTokenUserNew, idTokenUser);
                }

                // --- UserInfo ---
                // Busca dados UserInfo
                var userInfoJson = await new API().SendUserInfo(tokens.AccessToken);

                // Compara dados dos tokens com dados do UserInfo
                var userInfo = await ExtractUserInfo(userInfoJson);
                var resultUserinfoIdtoken = false;
                if (userInfoJson.IsNullOrEmpty() || userInfo is null)
                    Log.Error($"[ClientTest] Falha ao converter resposta userInfo. Comparação não pode ser realizada");
                else
                    resultUserinfoIdtoken = CompareUserinfoAndIdtoken(userInfo, idTokenUser);

                // Grava informações de testes
                await new Banco.TestUniId2_Results().InsertAuditTable(userId.ToString(), "-", refreshToken, tokens, userInfoJson, resultAccess, resultIdAccess, resultUserinfoIdtoken);

                count += 1;

                // Utiliza novo refresh_token gerado
                refreshToken = tokens.RefreshToken;
            }

            Log.Information($"[ClientTest][Encerrado] UserId: {userId}. Count loop: {count}");
            return true;
        }



        public static bool CompareAccessTokens(AccessTokenParameters access1, AccessTokenParameters access2)
        {
            if (access1.UserId == access2.UserId &&
                access1.Name == access2.Name &&
                access1.ClientId == access2.ClientId &&
                access1.Idp == access2.Idp)
            {
                Log.Information("[CompareAccessTokens][Sucesso] AccessTokens compatíveis");
                return true;
            }

            Log.Warning($"[CompareAccessTokens] Falha na comparação dos accessTokens - Access1 UserId: {access1.UserId} - Access2 UserId: {access2.UserId}");
            return false;
        }

        public static bool CompareAccessToIdToken(AccessTokenParameters accessToken, IdTokenParameters idToken)
        {
            if (accessToken.UserId == idToken.UserId &&
                accessToken.Name == idToken.Name &&
                accessToken.ClientId == idToken.ClientId &&
                accessToken.Idp == idToken.Idp)
            {
                Log.Information("[CompareAccessToIdToken][Sucesso] AccessToken e IdToken compatíveis");
                return true;
            }

            Log.Warning($"[CompareAccessToIdToken] Falha na comparação do access e id token - Access UserId: {accessToken.UserId} - IdToken UserId: {idToken.UserId}");
            return false;
        }

        public static bool CompareUserinfoAndIdtoken(UserInfoParameters userInfo, IdTokenParameters idToken)
        {
            if (userInfo.UserId == idToken.UserId &&
                userInfo.Name == idToken.Name &&
                userInfo.Document.Type == idToken.Document.Type &&
                userInfo.Document.Number == idToken.Document.Number)
            {
                Log.Information("[CompareUserinfoAndIdtoken][Sucesso] AccessToken e IdToken compatíveis");
                return true;
            }

            Log.Warning($"[CompareUserinfoAndIdtoken] Falha na comparação do userinfo e id_token - UserInfo UserId: {userInfo.UserId} - IdToken UserId: {idToken.UserId}");
            return false;
        }


        public static async Task<IdTokenParameters> ExtractParametersFromIdToken(JwtSecurityToken idToken)
        {
            var idTokenUser = new IdTokenParameters();

            //ID do usuário
            if (Guid.TryParse(idToken.Subject, out Guid result))
            {
                idTokenUser.UserId = result;
                idTokenUser.Name = idToken.Claims.FirstOrDefault((c) => c.Type.Equals("name"))?.Value;
                idTokenUser.Issuer = idToken.Issuer;
                idTokenUser.Idp = idToken.Claims.FirstOrDefault((c) => c.Type.Equals("idp"))?.Value;
                idTokenUser.ClientId = idToken.Claims.FirstOrDefault((c) => c.Type.Equals("aud"))?.Value;

                var document = idToken.Claims.FirstOrDefault((c) => c.Type.Equals("document"))?.Value;
                idTokenUser.Document = JsonConvert.DeserializeObject<Models.Document>(document);
                if (idTokenUser.Document is null)
                {
                    Log.Warning("[ExtractParametersFromIdToken] Documento não pode ser convertido.");
                    idTokenUser.Document = new Models.Document();
                }

                idTokenUser.RequestedScopes = idToken.Claims.Where((c) => c.Type.Equals("scope")).Select((c) => c.Value).ToList();
            }
            else
            {
                Log.Warning("[ExtractParametersFromIdToken] Id de usuário inválido.");
            }

            // Printa valores contidos no idToken
            Print(idTokenUser);
            return idTokenUser;
        }

        //validar o token de acesso JWT
        public static JwtSecurityToken ExtractJwt(string accessToken, bool validate)
        {
            JwtSecurityToken jwtToken = null;
            var errors = new List<string>();

            try
            {
                JwtSecurityTokenHandler tokenHandler = new();

                jwtToken = tokenHandler.ReadJwtToken(accessToken);
            }
            catch (Exception)
            {
                var errorMessage = "[AccessTokenError] Token de acesso não é um Token JWT.";
                Log.Warning(errorMessage);
                errors.Add(errorMessage);
            }

            if (validate && jwtToken.ValidTo < DateTime.UtcNow)
            {
                var errorMessage = "[AccessTokenError] Token de acesso já está expirado.";
                Log.Warning(errorMessage);
                errors.Add(errorMessage);
            }

            return jwtToken;
        }
        public static void Print(IdTokenParameters idTokenParameters)
        {
            try
            {
                var text = $"Userid: {idTokenParameters.UserId} - " +
                            $"Name: {idTokenParameters.Name} - " +
                            $"ClientId: {idTokenParameters.ClientId} - " +
                            $"Issuer: {idTokenParameters.Issuer} - " +
                            $"ClientId: {idTokenParameters.ClientId} - " +
                            $"Document: {idTokenParameters.Document.Type} number {idTokenParameters.Document.Number} - " +
                            $"Idp: {idTokenParameters.Idp} - " +
                            $"Requested Scopes: ";
                foreach (var scope in idTokenParameters.RequestedScopes)
                    text += $"{scope}, ";

                Log.Information(text);
            }
            catch (Exception ex)
            {
                Log.Warning($"Erro ao imprimir idTokenParameter. {ex.Message}");
            }
        }

        public static void Print(AccessTokenParameters accessTokenParameters)
        {
            try
            {
                var text = $"Userid: {accessTokenParameters.UserId} - " +
                            $"Name: {accessTokenParameters.Name} - " +
                            $"ClientId: {accessTokenParameters.ClientId} - " +
                            $"Issuer: {accessTokenParameters.Issuer} - " +
                            $"Authtime: {accessTokenParameters.AuthTime} - " +
                            $"Idp: {accessTokenParameters.Idp} - " +
                            $"Requested Scopes: ";
                foreach (var scope in accessTokenParameters.RequestedScopes)
                    text += $"{scope}, ";

                Log.Information(text);
            }
            catch (Exception ex)
            {
                Log.Warning($"Erro ao imprimir accessTokenParameter. {ex.Message}");
            }
        }

        public async Task<AccessTokenParameters> ExtractAccessToken(string accessToken, bool validate = true)
        {
            try
            {
                var accessTokenUser = new AccessTokenParameters();

                var jwt = ExtractJwt(accessToken, validate);
                accessTokenUser = await ExtractParametersFromAccessToken(jwt);

                return accessTokenUser;
            }
            catch (Exception ex)
            {
                Log.Error($"[ExtractAccessToken] Erro ao extrair accessToken. {ex.Message}");
            }

            return null;
        }
        public static async Task<AccessTokenParameters> ExtractParametersFromAccessToken(JwtSecurityToken accessToken)
        {
            var accessTokenUser = new AccessTokenParameters();

            //ID do usuário
            if (Guid.TryParse(accessToken.Subject, out Guid result))
            {
                accessTokenUser.UserId = result;
                accessTokenUser.Name = accessToken.Claims.FirstOrDefault((c) => c.Type.Equals("name"))?.Value;
                accessTokenUser.Issuer = accessToken.Issuer;
                accessTokenUser.AuthTime = accessToken.Claims.FirstOrDefault((c) => c.Type.Equals("auth_time"))?.Value;
                accessTokenUser.Idp = accessToken.Claims.FirstOrDefault((c) => c.Type.Equals("idp"))?.Value;
                accessTokenUser.ClientId = accessToken.Claims.FirstOrDefault((c) => c.Type.Equals("client_id"))?.Value;

                accessTokenUser.Audience = accessToken.Audiences.ToList();
                accessTokenUser.RequestedScopes = accessToken.Claims.Where((c) => c.Type.Equals("scope")).Select((c) => c.Value).ToList();
            }
            else
            {
                var errorMessage = "[AccessTokenError] Id de usuário inválido.";
                Log.Warning(errorMessage);
            }

            // Printa valores contidos no accessToken
            Print(accessTokenUser);
            return accessTokenUser;
        }
        public async Task<IdTokenParameters> ExtractIdToken(string idToken, bool validate = true)
        {
            try
            {
                var idTokenUser = new IdTokenParameters();

                var jwt = ExtractJwt(idToken, validate);
                idTokenUser = await ExtractParametersFromIdToken(jwt);

                return idTokenUser;
            }
            catch (Exception ex)
            {
                Log.Error($"[ExtractIdToken] Erro ao extrair idToken. {ex.Message}");
            }

            return null;
        }

        public async Task<UserInfoParameters?> ExtractUserInfo(string userInfo)
        {
            if (userInfo.IsNullOrEmpty())
                return null;

            return JsonConvert.DeserializeObject<UserInfoParameters>(userInfo);
        }


    }
}
