using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lively.NotionGadgetsServer.Models
{
    public class TaskBlock
    {
        public string PlainText { get; set; }
        public string ID { get; set; }
        public bool IsChecked { get; set; }
    }
}
