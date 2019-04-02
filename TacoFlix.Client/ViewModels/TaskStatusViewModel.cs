using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TacoFlix.Client.ViewModels
{
    public class TaskStatusViewModel
    {
        public string FriendlyTaskName { get; set; }
        public Guid TaskId { get; set; }
        public string Status { get; set; }
    }
}
