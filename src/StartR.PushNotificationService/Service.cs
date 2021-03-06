﻿using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Hubs;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace StartR.PushNotificationService
{
    public class Service : IService
    {
        private HubConnection _cn;
        private IHubProxy _proxy;
        public async void Start()
        {
            _cn = new HubConnection("http://localhost:29141/");
            _cn.Credentials = new System.Net.NetworkCredential("", "", "");
            
            _proxy = _cn.CreateHubProxy("qualification");
            await _cn.Start();
            Console.WriteLine("Connection started for SignalR");

            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare("StartR.SignalR", true, false, false, null);
                    var consumer = new QueueingBasicConsumer(channel);
                    channel.BasicConsume("StartR.SignalR", false, consumer);
                    Console.WriteLine(" [*] Waiting for push notification messages." +
                                             "To exit press CTRL+C");
                    int counter = 0;
                    while (true)
                    {
                        var ea = (BasicDeliverEventArgs)consumer.Queue.Dequeue();
                        counter++;
                        if (counter % 1000 == 00) Console.WriteLine("Processed " + counter);
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);
                        Console.WriteLine("Messaged received: " + message.Substring(0, 25));
                        _proxy.Invoke("updateQualification", message);
                        channel.BasicAck(ea.DeliveryTag, false);
                    }
                }
            }
        }

        public void Stop()
        {

        }
    }
}
