using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Nop.Plugin.Payments.Yuansfer.Extensions
{
    public static class CommonHelpers
    {
        #region Methods

        public static string GenerateSign(string token, (string Name, string Value)[] parameters)
        {
            if (token is null)
                throw new ArgumentNullException(nameof(token));

            if (parameters is null)
                throw new ArgumentNullException(nameof(parameters));

            parameters = parameters.OrderBy(p => p.Name).ToArray();

            var parametersBuilder = new StringBuilder();
            foreach (var parameter in parameters)
                parametersBuilder.Append($"{parameter.Name}={parameter.Value}&");

            parametersBuilder.Append(MD5(token));

            return MD5(parametersBuilder.ToString());
        }

        #endregion

        #region Utilities

        private static string MD5(string message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            using var cryptographer = new MD5CryptoServiceProvider();
            var hashBytes = cryptographer.ComputeHash(messageBytes);

            return BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLower();
        }

        #endregion
    }
}
