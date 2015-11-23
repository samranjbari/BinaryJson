using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using ProtoBuf;
using msgpack = MsgPack.Serialization;

namespace PlayMore
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var people = new List<Person>();
            for (int i = 0; i < 1000; i++)
            {
                var person = new Person
                {
                    Id = i,
                    Name = "Awesome",
                    Age = DateTime.Now.AddDays(20),
                    Address = new Address
                    {
                        Line1 = "Flat 1",
                        Line2 = "The Meadows"
                    }
                };
                people.Add(person);
            }

            var json = JsonConvert.SerializeObject(people);
            Console.WriteLine("JSON size: {0}", Encoding.UTF8.GetBytes(json).Length);
            

            BSON(people);

            ProtoBuf(people);

            MessagePack(people);

            Console.ReadLine();
        }

        private static void BSON<T>(T value) where T : new()
        {
            var watch = new Stopwatch();
            watch.Start();
            byte[] bson;

            using (var ms = new MemoryStream())
            {
                var bsonWriter = new BsonWriter(ms);
                var serializer = new JsonSerializer();
                
                serializer.Serialize(bsonWriter, value);

                bson = ms.ToArray();
                Console.WriteLine("\r\n=========== BSON ===============");
                Console.WriteLine("\r\nBSON value ({1}):::   {1}", BitConverter.ToString(bson), bson.Length);
                
                // going back...
                ms.Seek(0, SeekOrigin.Begin);

                var bsonReader = new BsonReader(ms);
                var originalPerson = serializer.Deserialize(bsonReader);

                watch.Stop();
                Console.WriteLine("Finished in {0}ms", watch.ElapsedMilliseconds);
                Console.WriteLine("\r\n=========== BSON ===============\r\n");
            }
        }

        private static void ProtoBuf<T>(T value)
        {
            var watch = new Stopwatch();
            byte[] protobuf;

            using (var ms = new MemoryStream())
            {
                watch.Start();
                Serializer.Serialize(ms, value);

                protobuf = ms.ToArray();

                Console.WriteLine("\r\n=========== Protobuf ===============");
                Console.WriteLine("\r\nProtobuf value ({1}):::   {1}", BitConverter.ToString(protobuf), protobuf.Length);
                
                // going back...
                ms.Seek(0, SeekOrigin.Begin);

                var newPerson = Serializer.Deserialize<T>(ms);

                watch.Stop();
                Console.WriteLine("Finished in {0}ms", watch.ElapsedMilliseconds);
                Console.WriteLine("\r\n=========== Protobuf ===============\r\n");
            }
        }

        private static void MessagePack<T>(T value)
        {
            
            byte[] msgPacked;

            using (var ms = new MemoryStream())
            {
                var serializer = msgpack.SerializationContext.Default.GetSerializer<T>();

                var watch = new Stopwatch();
                watch.Start();
                serializer.Pack(ms, value);
                
                msgPacked = ms.ToArray();

                Console.WriteLine("\r\n=========== MessagePack ===============");
                Console.WriteLine("\r\nMessagePack value ({1}):::   {1}", BitConverter.ToString(msgPacked), msgPacked.Length);

                // going back...
                ms.Seek(0, SeekOrigin.Begin);
                
                var unpackedObject = serializer.Unpack(ms);
                
                watch.Stop();
                Console.WriteLine("Finished in {0}ms", watch.ElapsedMilliseconds);
                Console.WriteLine("\r\n=========== MessagePack ===============\r\n");
            }
        }

        [ProtoContract] // protobuf requires the attribute
        public class Person
        {
            [ProtoMember(1)]
            public int Id { get; set; }
            [ProtoMember(2)]
            public string Name { get; set; }
            [ProtoMember(3)]
            public DateTime Age { get; set; }
            [ProtoMember(4)]
            public Address Address { get; set; }
        }

        [ProtoContract]
        public class Address
        {
            [ProtoMember(1)]
            public string Line1 { get; set; }
            [ProtoMember(2)]
            public string Line2 { get; set; }
        }
    }
}
