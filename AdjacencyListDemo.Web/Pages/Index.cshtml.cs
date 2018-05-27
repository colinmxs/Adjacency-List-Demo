using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdjacencyListDemo.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly AmazonDynamoDBClient _dynamoClient;
        public IndexModel(AmazonDynamoDBClient dynamoClient)
        {
            _dynamoClient = dynamoClient;
        }
        public string Index { get; set; }
        public string PartitionKey { get; set; }
        public string RangeKey { get; set; }
        public QueryOperator Operator { get; set; }
        
        public List<Domain.Item> Results { get; set; }
        public double ReadUnitsUsed { get; set; }
        public double ElapsedMilliseconds { get; set; }

        
        public async Task OnGet(string index, string partitionKey, string rangeKey, QueryOperator @operator)
        {
            Index = index;
            PartitionKey = partitionKey;
            RangeKey = rangeKey;
            Operator = @operator;

            var items = new List<Domain.Item>();
            var table = Table.LoadTable(_dynamoClient, "AdjacencyListDemo");
            var stopwatch = new Stopwatch();

            if (string.IsNullOrEmpty(PartitionKey) && string.IsNullOrEmpty(RangeKey))
            {
                var scanRequest = new ScanRequest(table.TableName);
                stopwatch.Start();
                var scanResponse = await _dynamoClient.ScanAsync(scanRequest);
                stopwatch.Stop();
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
                //ReadUnitsUsed = scanResponse.ConsumedCapacity.CapacityUnits;
                var results = scanResponse.Items;
                foreach (var result in results)
                {
                    items.Add(new Domain.Item
                    {
                        HASH = result.GetValueOrDefault("HASH").S,
                        RANGE = result.GetValueOrDefault("RANGE").S,
                        CreatedDateTime = DateTime.Parse(result.GetValueOrDefault("CreatedDateTime").S),
                        Data = result.GetValueOrDefault("Data").S,
                        Type = result.GetValueOrDefault("Type").S
                    });
                }
                Results = items;
            }
            else
            {


                var queryConfig = new QueryOperationConfig();
                var queryFilter = new QueryFilter();

                if (!string.IsNullOrEmpty(Index))
                {
                    queryConfig.IndexName = Index;
                    if (Index == "Range-Hash-index")
                    {
                        queryFilter.AddCondition("RANGE", QueryOperator.Equal, PartitionKey);
                        if (!string.IsNullOrEmpty(RangeKey))
                        {
                            queryFilter.AddCondition("HASH", Operator, RangeKey);
                        }
                    }
                    else
                    {
                        queryFilter.AddCondition("Type", QueryOperator.Equal, PartitionKey);
                        if (!string.IsNullOrEmpty(RangeKey))
                        {
                            queryFilter.AddCondition("Data", Operator, RangeKey);
                        }
                    }
                }
                else
                {
                    queryFilter.AddCondition("HASH", QueryOperator.Equal, PartitionKey);
                    if (!string.IsNullOrEmpty(RangeKey))
                    {
                        queryFilter.AddCondition("RANGE", Operator, "Person");
                    }
                }
                queryConfig.Filter = queryFilter;

                var query = table.Query(queryConfig);
                var results = await query.GetRemainingAsync();
                foreach (var result in results)
                {
                    items.Add(new Domain.Item
                    {
                        HASH = result["HASH"],
                        RANGE = result["RANGE"],
                        CreatedDateTime = DateTime.Parse(result["CreatedDateTime"]),
                        Data = result["Data"],
                        Type = result["Type"]
                    });
                }
                Results = items;
            }
        }
    }
}
