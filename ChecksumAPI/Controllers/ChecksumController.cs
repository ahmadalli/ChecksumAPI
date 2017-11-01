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
using Microsoft.Extensions.Logging;

namespace ChecksumAPI.Controllers
{
    [Produces("application/json")]
    [Route("/")]
    public class ChecksumController : Controller
    {
        private readonly CADbContext _context;
        private readonly DbSet<FileChecksum> _set;
        private readonly ILogger _logger;

        public ChecksumController(CADbContext context, ILoggerFactory loggerFactory)
        {
            _context = context;
            _set = _context.Set<FileChecksum>();
            _logger = loggerFactory.CreateLogger(nameof(ChecksumController));
        }

        [HttpGet]
        public IActionResult Get(string fileUrl, byte offsetPercent = 0, string algorithm = "MD5", bool force = false)
        {
            _logger.LogInformation("ChecksumController - Get - init");
            if (offsetPercent > 50)
            {
                return BadRequest("offset must be less than 50 percent");
            }

            if (!isValidUrl(fileUrl))
            {
                return BadRequest("the url is not valid");
            }

            Expression<Func<FileChecksum, bool>> predicate = fc => fc.FileUrl == fileUrl && fc.Algorithm == algorithm && fc.OffsetPercent == offsetPercent;

            if (force || !_set.Any(predicate))
            {
                _logger.LogInformation("ChecksumController - Get - downloading");

                byte[] result;
                using (var webClient = new System.Net.WebClient())
                {
                    result = webClient.DownloadData(fileUrl);
                }

                if (!_set.Any(fc => fc.FileUrl == fileUrl && fc.Algorithm == algorithm))
                {
                    for (byte op = 0; op < 50;)
                    {
                        _logger.LogInformation($"ChecksumController - Get - computing hash for op = {op}");

                        try
                        {
                            string checksum = CalculateChecksum(algorithm, result, op);

                            _set.AddOrUpdate(new FileChecksum
                            {
                                FileUrl = fileUrl,
                                OffsetPercent = op,
                                Algorithm = algorithm,
                                Checksum = checksum
                            });

                            _context.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "");
                            continue;
                        }

                        if (op < 10)
                        {
                            op++;
                        }
                        else
                        {
                            op += 5;
                        }
                    }
                }

                if (!_set.Any(predicate))
                {
                    string checksum = CalculateChecksum(algorithm, result, offsetPercent);

                    _set.AddOrUpdate(new FileChecksum
                    {
                        FileUrl = fileUrl,
                        OffsetPercent = offsetPercent,
                        Algorithm = algorithm,
                        Checksum = checksum
                    });

                    _context.SaveChanges();
                }
            }

            _context.SaveChanges();

            _logger.LogInformation($"ChecksumController - Get - end");

            return Ok(_set.First(predicate).Checksum);
        }

        private static string CalculateChecksum(string algorithm, byte[] result, byte offsetPercent)
        {
            var offset = result.Length * offsetPercent / 200;
            byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName(algorithm)).ComputeHash(result, offset, result.Length - offset);
            var checksum = BitConverter
                .ToString(hash)
                .Replace("-", string.Empty)
                .ToLower();
            return checksum;
        }

        private bool isValidUrl(string url)
        {
            bool result = Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            return result;
        }
    }
}