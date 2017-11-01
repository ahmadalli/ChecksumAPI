using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ChecksumAPI.Controllers
{
    [Produces("application/json")]
    [Route("/MD5")]
    public class MD5Controller : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly CADbContext _context;
        private readonly DbSet<FileChecksum> _set;

        public MD5Controller(IConfiguration configuration, CADbContext context)
        {
            _configuration = configuration;
            _context = context;
            _set = _context.Set<FileChecksum>();
        }

        [HttpGet]
        public IActionResult MD5(string fileUrl, byte? offsetPercent, bool force = false)
        {
            if (offsetPercent > 49)
            {
                return BadRequest("offset must be less than 50 percent");
            }

            Expression<Func<FileChecksum, bool>> predicate = fc => fc.FileUrl == fileUrl && fc.Algorithm == "MD5" && fc.OffsetPercent == offsetPercent;

            if (force || !_set.Any(predicate))
            {
                byte[] result;
                using (var webClient = new System.Net.WebClient())
                {
                    result = webClient.DownloadData(fileUrl);
                }

                for (byte op = 0; op < 50; op++)
                {
                    var offset = result.Length * (offsetPercent ?? Convert.ToInt32(_configuration["MD5FileOffsetPercent"])) / 200;
                    byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(result, offset, result.Length - offset);
                    var checksum = BitConverter
                        .ToString(hash)
                        .Replace("-", string.Empty)
                        .ToLower();

                    _set.Add(new FileChecksum
                    {
                        FileUrl = fileUrl,
                        OffsetPercent = op,
                        Algorithm = "MD5",
                        Checksum = checksum
                    });
                }
            }

            _context.SaveChanges();

            return Ok(_set.First(predicate).Checksum);
        }
    }
}