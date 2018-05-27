using Amazon.DynamoDBv2;
using Amazon.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdjacenyListDemo.Web.Infrastructure
{
    public class AmazonDynamoDBClientProvider
    {
        public AmazonDynamoDBClient Get()
        {
            var credentials = new BasicAWSCredentials(accessKey: "fake-access-key", secretKey: "fake-secret-key");
            var config = new AmazonDynamoDBConfig
            {
                ServiceURL = "http://localhost:5153"
            };
            var client = new AmazonDynamoDBClient(credentials, config);
            return client;
        }
    }
}
