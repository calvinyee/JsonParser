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

var output = input.Replace(Path.GetExtension(input), ".json");
var lines = File.ReadAllLines(input);

Item main = new Item() { name = "", tagType="Provider", tags = new List<object>() };

foreach (var line in lines.Skip(1))
{    
    if (line.StartsWith(";"))
    {
        continue;
    }
    var words = line.Split(',');
    if (words.Length < 17)
    {
        continue;
    }  
  
    string name = words[0].Trim('\"');
    string almNotes = words[15].Trim('\"');
    bool isAlarm = words[1].Trim('\"').StartsWith("1");
    var almName = name;

    Item item = new Item() { name = name, tags = new List<object>() };

    main.tags.Add(item);
  
    if (!string.IsNullOrEmpty(almName))
    {
        almName += "_";
    }
    almName += "ALM";

    List<Alarm>? alarms = null;
    if (isAlarm)
    {      
        alarms = new List<Alarm>
        {
            new Alarm() { name = almName, notes = almNotes,
                priority = (almNotes != null && almNotes.ToLower().Contains("fire")) ? "High" : "Medium",
                displayPath = new DisplayPath() }
        };
    }

    item.tags.Add(new Tag()
    {
        name = name,
        opcItemPath = string.Format(@"ns\u003d1;s\u003d[{0}]{1}","KMA", ""),
        alarms = alarms
    });
}

var json = JsonConvert.SerializeObject(main, Formatting.Indented, new JsonSerializerSettings() 
{ NullValueHandling = NullValueHandling.Ignore });
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
    public string? label { get; set; }
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