using KafkaFlow.Serializer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pawnshop.Web.Extensions.Helpers;
using System.Text.Json;
using KafkaFlow;
using Pawnshop.Web.Handlers;

namespace Pawnshop.Web.Kafka
{
    public static class KafkaInitializerHelper
    {
        public static void Initialize(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            string bootstrapServer = configuration.GetValue<string>("KafkaOptions:BootstrapUrl");
            string consumerGroup = configuration.GetValue<string>("KafkaOptions:ConsumerGroupName");
            string kafkaTopicPrefix = configuration.GetValue<string>("KafkaOptions:KafkaTopicPrefix");
            bool initConsumer = configuration.GetValue<bool>("KafkaOptions:InitConsumer");
            if (string.IsNullOrEmpty(bootstrapServer))
                bootstrapServer = "192.168.12.33:19092";
            if (string.IsNullOrEmpty(consumerGroup))
                consumerGroup = "DefaultGroupName";

            var jsonCoreSerializer = new JsonCoreSerializer(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new NullableDateTimeConverter() }
            });
            var jsonCoreDeserializer = new JsonCoreDeserializer(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new NullableDateTimeConverter() }
            });

            if (initConsumer)
            {
                serviceCollection.AddKafka(kafka => kafka
                .UseConsoleLog()
                .AddCluster(cluster => cluster
                    .WithBrokers(new[] { bootstrapServer }) // kafka url 
                    .CreateTopicIfNotExists($"{kafkaTopicPrefix}ApplicationOnline", 4, 1) // AutoCreateTopic if not exists topic partions = 4 * N - where n service numbers
                    .CreateTopicIfNotExists($"{kafkaTopicPrefix}OnlineTask", 4, 1) // AutoCreateTopic if not exists topic partions = 4 * N - where n service numbers
                    .CreateTopicIfNotExists($"{kafkaTopicPrefix}Contract", 4, 1)
                    .CreateTopicIfNotExists($"{kafkaTopicPrefix}Notification", 4, 1)
                    .AddConsumer(consumer => consumer
                        .Topic($"{kafkaTopicPrefix}ApplicationOnline")
                        .Topic($"{kafkaTopicPrefix}OnlineTask")// In this sections need set topics to listen
                        .Topic($"{kafkaTopicPrefix}Contract")
                        .Topic($"{kafkaTopicPrefix}Notification")
                        .Topic($"{kafkaTopicPrefix}NpckEsignDocument")
                        .WithGroupId(consumerGroup)
                        .WithBufferSize(4)
                        .WithWorkersCount(10)
                        .WithAutoOffsetReset(AutoOffsetReset.Earliest) // New consumer groups join to topics from first message
                        .AddMiddlewares(middlewares => middlewares
                            .AddDeserializer(resolver => jsonCoreDeserializer)
                            .AddTypedHandlers(h =>
                            {
                                h.AddHandler<ApplicationOnlineStatusChangedEventHandler>();
                                h.AddHandler<OnlineTaskCreatedEventHandler>();
                            })))
                    .AddProducer($"Contract", producer => producer
                        .AddMiddlewares(middlewares => middlewares
                            .AddSerializer(resolver => jsonCoreSerializer))
                        .DefaultTopic($"{kafkaTopicPrefix}Contract"))
                    .AddProducer($"Notification", producer => producer
                        .AddMiddlewares(middlewares => middlewares
                            .AddSerializer(resolver => jsonCoreSerializer))
                        .DefaultTopic($"{kafkaTopicPrefix}Notification"))
                    .AddProducer($"ApplicationOnline", producer => producer
                        .AddMiddlewares(middlewares => middlewares
                            .AddSerializer(resolver => jsonCoreSerializer))
                        .DefaultTopic($"{kafkaTopicPrefix}ApplicationOnline"))
                    .AddProducer("OnlineTask", producer => producer
                        .AddMiddlewares(middlewares => middlewares
                            .AddSerializer(resolver => jsonCoreSerializer))
                        .DefaultTopic($"{kafkaTopicPrefix}OnlineTask"))
                    .AddProducer("NpckEsignDocument", producer => producer
                        .AddMiddlewares(middlewares => middlewares
                            .AddSerializer(resolver => jsonCoreSerializer))
                        .DefaultTopic($"{kafkaTopicPrefix}NpckEsignDocument")))
                );
            }
            else
            {
                serviceCollection.AddKafka(kafka => kafka
                .UseConsoleLog()
                .AddCluster(cluster => cluster
                    .WithBrokers(new[] { bootstrapServer }) // kafka url 
                    .CreateTopicIfNotExists($"{kafkaTopicPrefix}ApplicationOnline", 4, 1) // AutoCreateTopic if not exists topic partions = 4 * N - where n service numbers
                    .CreateTopicIfNotExists($"{kafkaTopicPrefix}OnlineTask", 4, 1) // AutoCreateTopic if not exists topic partions = 4 * N - where n service numbers
                    .CreateTopicIfNotExists($"{kafkaTopicPrefix}Contract", 4, 1)
                    .CreateTopicIfNotExists($"{kafkaTopicPrefix}Notification", 4, 1)
                    .CreateTopicIfNotExists($"{kafkaTopicPrefix}NpckEsignDocument", 4, 1)
                    .AddProducer($"Contract", producer => producer
                        .AddMiddlewares(middlewares => middlewares
                            .AddSerializer(resolver => jsonCoreSerializer))
                        .DefaultTopic($"{kafkaTopicPrefix}Contract"))
                    .AddProducer($"Notification", producer => producer
                        .AddMiddlewares(middlewares => middlewares
                            .AddSerializer(resolver => jsonCoreSerializer))
                        .DefaultTopic($"{kafkaTopicPrefix}Notification"))
                    .AddProducer($"ApplicationOnline", producer => producer
                        .AddMiddlewares(middlewares => middlewares
                            .AddSerializer(resolver => jsonCoreSerializer))
                        .DefaultTopic($"{kafkaTopicPrefix}ApplicationOnline"))
                    .AddProducer("OnlineTask", producer => producer
                        .AddMiddlewares(middlewares => middlewares
                            .AddSerializer(resolver => jsonCoreSerializer))
                        .DefaultTopic($"{kafkaTopicPrefix}OnlineTask"))
                    .AddProducer("NpckEsignDocument", producer => producer
                        .AddMiddlewares(middlewares => middlewares
                            .AddSerializer(resolver => jsonCoreSerializer))
                        .DefaultTopic($"{kafkaTopicPrefix}NpckEsignDocument")))
                );
            }
            serviceCollection.AddHostedService<KafkaBusHostedService>();
        }
    }
}
