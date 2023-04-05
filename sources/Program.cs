using Newtonsoft.Json;

if (args.Length < 1)
{
    Console.WriteLine("Set input filename: JsonParser.exe input.txt");
    Console.ReadLine();
    return;
}

var input = args[0];
if (!File.Exists(input))
{
    Console.WriteLine(input + " is not found");
    Console.ReadLine();
    return;
}
var lines = File.ReadAllLines(input);

Item item = new Item() { name = "", tagType = "Provider", tags = new List<object>() };
Item item1 = new Item() { name = "", tagType = "Alarm", tags = new List<object>() };

foreach (var line in lines)
{    
    if (line.StartsWith(";"))
    {
        continue;
    }
    var words = line.Split(',');
    if (words.Length < 3)
    {
        continue;
    }

    string name = words[0];
    string alarm = words[1];
    string address = words[2];

    if (string.IsNullOrEmpty(name))
        continue;

    if (name == "Name")
        continue;

    List<Alarm> alarms = new List<Alarm>
    {
        new Alarm()
        {
            notes = alarm,
            name = name + "_ALM",
        }
    };

    item1.tags.Add(new Tag()
    {        
        opcItemPath = string.Format(@"ns\u003d1;s\u003d[KOW]BinaryOutput:{0}:g10v2i{0}", words[2]),
        name = name,
        alarms = alarms
    }); ;
}

item.tags.Add(item1);

var output = Path.GetFileNameWithoutExtension(input) + ".json";
var json = JsonConvert.SerializeObject(item, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
File.WriteAllText(output, json.Replace("\\\\", "\\"));

Console.WriteLine(output + " was created successfully");


Console.ReadLine();

Item? FindItem(List<object>? items, string name)
{
    if (items == null)
        return null;
    foreach(var o in items)
    {
        var item = o as Item;
        if (item == null)
        {
            continue;
        }
        if (item.name == name)
        {
            return item;
        }
    }
    return null;
}

class Tag
{
    public string? valueSource { get; set; } = "opc";
    public string? opcItemPath { get; set; }
    public string? dataType { get; set; } = "Boolean";
    public List<Alarm>? alarms { get; set; }
    public string? name { get; set; }
    public string? tagType  { get; set; } = "AtomicTag";
    public string? opcServer { get; set; } = "Ignition OPC UA Server";
}

public class Alarm
{    
    public string? notes { get; set; }
    public string? name { get; set; }
    public string? label { get; set; } = "ELEC";
    public string? priority { get; set; } = "Medium";
    public DisplayPath? displayPath { get;set;}
}

public class DisplayPath
{
    public string? bindType { get; set; } = "Expression";
    public string? value { get; set; } = "{notes}";
}


class Item
{
    public string? name { get; set; }
    public string? tagType { get; set; }
    public List<object>? tags { get; set; }
}