using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdjacencyListDemo.Web.Pages
{
    public class InsertModel : PageModel
    {
        [BindProperty]     
        public string Id { get; set; }
        [BindProperty]
        public string Type { get; set; }
        [BindProperty]
        public string Data { get; set; }
        [BindProperty]
        public string Targets { get; set; }

        private readonly AmazonDynamoDBClient _dynamoClient;

        public InsertModel(AmazonDynamoDBClient dynamoClient)
        {
            _dynamoClient = dynamoClient;
        }

        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            var table = Table.LoadTable(_dynamoClient, "AdjacencyListDemo");

            var node = new Domain.Item
            {
                HASH = Type + "-" + Id,
                RANGE = Type + "-" + Id,
                CreatedDateTime = DateTime.Now,
                Data = Data,
                Type = Type
            };

            await table.PutItemAsync(Document.FromJson(node.ToJson()));

            if(Targets != null)
            {
                //add specified edges
                var targets = Targets.Split(',');
                foreach (var target in targets)
                {
                    var record = await table.GetItemAsync(target, target);
                    var recordType = record["Type"];
                    if (Type == "Person")
                    {
                        if(recordType == "Post" || recordType == "Topic")
                        {
                            var edge = new Domain.Item
                            {
                                HASH = node.HASH,
                                RANGE = target,
                                CreatedDateTime = DateTime.Now,
                                Data = record["Data"],
                                Type = $"{node.Type}|{recordType}"
                            };

                            await table.PutItemAsync(Document.FromJson(edge.ToJson()));
                        }
                        if(recordType == "Topic")
                        {
                            //add graph projected edges
                            var results = (await table.Query(target, new QueryFilter("RANGE", QueryOperator.BeginsWith, "Post")).GetRemainingAsync());

                            foreach (var result in results)
                            {
                                result["HASH"] = node.HASH;
                                result["Type"] = $"{node.Type}|{recordType}|Post";                                
                                await table.PutItemAsync(result);
                            }
                        }
                    }
                    else if (Type == "Topic")
                    {
                        if (recordType == "Person")
                        {
                            //add topic to person partition
                            var edge = new Domain.Item
                            {
                                HASH = record["HASH"],
                                RANGE = node.HASH,
                                CreatedDateTime = DateTime.Now,
                                Data = record["Data"],
                                Type = "Person|Topic"
                            };

                            await table.PutItemAsync(Document.FromJson(edge.ToJson()));
                        }
                        
                        if(recordType == "Post")
                        {
                            //add post to topic partition
                            var edge = new Domain.Item
                            {
                                HASH = node.HASH,
                                RANGE = record["HASH"],
                                CreatedDateTime = DateTime.Now,
                                Data = record["Data"],
                                Type = "Topic|Post"
                            };

                            await table.PutItemAsync(Document.FromJson(edge.ToJson()));
                        }
                    }
                    else if (Type == "Post")
                    {
                        if (recordType == "Person" || recordType == "Topic")
                        {
                            //add post to record partition
                            var edge = new Domain.Item
                            {
                                HASH = record["HASH"],
                                RANGE = node.HASH,
                                CreatedDateTime = DateTime.Now,
                                Data = node.Data,
                                Type = $"{record["Type"]}|Post"
                            };

                            await table.PutItemAsync(Document.FromJson(edge.ToJson()));

                        }

                        if(recordType == "Topic")
                        {
                            //add posts to person partitions subscribed to this topic record
                            var queryFilter = new QueryFilter("RANGE", QueryOperator.Equal, node.HASH);
                            queryFilter.AddCondition("HASH", QueryOperator.BeginsWith, "Person");
                            var queryConfig = new QueryOperationConfig
                            {
                                IndexName = "Range-Hash-index",                                
                                Filter = queryFilter
                            };
                            var results = (await table.Query(queryConfig).GetRemainingAsync());
                            foreach (var result in results)
                            {
                                var edge = new Domain.Item
                                {
                                    HASH = result["HASH"],
                                    RANGE = node.HASH,
                                    CreatedDateTime = DateTime.Now,
                                    Data = node.Data,
                                    Type = "Person|Topic|Post"
                                };

                                await table.PutItemAsync(Document.FromJson(edge.ToJson()));
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("Unknown Type");
                    }
                }
            }

            return RedirectToPage("Index");
        }
    }
}