using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using PromoPilot.Application.DTOs;

namespace PromoPilot.Infrastructure.Formatters
{

    public class CsvOutputFormatter : TextOutputFormatter
    {
        public CsvOutputFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/csv"));
            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }

        protected override bool CanWriteType(Type type)
        {
            return typeof(CampaignDto).IsAssignableFrom(type) ||
                   typeof(IEnumerable<CampaignDto>).IsAssignableFrom(type);
        }

        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            var response = context.HttpContext.Response;
            var buffer = new StringBuilder();

            if (context.Object is IEnumerable<CampaignDto> campaigns)
            {
                buffer.AppendLine("CampaignID,Name,StartDate,EndDate,TargetProducts,StoreList");
                foreach (var campaign in campaigns)
                {
                    buffer.AppendLine($"{campaign.CampaignId},{campaign.Name},{campaign.StartDate},{campaign.EndDate},{string.Join("|", campaign.TargetProducts)},{string.Join("|", campaign.StoreList)}");
                }
            }
            else if (context.Object is CampaignDto campaign)
            {
                buffer.AppendLine("CampaignID,Name,StartDate,EndDate,TargetProducts,StoreList");
                buffer.AppendLine($"{campaign.CampaignId},{campaign.Name},{campaign.StartDate},{campaign.EndDate},{string.Join("|", campaign.TargetProducts)},{string.Join("|", campaign.StoreList)}");
            }

            await response.WriteAsync(buffer.ToString());
        }
    }
}
