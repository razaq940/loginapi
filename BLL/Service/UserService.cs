using DAL.Models;
using DAL.Models.Dto.User;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Service
{
    public class UserService
    {
        //Generate Token
        public Tuple<bool, string> GenerateToken(long userId, string email, string roleBase)
        {
            try
            {
                DotNetEnv.Env.Load();
                var claims = new List<Claim>();
                claims.Add((new Claim(JwtRegisteredClaimNames.Sub, email)));
                claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
                claims.Add(new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToString()));
                claims.Add(new Claim("userId", userId.ToString()));
                claims.Add(new Claim("email", email));
                claims.Add(new Claim(ClaimTypes.Role, roleBase));
                int tokenExpired = 60;
                SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("Key")));
                SigningCredentials signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                JwtSecurityToken token = new JwtSecurityToken(Environment.GetEnvironmentVariable("Issuer"), "StoreApi", claims, expires: DateTime.Now.AddMinutes(tokenExpired), signingCredentials: signIn);
                return Tuple.Create(true, new JwtSecurityTokenHandler().WriteToken(token));
            }
            catch (Exception Ex)
            {
                return Tuple.Create(false, Ex.Message);
            }
        }
        //Login user
        public Tuple<int, long, string, string> Login(UserLoginDto data)
        {
            try
            {
                using (loginDbContext _context = new loginDbContext())
                {
                    using (var _transaction = _context.Database.BeginTransaction())
                    {
                        #region cek valid email
                        try
                        {
                            MailAddress em = new MailAddress(data.Email);
                        }
                        catch (FormatException)
                        {
                            return Tuple.Create(-1, long.Parse("0"), string.Empty, "Please Input Valid Email");
                        }
                        #endregion cek valid email

                        #region cek email exist
                        var user = _context.Users.Where(u => u.Email == data.Email).FirstOrDefault();
                        if (user == null)
                        {
                            return Tuple.Create(-1, long.Parse("0"), string.Empty, "Email not exist");
                        }
                        #endregion cek email exist


                        #region cek user is not Activation
                        if (user.IsActive == false)
                        {
                            #region add otp
                            OTP otp = new OTP()
                            {
                                Otp = new Random().Next(1000, 9999).ToString(),
                                FkUser = user.Id
                            };
                            _context.OTPs.Add(otp);
                            _context.SaveChanges();
                            #endregion add otp

                            #region send email
                            var res = EmailService.SendEmailVerifikasi(data.Email, otp.Otp);
                            if (res.Item1 == -1)
                            {
                                _transaction.Rollback();
                                return Tuple.Create(-1, long.Parse("0"), string.Empty, "Failed Send Email OTP");
                            }
                            #endregion send email

                            return Tuple.Create(2, user.Id, user.Email, "your account has not been verified, otp send to your email");
                        }
                        #endregion cek user is not Activation

                        #region cek password
                        var pswdencrpt = NawaEncryption.Common.Encrypt(data.Password, user.Salt);
                        if (!pswdencrpt.Equals(user.Password))
                        {
                            return Tuple.Create(-1, long.Parse("0"), string.Empty, "Password wrong");
                        }
                        #endregion cek password


                        #region generate token
                        var role = user.Role;
                        var token = GenerateToken(user.Id, user.Email, role);
                        if (token.Item1 == false)
                        {
                            return Tuple.Create(-1, user.Id, String.Empty, token.Item2);
                        }
                        #endregion generate token

                        return Tuple.Create(1, user.Id, token.Item2, "Login Success");
                    }
                }
            }
            catch (Exception Ex)
            {
                return Tuple.Create(-1, long.Parse("0"), string.Empty, Ex.Message);
            }
        }
        //activation user
        public Tuple<int, long, string, string> ActivationUser(ActivationUserDto data)
        {
            try
            {
                using (var _context = new loginDbContext())
                {
                    using (var _transaction = _context.Database.BeginTransaction())
                    {
                        try
                        {
                            #region cek user id exist 

                            var thisuser = _context.Users.Where(u => u.Email == data.Email).FirstOrDefault();
                            var userId = _context.Users.Where(u => u.Email == data.Email).FirstOrDefault().Id;
                            if (thisuser == null)
                            {
                                return Tuple.Create(-1, long.Parse("0"), string.Empty, "Failed activation = your email does'n exist");
                            }
                            #endregion cek user Id exist


                            #region cek otp
                            var otpInDb = (from p in _context.OTPs.Where(o => o.FkUser == userId) orderby p.Id descending select p.Otp).FirstOrDefault();
                            if (otpInDb != data.otp)
                            {
                                return Tuple.Create(1, userId, string.Empty, "Failed activation = otp code wrong");
                            }
                            #endregion cek otp 

                            #region activation user
                            var user = _context.Users.Where(u => u.Id == userId).FirstOrDefault();
                            user.IsActive = true;
                            _context.Users.Update(user);
                            _context.SaveChanges();
                            #endregion activation user



                            #region create token
                            var role = user.Role;
                            var token = GenerateToken(userId, user.Email, role);
                            if (token.Item1 == false)
                            {
                                return Tuple.Create(-1, userId, String.Empty, token.Item2);
                            }
                            #endregion create token

                            _transaction.Commit();
                            _transaction.Dispose();
                            return Tuple.Create(1, userId, token.Item2, "Activation Succes");

                        }
                        catch (Exception Ex)
                        {
                            _transaction.Rollback();
                            _transaction.Dispose();
                            return Tuple.Create(-1, long.Parse("0"), string.Empty, Ex.Message);
                        }

                    }
                }
            }
            catch (Exception Ex)
            {
                return Tuple.Create(-1, long.Parse("0"), string.Empty, Ex.Message);
            }
        }
        //create new user
        public Tuple<int, string> RegisterNewUser(User data)
        {
            try
            {
                using (var _context = new loginDbContext())
                {
                    using (var _transaction = _context.Database.BeginTransaction())
                    {
                        try
                        {
                            #region Add User
                            var validate = ValidateUserInput(data);
                            if (validate.Item1 == -1)
                            {
                                return Tuple.Create(-1, validate.Item2);
                            }
                            data.Salt = Guid.NewGuid().ToString();
                            data.Password = NawaEncryption.Common.Encrypt(data.Password, data.Salt);
                            data.IsActive = false;
                            _context.Users.Add(data);
                            _context.SaveChanges();
                            #endregion Add User

                            #region add otp
                            OTP otp = new OTP()
                            {
                                Otp = new Random().Next(1000, 9999).ToString(),
                                FkUser = data.Id
                            };
                            _context.OTPs.Add(otp);
                            _context.SaveChanges();
                            #endregion add otp

                            #region send email
                            var res = EmailService.SendEmailVerifikasi(data.Email, otp.Otp);
                            if (res.Item1 == -1)
                            {
                                _transaction.Rollback();
                                return Tuple.Create(-1, "Failed Send OTP Please Re Register");
                            }
                            #endregion send email


                            _transaction.Commit();
                            _transaction.Dispose();
                            return Tuple.Create(1, "Succes Create New User");

                        }
                        catch (Exception Ex)
                        {
                            _transaction.Rollback();
                            return Tuple.Create(-1, Ex.Message);
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                return Tuple.Create(-1, Ex.Message);
            }
        }

        //Cek Data User
        public Tuple<int, string> ValidateUserInput(User user)
        {
            try
            {
                using (var _context = new loginDbContext())
                {
                    #region cek email
                    if (_context.Users.Any(p => p.Email == user.Email))
                    {
                        return Tuple.Create(-1, "Email Already Used");
                    }
                    #endregion cek email



                    return Tuple.Create(1, "Valid Credential");
                }
            }
            catch (Exception Ex)
            {
                return Tuple.Create(-1, Ex.Message);
            }
        }
    }
}
