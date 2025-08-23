using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PulsNet.Web.Data;
using PulsNet.Web.Services.Snmp;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace PulsNet.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OidBrowserController : ControllerBase
    {
        [HttpGet("walk")]
        public IActionResult Walk(string deviceId, string baseOid)
        {
            if (string.IsNullOrEmpty(deviceId) || string.IsNullOrEmpty(baseOid))
                return BadRequest("Device ID and Base OID are required.");

            try
            {
                // Run snmpwalk process
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "snmpwalk",
                        Arguments = $"-v2c -c public {deviceId} {baseOid}",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                var categories = ProcessSnmpResult(output);
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error during SNMP walk: {ex.Message}");
            }
        }

        private Dictionary<string, List<object>> ProcessSnmpResult(string snmpwalkResult)
        {
            var categories = new Dictionary<string, List<object>>();
            var lines = snmpwalkResult.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                try
                {
                    var parts = line.Split(" = ", 2);
                    if (parts.Length < 2) continue;

                    string oid = parts[0].Trim();
                    string value = parts[1].Trim();

                    // Use OID as "name" for now
                    string name = oid;

                    string category = "Other"; // default category for now

                    if (!categories.ContainsKey(category))
                        categories[category] = new List<object>();

                    categories[category].Add(new { oid, name, value });
                }
                catch
                {
                    // Ignore bad lines
                }
            }

            return categories;
        }
    }
}
