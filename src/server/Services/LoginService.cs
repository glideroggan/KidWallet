using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;
using server.Data;
using server.Data.User;

namespace server.Services
{
    internal record TokenModel(int UserId, string Name, int Iat);
    
    public class LoginService
    {
        private readonly UserService _userService;
        private readonly AppState _appState;
        private int _saltSize; 
        private int _iterCount;

        public LoginService(UserService userService, AppState appState, IConfiguration config)
        {
            _userService = userService;
            _appState = appState;
            _jwtHandler = new JwtSecurityTokenHandler();
            _secret = config["secret"];
            _saltSize = int.Parse(config["saltSize"]);
            _iterCount = int.Parse(config["iterCount"]);
        }

        public async Task<(bool Ok, string Token)> ValidateAsync(string username, string? password=null)
        {
            var user = await _userService.GetUserAsync(username);
            var valid = user.Role switch
            {
                RoleEnum.Child => true,
                RoleEnum.Parent when string.IsNullOrEmpty(password) => false,
                RoleEnum.Parent when 
                    VerifyPassword(Convert.FromBase64String(user.PasswordHash), password) => true,
                _ => false
            };

            if (!valid) return (false, null);
            
            var token = GenerateToken(user.Id, user.Name);
            if (user != null)
            {
                _appState.User = user;
                _appState.Balance = user.Balance;
                _appState.NotifyStateChanged();
            }

            return (valid, token);
        }

        // Compares two byte arrays for equality. The method is specifically written so that the loop is not optimized.
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static bool ByteArraysEqual(byte[] a, byte[] b)
        {
            if (a == null && b == null)
            {
                return true;
            }
            if (a == null || b == null || a.Length != b.Length)
            {
                return false;
            }
            var areSame = true;
            for (var i = 0; i < a.Length; i++)
            {
                areSame &= (a[i] == b[i]);
            }
            return areSame;
        }
        
        
        private bool VerifyPassword(byte[] hashedPassword, string passwordHash)
        {
            var prf = KeyDerivationPrf.HMACSHA256;
            var salt = new byte[_saltSize];
            Buffer.BlockCopy(hashedPassword, 0, salt, 0, salt.Length);

            var expectedSubKey = new byte[hashedPassword.Length - salt.Length];
            Buffer.BlockCopy(hashedPassword, salt.Length, expectedSubKey, 0, expectedSubKey.Length);

            var actualSubKey = KeyDerivation.Pbkdf2(passwordHash, salt, prf, _iterCount, expectedSubKey.Length);
            return ByteArraysEqual(expectedSubKey, actualSubKey);
        }

        private string GenerateToken(int userId, string name)
        {
            var tokenModel = CreateToken(userId, name,
                Convert.ToInt32((DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds));
            var token = SignToken(tokenModel);
            return token;
        }
        
        private static readonly Dictionary<string, string> PropertyList = new()
        {
            ["UserId"] = "Int32",
            ["Name"] = "String",
            ["Iat"] = "Int32",
            // ["SpendingAccountId"] = "Int32",
            // ["ParentId"] = "Int32?",
        };

        private readonly JwtSecurityTokenHandler _jwtHandler;
        private readonly string _secret;

        private string SignToken(TokenModel model)
        {
            var header = "{\"alg\": \"HS256\",\"typ\": \"JWT\"}";
            var payload = MapProperties(PropertyList, model);
            var token = Sign(header, payload);
            return token;
        }

        private string Sign(string header, string payload)
        {
            var rawHeader = header;
            var rawPayload = payload;
            // FEAT: would be nice to switch to public/private keys
            var secret = _secret;
            var data = Base64UrlEncoder.Encode(rawHeader) + "." + Base64UrlEncoder.Encode(rawPayload);
            var hash = Hash(data, secret);
            return data + "." + hash;
        }

        private string Hash(string data, string secret)
        {
            var encoding = Encoding.ASCII;
            // FEAT: secret needs to be at least 64 bytes long
            using var hasher = new HMACSHA256(encoding.GetBytes(secret));
            var hashBytes = hasher.ComputeHash(encoding.GetBytes(data));
            var hash = Base64UrlEncoder.Encode(hashBytes, 0, hashBytes.Length);
            return hash;
        }

        private string MapProperties(Dictionary<string, string> propertyList, TokenModel model)
        {
            // TODO: fix this mess
            var options = new JsonWriterOptions
            {
                Indented = true
            };
            using var stream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(stream, options))
            {
                writer.WriteStartObject();
                var props = model.GetType().GetProperties();
                foreach (var pair in propertyList)
                {
                    var name = pair.Key;
                    var dataType = pair.Value;
                    var found = false;
                    foreach (var prop in props)
                    {
                        if (!string.Equals(prop.Name, name, StringComparison.InvariantCultureIgnoreCase)) continue;
                        
                        found = true;
                        var val = prop.GetValue(model);
                        switch (dataType)
                        {
                            case "Int32?":
                                if (val == null) writer.WriteNull(name);
                                else writer.WriteNumber(name, Convert.ToInt32(val));
                                break;
                            case "Int32":
                                writer.WriteNumber(name, Convert.ToInt32(val));
                                break;
                            case "String":
                                writer.WriteString(name, val.ToString());
                                break;
                            case "Double":
                                writer.WriteNumber(name, Convert.ToDouble(val));
                                break;
                            default: throw new NotImplementedException();
                        }
                        break;
                    }

                    if (!found) throw new Exception($"Missed adding '{name}' property to TokenModel.");
                }
                writer.WriteEndObject();
            }
            
            var json = Encoding.UTF8.GetString(stream.ToArray());
            return json;
        }

        private TokenModel CreateToken(int userId, string name, int iat)
        {
            return new TokenModel(userId, name, iat);
        }

        internal async Task<IList<LoginUserModel>> GetUsers()
		{
            // TODO: cache the results for some time (session?)
            var loginUsers = await _userService.GetAll(dto =>
                new LoginUserModel(dto.Name, dto.ProfileImg, dto.Role)
            );
            return loginUsers;
        }
        
        public async Task<bool> ValidateLoginAsync(Func<ValueTask<ProtectedBrowserStorageResult<string>>> GetTokenCallbackAsync)
        {
            if (_appState.User == null)
            {
                var tokenResults = await GetTokenCallbackAsync();
                if (tokenResults.Success)
                {
                    var jwtToken = _jwtHandler.ReadJwtToken(tokenResults.Value);
                    if (!ValidateToken(jwtToken))
                    {
                        return false;
                    }
                    // TODO: set the user
                    var userName = jwtToken.Claims.First(x => x.Type == "Name").Value;
                    var user = await _userService.GetUserAsync(userName);
                    _appState.User = user;
                    _appState.Balance = user.Balance;
                    _appState.NotifyStateChanged();
                    
                    // TODO: check that the token is of the server version
                    // otherwise we need to log you in again and update your token
                    
                    // if (results.Valid)
                    // {
                    //     LoginUser(results);
                    // }

                    // TODO: we should update the token if the expire time is getting closer
                    return true;
                }
                

                return false;
            }

            return true;
            
        }

        private bool ValidateToken(JwtSecurityToken jwtToken)
        {
            var secret = _secret;
            var data = jwtToken.RawHeader + "." + jwtToken.RawPayload;
            var hash = Hash(data, secret);
            return string.Equals(jwtToken.RawSignature, hash);
        }
    }
}
