using Newtonsoft.Json;

if (args.Length < 1)
{
    Console.WriteLine("Set input filename and optionally output filename: JsonParser.exe input.txt output.json");
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
var output = Path.GetFileNameWithoutExtension(input) + ".json";
if (args.Length > 1)
{
    output = args[1];
}

var lines = File.ReadAllLines(input);

Item item = new Item() { name = "", tagType = "Provider", tags = new List<object>() };

foreach (var line in lines)
{
    if (line.StartsWith(";"))
    {
        continue;
    }
    var words = line.Split(',');
    if (words.Length < 30)
    {
        continue;
    }
    var names = words[1].Split('\\');
    if (names.Length < 2)
    {
        if (names.Length == 1)
        {
            names = new string[2] { "General", names[0] };
        }
        else
            continue;
    }
    bool newSubItem = false;
    var subItem = FindItem(item.tags, names[0]);
    if (subItem == null)
    {
        newSubItem = true;
        subItem = new Item() { name = names[0], tagType = "Folder" };
    }  
    
    if (subItem.tags == null)
    {
        subItem.tags = new List<object>();
    }     
    if (newSubItem)
    {
        item.tags.Add(subItem);
    }

    var name = names[1];
    if (names.Length > 2)
    {
        name += "_" + names[2];
    }

    var almName = name;
    if (!string.IsNullOrEmpty(almName))
    {
        almName += "_";
    }
    almName += "ALM";

    List<Alarm>? alarms = null;
    if (words[6] != "F")
    {
        var note = words[2];
        alarms = new List<Alarm>
        {
            new Alarm() { name = almName, notes = note, label = words[18],
                priority = (note != null && note.ToLower().Contains("fire")) ? "High" : "Medium",
                displayPath = new DisplayPath() }
        };
    }   

    subItem.tags.Add(new Tag()
    {
        name = name,
        opcItemPath = string.Format(@"ns\u003d1;s\u003d[{0}]{1}", words[22], words[23]),
        alarms = alarms
    });
}


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
    public float setpointA { get; set; } = 1.0f;
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