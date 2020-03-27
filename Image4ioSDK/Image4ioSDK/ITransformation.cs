using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Image4ioSDK
{
    public interface ITransformation
    {
        string ToWebP(int Quality, int? Width = null, int? Height = null);
        string Resize(int Quality, int? Width = null, int? Height = null);
        string Compress(int Quality);
    }
}
