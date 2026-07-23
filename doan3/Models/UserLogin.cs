using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace doan3.Models
{
    [Serializable] 
    public class UserLogin
    {

            public int UserID { get; set; }
            public string FullName { get; set; }
            public string UserName { set; get; }
            public string GroupID { set; get; }
    }
}