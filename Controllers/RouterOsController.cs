using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace PulsNet.Controllers {
  [ApiController]
  [Route("api/[controller]")]
  [Authorize(Roles="Admin,SuperAdmin,Operator,User")]
  public sealed class RouterOsController : ControllerBase {
    public sealed class Conn { public string Host { get; set; } = ""; public string User { get; set; } = ""; }

    private static async Task<IActionResult> Run(string host, string user, params string[] command){
      if(string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(user)) return new BadRequestObjectResult("host/user required");
      var psi = new ProcessStartInfo{ FileName = "/usr/bin/ssh", RedirectStandardOutput = true, RedirectStandardError = true };
      psi.ArgumentList.Add("-o"); psi.ArgumentList.Add("BatchMode=yes");
      psi.ArgumentList.Add("-o"); psi.ArgumentList.Add("StrictHostKeyChecking=no");
      psi.ArgumentList.Add("-l"); psi.ArgumentList.Add(user);
      psi.ArgumentList.Add(host);
      foreach (var c in command) psi.ArgumentList.Add(c);
      try{
        using var p = Process.Start(psi)!;
        var stdout = await p.StandardOutput.ReadToEndAsync();
        var stderr = await p.StandardError.ReadToEndAsync();
        await p.WaitForExitAsync();
        if(p.ExitCode!=0) return new ObjectResult(new{ error=stderr.Trim()}){ StatusCode=502 };
        var lines = stdout.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(x=>x.Trim()).ToArray();
        return new OkObjectResult(new{ lines });
      }catch(Exception ex){ return new ObjectResult(new{ error=ex.Message}){ StatusCode=500 }; }
    }

    [HttpPost("interfaces")] public Task<IActionResult> Interfaces([FromBody] Conn c)=> Run(c.Host, c.User, "/interface/print", "detail", "without-paging");
    [HttpPost("routes")] public Task<IActionResult> Routes([FromBody] Conn c)=> Run(c.Host, c.User, "/ip/route/print", "detail", "without-paging");
    [HttpPost("queues")] public Task<IActionResult> Queues([FromBody] Conn c)=> Run(c.Host, c.User, "/queue/simple/print", "detail", "without-paging");
    [HttpPost("logs")] public Task<IActionResult> Logs([FromBody] Conn c)=> Run(c.Host, c.User, "/log/print", "without-paging");
  }
}

