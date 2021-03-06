using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml.Linq;

using FeedReaders.Extensions;
using FeedReaders.Models;

namespace FeedReaders
{
    /// <summary>
    /// This represents the feed reader entity for YouTube.
    /// </summary>
    public class YouTubeFeedReader : FeedReader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="YouTubeFeedReader"/> class.
        /// </summary>
        public YouTubeFeedReader()
            : base()
        {
        }

        /// <inheritdoc />
        public override async Task<List<FeedItem>> GetFeedItemsAsync(FeedReaderContext context)
        {
            var feed = await this.LoadFeedAsync(context.FeedUri).ConfigureAwait(false);

            var items = feed.Items
                            .FilterIncluded(context.PrefixesIncluded)
                            .FilterExcluded(context.PrefixesExcluded)
                            .OrderByDescending(p => p.PublishDate)
                            .Take(context.NumberOfFeedItems)
                            .ToList();

            var feedItems = items.Select(p => this.BuildFeedItem(p)).ToList();

            return feedItems;
        }

        /// <inheritdoc />
        public override async Task<FeedItem> GetFeedItemAsync(FeedReaderContext context)
        {
            var feedItems = await this.GetFeedItemsAsync(context).ConfigureAwait(false);

            var index = this.GetSkipNumber(context.NumberOfFeedItems > feedItems.Count ? feedItems.Count : context.NumberOfFeedItems, context.IsRandom);
            var feedItem = feedItems.Skip(index).Take(1).SingleOrDefault();

            return feedItem;
        }

        /// <inheritdoc />
        protected override FeedItem BuildFeedItem(SyndicationItem item)
        {
            var group = item.ElementExtensions.FirstOrDefault(p => p.OuterName == "group").GetObject<XElement>();
            var title = group.Elements().SingleOrDefault(p => p.Name.LocalName == "title").Value;
            var description = group.Elements().SingleOrDefault(p => p.Name.LocalName == "description").Value;
            var thumbnailLink = group.Elements().SingleOrDefault(p => p.Name.LocalName == "thumbnail").Attribute("url").Value;
            var datePublished = item.PublishDate;

            var feedItem = new FeedItem()
            {
                Title = item.Title.Text,
                Description = description,
                Link = item.Links.First().Uri.ToString(),
                ThumbnailLink = thumbnailLink,
                DatePublished = datePublished,
            };

            return feedItem;
        }
    }
}