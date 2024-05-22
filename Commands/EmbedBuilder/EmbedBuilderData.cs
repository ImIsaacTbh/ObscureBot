using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Obscura.Commands.EmbedBuilder
{
    public class EmbedBuilderData
    {
        public int embedID { get; set; }
        public int numberOfFields { get; set; }

        public Embed embed { get; set; } = new();
    }
}
