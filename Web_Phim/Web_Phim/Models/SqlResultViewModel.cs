using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web_Phim.Models
{
    public class SqlResultViewModel
    {
        // SAI: public decimal NewUserID { get; set; }  <-- Nguyên nhân lỗi là đây

        // ĐÚNG: Sửa thành int hoặc long
        public int NewUserID { get; set; }

        public int Result { get; set; }
        public string Message { get; set; }
    }
}