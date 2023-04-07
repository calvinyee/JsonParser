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


Dictionary<string, Item> itemDict = new Dictionary<string, Item>();

foreach (var line in lines)
{    
    if (line.StartsWith("-"))
    {
        continue;
    }
    var words = line.Split('|', StringSplitOptions.TrimEntries);
    if (words.Length < 8)
    {
        continue;
    }

    string fip = words[0];
    string dev = words[4];

    if (fip == "fip")
        continue;
    
    if (!itemDict.ContainsKey(fip))
    {
        itemDict.Add(fip, new Item() { name = "", tagType = "Provider", tags = new List<object>() });
        itemDict[fip].tags.Add(new Item() { name = "Alarm", tagType = "Folder", tags = new List<object>() });
    }

    Item item = itemDict[fip];
    Item alarmSubItem = item.tags[0] as Item;

    List<Alarm> alarms = new List<Alarm>
    {
        new Alarm()
        {
            notes = words[7],
            name = dev + "_ALM",
            label = words[6]
        }
    };

    alarmSubItem.tags.Add(new Tag()
    {        
        opcItemPath = string.Format(@"ns\u003d1;s\u003d[{0}]BinaryOutput:{1}:g10v2i{1}", fip, words[2]),
        name = dev,
        alarms = alarms
    }); ;
}
foreach (var pair in itemDict)
{    
    var json = JsonConvert.SerializeObject(pair.Value, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
    File.WriteAllText(pair.Key + ".json", json.Replace("\\\\", "\\"));

    Console.WriteLine(pair.Key + ".json" + " was created successfully");
}


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