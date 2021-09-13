using System;

namespace NetCoreDAL
{
    /* It is used for standard return types */
    public class Response
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public dynamic Data { get; set; }
    }
}
