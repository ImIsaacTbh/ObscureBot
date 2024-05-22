using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Obscura.Commands.EmbedBuilder
{
    public class Embed
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string ThumbnailImageUrl { get; set; } = string.Empty;
        public System.Drawing.Color Color { get; set; }
        public List<Field> Fields { get; set; } = new List<Field>();
        public string Footer { get; set; } = "Obscūrus • Team Unity Development";
        public string ImageURL { get; set; }
        public bool ShowTimestamp { get; set; } = true;
        public ulong ChannelId { get; set; }

        public Discord.Embed Build()
        {
            Discord.EmbedBuilder builder = new Discord.EmbedBuilder();
            builder.WithTitle(Title);
            builder.WithDescription(Description);
            builder.WithColor(new Discord.Color(Color.R, Color.G, Color.B));
            builder.WithThumbnailUrl(ThumbnailImageUrl);
            foreach (Field field in Fields)
            {
                builder.AddField(field.Name, field.Value, field.Inline);
            }

            if (ImageURL != null)
            {
                builder.WithImageUrl(ImageURL);
            }
            if (Footer != null)
            {
                builder.WithFooter(Footer);
            }
            if (ShowTimestamp)
            {
                builder.WithCurrentTimestamp();
            }
            return builder.Build();
        }
    }

    public class Field
    {
        public int id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public bool Inline { get; set; }
    }
}
