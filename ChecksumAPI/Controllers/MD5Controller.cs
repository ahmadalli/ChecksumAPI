using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;

namespace ChecksumAPI.Controllers
{
    [Produces("application/json")]
    [Route("/MD5")]
    public class MD5Controller : Controller
    {
        public MD5Controller(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        [HttpGet]
        public string MD5(string fileUrl, int? offsetPercent)
        {
            byte[] result;
            using (var webClient = new System.Net.WebClient())
            {
                result = webClient.DownloadData(fileUrl);
            }
            var offset = result.Length * (offsetPercent ?? Convert.ToInt32(Configuration["MD5FileOffsetPercent"])) / 200;
            byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(result, offset, result.Length - offset);

            return BitConverter
                    .ToString(hash)
                    .Replace("-", string.Empty)
                    .ToLower();
        }
    }
}