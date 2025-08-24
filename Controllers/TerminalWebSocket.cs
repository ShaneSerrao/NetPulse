using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace PulsNet.Controllers {
  [Authorize(Roles="Admin,SuperAdmin,Operator,User")]
  public static class TerminalWebSocket {
    public static async Task Handle(HttpContext ctx){
      // AuthZ: require authenticated and allowed roles
      var user = ctx.User;
      if (user?.Identity?.IsAuthenticated != true || !(user.IsInRole("SuperAdmin") || user.IsInRole("Admin") || user.IsInRole("Operator") || user.IsInRole("User")))
      { ctx.Response.StatusCode = 401; return; }
      if (!ctx.WebSockets.IsWebSocketRequest) { ctx.Response.StatusCode = 400; return; }
      using var ws = await ctx.WebSockets.AcceptWebSocketAsync();

      // Expect a first message containing JSON: { cmd: "ssh", host:"1.2.3.4", user:"admin" } or { cmd:"telnet", host:"1.2.3.4" }
      var initBuf = new byte[4096]; var init = await ws.ReceiveAsync(initBuf, CancellationToken.None);
      if (init.MessageType != WebSocketMessageType.Text){ await ws.CloseAsync(WebSocketCloseStatus.InvalidMessageType, "text required", CancellationToken.None); return; }
      var initJson = Encoding.UTF8.GetString(initBuf, 0, init.Count);
      string cmd = ""; string host = ""; string? user = null;
      try{
        using var doc = System.Text.Json.JsonDocument.Parse(initJson);
        var r = doc.RootElement;
        cmd = r.GetProperty("cmd").GetString() ?? "";
        host = r.GetProperty("host").GetString() ?? "";
        if (r.TryGetProperty("user", out var u)) user = u.GetString();
      }catch{
        await ws.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "invalid init", CancellationToken.None); return;
      }

      var psi = new ProcessStartInfo{ RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true };
      if (cmd == "ssh"){
        psi.FileName = "/usr/bin/ssh"; if(!string.IsNullOrWhiteSpace(user)) psi.ArgumentList.Add("-l"); if(!string.IsNullOrWhiteSpace(user)) psi.ArgumentList.Add(user!); psi.ArgumentList.Add(host);
      } else if (cmd == "telnet"){
        psi.FileName = "/usr/bin/telnet"; psi.ArgumentList.Add(host);
      } else { await ws.CloseAsync(WebSocketCloseStatus.PolicyViolation, "bad cmd", CancellationToken.None); return; }

      using var proc = Process.Start(psi)!;

      var stdoutTask = Task.Run(async ()=>{
        var buffer = new char[2048]; var sb = new StringBuilder();
        while (!proc.HasExited){
          var chunk = await proc.StandardOutput.ReadAsync(buffer, 0, buffer.Length);
          if (chunk > 0){ var bytes = Encoding.UTF8.GetBytes(buffer, 0, chunk); await ws.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None); }
        }
      });
      var stderrTask = Task.Run(async ()=>{
        var buffer = new char[2048];
        while (!proc.HasExited){
          var chunk = await proc.StandardError.ReadAsync(buffer, 0, buffer.Length);
          if (chunk > 0){ var bytes = Encoding.UTF8.GetBytes(buffer, 0, chunk); await ws.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None); }
        }
      });

      var recv = new byte[2048];
      while (ws.State == WebSocketState.Open){
        var r = await ws.ReceiveAsync(recv, CancellationToken.None);
        if (r.MessageType == WebSocketMessageType.Close) break;
        if (r.MessageType == WebSocketMessageType.Text){
          var text = Encoding.UTF8.GetString(recv, 0, r.Count);
          await proc.StandardInput.WriteAsync(text);
          await proc.StandardInput.FlushAsync();
        }
      }
      try{ proc.Kill(entireProcessTree:true); }catch{}
      await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", CancellationToken.None);
    }
  }
}

