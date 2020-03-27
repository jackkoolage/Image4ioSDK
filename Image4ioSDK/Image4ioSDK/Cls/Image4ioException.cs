using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Image4ioSDK
{
    //public class Image4ioException : Exception
    //{
    //    public Image4ioException(string errorMessage, int errorCode) : base(errorMessage) { }
    //}

    //public class ExceptionCls
    //{
    //    public static Image4ioException CreateException(string errorMesage, int errorCode)
    //    {
    //        return new Image4ioException(errorMesage, errorCode);
    //    }
    //}
    public class Image4ioException : Exception
    {
        public Image4ioException(string errorMesage, int errorCode) : base(errorMesage) { }
    }

}
