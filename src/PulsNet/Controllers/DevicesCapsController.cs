using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Mvc; using PulsNet.Data;
namespace PulsNet.Controllers {
	[ApiController][Route("api/devices")][Authorize(Roles="Admin,SuperAdmin")]
	public sealed class DevicesCapsController : ControllerBase {
		private readonly Db _db; public DevicesCapsController(Db db){ _db=db; }
		public sealed class IndexBody{ public int InterfaceIndex{get;set;} }
		[HttpPost("{id}/interface-index")] public async Task<IActionResult> SetIndex(int id,[FromBody] IndexBody b){ await _db.Exec("UPDATE devices SET interface_index=@i WHERE id=@id", new{ id, i=b.InterfaceIndex}); return Ok(); }
		public sealed class CapsBody{ public bool CapDownEnabled{get;set;} public int? CapDownMbps{get;set;} public bool CapUpEnabled{get;set;} public int? CapUpMbps{get;set;} }
		[HttpPost("{id}/caps")] public async Task<IActionResult> SetCaps(int id,[FromBody] CapsBody b){
			await _db.Exec("UPDATE devices SET cap_down_enabled=@d, cap_down_mbps=@dm, cap_up_enabled=@u, cap_up_mbps=@um WHERE id=@id", new{ id, d=b.CapDownEnabled, dm=b.CapDownMbps, u=b.CapUpEnabled, um=b.CapUpMbps});
		 return Ok();
		}
	}
}
