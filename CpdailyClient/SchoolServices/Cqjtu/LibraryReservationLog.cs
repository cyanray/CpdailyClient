using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpdaily.SchoolServices.Cqjtu
{
    /// <summary>
    /// 图书馆预约记录
    /// </summary>
    public class LibraryReservationLog
    {
        public string? Id { get; set; }
        public string? LibraryName { get; set; }
        public DateTime Date { get; set; }
    }
}
