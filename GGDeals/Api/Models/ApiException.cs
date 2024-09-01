using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GGDeals.Api.Models
{
    public class ApiException:Exception
    {
        public ApiException(string message)
        {
            
        }
    }
}
