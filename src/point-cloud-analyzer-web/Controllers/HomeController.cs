﻿using CliWrap;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using point_cloud_analyzer_web.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace point_cloud_analyzer_web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _clientFactory;
        public HomeController(ILogger<HomeController> logger,
            IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _clientFactory = clientFactory;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("FileUpload")]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> FileUpload(IFormFile file)
        {
            var root = System.IO.Directory.GetCurrentDirectory();
            var upload = Path.Combine(root, "upload", file.FileName);
            var fileName = file.FileName.Split('.')[0];
            var redirect = "output\\" + fileName + ".html";

            Directory.CreateDirectory(Path.Combine(root, "upload"));
            using (Stream fileStream = new FileStream(upload, FileMode.Create))
            {
                file.CopyTo(fileStream);
            }

            var converterPath = Path.Combine(root, "PotreeConverter", "Windows", "PotreeConverter.exe");
            var filePath = Path.Combine(root, "upload", file.FileName);
            var outputPath = Path.Combine(root, "wwwroot", "output", fileName);

            Exec($"chmod +x {converterPath}");
            Exec($"chmod +x {filePath}");

            Console.WriteLine(converterPath);
            Console.WriteLine(filePath);
            Console.WriteLine(outputPath);

            if (System.IO.File.Exists(converterPath))
            {
                Console.WriteLine("Converter Exists");
            }

            if (System.IO.File.Exists(filePath))
            {
                Console.WriteLine("upload Exists");
            }

            await Cli.Wrap("wine")
                .WithArguments($"{converterPath} {filePath} -o {outputPath} --output-format LAZ")
                .ExecuteAsync();


            string text = System.IO.File.ReadAllText(Path.Combine(root, "PotreeConverter", "template.html"));
            text = text.Replace("[OutputFilePath]", fileName + "/cloud.js");
            System.IO.File.WriteAllText(outputPath + ".html", text);

            System.IO.File.Delete(upload);

            return Redirect(redirect);
        }

        public IActionResult Registration()
        {
            return View("Registration");
        }

        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> Registration(string ply1, string ply2, string output)
        {

            var request = new HttpRequestMessage(HttpMethod.Get,
             $"service1/main?ply1={ply1}&ply2={ply2}&output={output}");

            var client = _clientFactory.CreateClient();

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                using var responseStream = await response.Content.ReadAsStreamAsync();
                return Ok();
            }

            return BadRequest();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private static void Exec(string cmd)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\""
                }
            };

            process.Start();
            process.WaitForExit();
        }
    }
}
