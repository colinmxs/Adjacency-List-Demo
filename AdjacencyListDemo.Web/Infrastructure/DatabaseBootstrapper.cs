using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdjacencyListDemo.Web.Infrastructure
{
    public class DatabaseBootstrapper
    {
        public void Run()
        {
            var dbClient = new AmazonDynamoDBClientProvider().Get();

            var typeDataIndex = new GlobalSecondaryIndex
            {
                IndexName = "Type-Data-index",
                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = 1,
                    WriteCapacityUnits = 1
                },
                Projection = new Projection
                {
                    ProjectionType = ProjectionType.ALL
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement
                    {
                        AttributeName = "Type",
                        KeyType = KeyType.HASH
                    },
                    new KeySchemaElement
                    {
                        AttributeName = "Data",
                        KeyType = KeyType.RANGE
                    }
                }                
            };

            var rangeHashIndex = new GlobalSecondaryIndex
            {
                IndexName = "Range-Hash-index",
                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = 1,
                    WriteCapacityUnits = 1
                },
                Projection = new Projection
                {
                    ProjectionType = ProjectionType.ALL
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement
                    {
                        AttributeName = "RANGE",
                        KeyType = KeyType.HASH
                    },
                    new KeySchemaElement
                    {
                        AttributeName = "HASH",
                        KeyType = KeyType.RANGE
                    }
                }
            };

            var createTableRequest = new CreateTableRequest(
                    tableName: "AdjacencyListDemo",
                    keySchema: new List<KeySchemaElement>
                    {
                        new KeySchemaElement
                        {
                            AttributeName = "HASH",
                            KeyType = KeyType.HASH
                        },
                        new KeySchemaElement
                        {
                            AttributeName = "RANGE",
                            KeyType = KeyType.RANGE
                        }
                    },
                    attributeDefinitions: new List<AttributeDefinition>
                    {
                        new AttributeDefinition
                        {
                            AttributeName = "HASH",
                            AttributeType = ScalarAttributeType.S
                        },
                        new AttributeDefinition
                        {
                            AttributeName = "RANGE",
                            AttributeType = ScalarAttributeType.S
                        },
                        new AttributeDefinition
                        {
                            AttributeName = "Type",
                            AttributeType = ScalarAttributeType.S
                        },
                        new AttributeDefinition
                        {
                            AttributeName = "Data",
                            AttributeType = ScalarAttributeType.S
                        }
                    },
                    provisionedThroughput: new ProvisionedThroughput
                    {
                        ReadCapacityUnits = 1,
                        WriteCapacityUnits = 1
                    }
                )
            {
                GlobalSecondaryIndexes = new List<GlobalSecondaryIndex> { typeDataIndex, rangeHashIndex }
            };
            

            var result = dbClient.CreateTableAsync(createTableRequest).Result;
        }
    }
}
