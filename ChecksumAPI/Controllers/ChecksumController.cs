﻿using System;
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
    [Route("/")]
    public class ChecksumController : Controller
    {
        private readonly CADbContext _context;
        private readonly DbSet<FileChecksum> _set;

        public ChecksumController(CADbContext context)
        {
            _context = context;
            _set = _context.Set<FileChecksum>();
        }

        [HttpGet]
        public IActionResult MD5(string fileUrl, byte? offsetPercent, string algorithm = "MD5", bool force = false)
        {
            if (offsetPercent > 100)
            {
                return BadRequest("offset must be less than 100 percent");
            }

            Expression<Func<FileChecksum, bool>> predicate = fc => fc.FileUrl == fileUrl && fc.Algorithm == "MD5" && fc.OffsetPercent == offsetPercent;

            if (force || !_set.Any(predicate))
            {
                byte[] result;
                using (var webClient = new System.Net.WebClient())
                {
                    result = webClient.DownloadData(fileUrl);
                }

                for (byte op = 0; op < 100; op++)
                {
                    var offset = result.Length * op / 200;
                    byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName(algorithm)).ComputeHash(result, offset, result.Length - offset);
                    var checksum = BitConverter
                        .ToString(hash)
                        .Replace("-", string.Empty)
                        .ToLower();

                    _set.AddOrUpdate(new FileChecksum
                    {
                        FileUrl = fileUrl,
                        OffsetPercent = op,
                        Algorithm = algorithm,
                        Checksum = checksum
                    });
                }
            }

            _context.SaveChanges();

            return Ok(_set.First(predicate).Checksum);
        }
    }
}