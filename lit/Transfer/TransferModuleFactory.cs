using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lit
{
    public class TransferModuleFactory
    {
        public ITransferModule Create()
        {
            return new HttpTransferModule();
        }

    }
}
