// See https://aka.ms/new-console-template for more information

using System.Text.Json;
using ProtoBuf;

Console.WriteLine("Hello, World!");

// var foo = new Foo
// {
//     Name = "m name",
//     Description = "m desc",
//     Id = 1337,
//     Value = 42,
//     NewValue = "ssss"
// };
// using (var file = File.Create("person.bin")) {
//     Serializer.Serialize(file, foo);
// }
Foo newPerson;
using (var file = File.OpenRead("person.bin")) {
    newPerson = Serializer.Deserialize<Foo>(file);
}
Console.WriteLine(JsonSerializer.Serialize(newPerson));
string proto = Serializer.GetProto<Foo>();
Console.WriteLine(proto);
Console.WriteLine("done!");

[ProtoContract]
public class Foo
{
    [ProtoMember(1)]
    public int Id { get; set; }
    [ProtoMember(2)]
    public string Name { get; set; }
    [ProtoMember(3)]
    public double Value { get; set; }
    // [ProtoMember(4)]
    // public string Description { get; set; }
    [ProtoMember(5)]
    public required Bar NewValue { get; set; }
}

[ProtoContract]
public class Bar
{
    [ProtoMember(1)]
    public DateOnly Asdf { get; set; }
}