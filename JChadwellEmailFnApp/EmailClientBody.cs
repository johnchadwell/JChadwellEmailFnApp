using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JChadwellEmailFnApp
{
    public class EmailClientBody
    {
        public string subject { get; set; }
        public string sender { get; set; }
        public string recipient { get; set; }
        public string htmlContent { get; set; }
        public string textContent { get; set; }

    }
}
