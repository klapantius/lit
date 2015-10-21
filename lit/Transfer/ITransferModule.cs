using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lit
{
    public interface ITransferModule: IDisposable
    {
        void Init();
        void Start();
        void ReceiveChanges(IDictionary<string, string> record);
        void Stop();
    }
}
