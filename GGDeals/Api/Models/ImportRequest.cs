using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GGDeals.Api.Models
{
	public class ImportRequest
	{
		public string Version => "v1";

		public string Token { get; set; }

		public string Data { get; set; }
	}
}